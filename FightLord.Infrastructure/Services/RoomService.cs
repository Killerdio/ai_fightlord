using FightLord.Core.Entities;
using FightLord.Core.Interfaces;
using FightLord.Core.Enums;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FightLord.Infrastructure.Services
{
    public class RoomService : IRoomService
    {
        private readonly FightLordDbContext _context;
        private readonly IDatabase _redis;

        public RoomService(FightLordDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis.GetDatabase();
        }

        public async Task<Room> CreateRoomAsync(string roomName, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = roomName,
                Status = RoomStatus.Waiting,
                Players = new List<User> { user }
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await CacheRoomAsync(room);

            return room;
        }

        public async Task<Room> JoinRoomAsync(Guid roomId, Guid userId)
        {
            var dbRoom = await _context.Rooms.Include(r => r.Players).FirstOrDefaultAsync(r => r.Id == roomId);
            if (dbRoom == null) throw new Exception("Room not found");

            if (dbRoom.Players.Any(p => p.Id == userId)) return dbRoom; // Already joined

            if (dbRoom.Players.Count >= 3) throw new Exception("Room is full");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            dbRoom.Players.Add(user);
            await _context.SaveChangesAsync();
            
            await CacheRoomAsync(dbRoom);
            return dbRoom;
        }

        public async Task LeaveRoomAsync(Guid roomId, Guid userId)
        {
             var dbRoom = await _context.Rooms.Include(r => r.Players).FirstOrDefaultAsync(r => r.Id == roomId);
             if (dbRoom != null)
             {
                 var player = dbRoom.Players.FirstOrDefault(p => p.Id == userId);
                 if (player != null)
                 {
                     dbRoom.Players.Remove(player);
                     await _context.SaveChangesAsync();
                     
                     if (dbRoom.Players.Count == 0)
                     {
                         _context.Rooms.Remove(dbRoom);
                         await _context.SaveChangesAsync();
                         await _redis.KeyDeleteAsync($"room:{roomId}");
                     }
                     else
                     {
                        await CacheRoomAsync(dbRoom);
                     }
                 }
             }
        }

        public async Task<List<Room>> GetRoomsAsync()
        {
             return await _context.Rooms.Include(r => r.Players).ToListAsync();
        }

        public async Task<Room?> GetRoomAsync(Guid roomId)
        {
            var cached = await _redis.StringGetAsync($"room:{roomId}");
            if (!cached.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<Room>(cached.ToString(), GetJsonOptions());
            }

            var room = await _context.Rooms.Include(r => r.Players).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room != null)
            {
                await CacheRoomAsync(room);
            }
            return room;
        }

        private async Task CacheRoomAsync(Room room)
        {
            await _redis.StringSetAsync($"room:{room.Id}", JsonSerializer.Serialize(room, GetJsonOptions()), TimeSpan.FromMinutes(30));
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions 
            { 
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            };
        }
    }
}
