using FightLord.Application.Commands;
using FightLord.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FightLord.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("play")]
        public async Task<IActionResult> PlayCard([FromBody] PlayCardRequest request)
        {
            var command = new PlayCardCommand(request.PlayerId, request.CardIds);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid move or not your turn.");
            return Ok(new { success = true });
        }

        [HttpPost("bid")]
        public async Task<IActionResult> Bid([FromBody] BidRequest request)
        {
            var command = new BidCommand(request.PlayerId, request.Score);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid bid or not your turn.");
            return Ok(new { success = true });
        }

        [HttpPost("pass")]
        public async Task<IActionResult> Pass([FromBody] PassRequest request)
        {
            var command = new PassCommand(request.PlayerId);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Invalid pass or not your turn.");
            return Ok(new { success = true });
        }
    }
}
