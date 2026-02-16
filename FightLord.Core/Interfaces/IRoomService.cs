using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FightLord.Core.Entities;

namespace FightLord.Core.Interfaces
{
    public interface IRoomService
    {
        Task<Room> CreateRoomAsync(string roomName, Guid userId);
        Task<Room> JoinRoomAsync(Guid roomId, Guid userId);
        Task LeaveRoomAsync(Guid roomId, Guid userId);
        Task<List<Room>> GetRoomsAsync();
        Task<Room?> GetRoomAsync(Guid roomId);
    }
}
