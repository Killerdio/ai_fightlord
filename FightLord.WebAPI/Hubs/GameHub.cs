using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FightLord.WebAPI.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", Context.ConnectionId);
        }

        public async Task SendMessage(string roomId, string user, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
        }
        
        public async Task UpdateGame(string roomId, object gameState)
        {
            await Clients.Group(roomId).SendAsync("GameUpdated", gameState);
        }
    }
}
