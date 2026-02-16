using FightLord.Application.Commands;
using FightLord.Application.DTOs;
using FightLord.Core.Interfaces;
using FightLord.WebAPI.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace FightLord.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IRoomService _roomService;

        public GameController(IMediator mediator, IHubContext<GameHub> hubContext, IRoomService roomService)
        {
            _mediator = mediator;
            _hubContext = hubContext;
            _roomService = roomService;
        }

        [HttpPost("play")]
        public async Task<IActionResult> PlayCard([FromBody] PlayCardRequest request)
        {
            var command = new PlayCardCommand(request.PlayerId, request.CardIds);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid move or not your turn.");
            
            await NotifyRoom();
            return Ok(new { success = true });
        }

        [HttpPost("bid")]
        public async Task<IActionResult> Bid([FromBody] BidRequest request)
        {
            var command = new BidCommand(request.PlayerId, request.Score);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid bid or not your turn.");

            await NotifyRoom();
            return Ok(new { success = true });
        }

        [HttpPost("pass")]
        public async Task<IActionResult> Pass([FromBody] PassRequest request)
        {
            var command = new PassCommand(request.PlayerId);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid pass or not your turn.");

            await NotifyRoom();
            return Ok(new { success = true });
        }

        private async Task NotifyRoom()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr != null && Guid.TryParse(userIdStr, out var userId))
            {
                var rooms = await _roomService.GetRoomsAsync();
                var room = rooms.FirstOrDefault(r => r.Players.Any(p => p.Id == userId));
                if (room != null)
                {
                    // Notify clients in the room that game state has updated
                    // They should probably fetch the latest state
                    await _hubContext.Clients.Group(room.Id.ToString()).SendAsync("GameUpdated", new { timestamp = DateTime.UtcNow });
                }
            }
        }
    }
}
