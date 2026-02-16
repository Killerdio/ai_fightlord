using FightLord.Core.Enums;

namespace FightLord.Core.Entities
{
    public class Room
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public RoomStatus Status { get; set; }
        public List<User> Players { get; set; } = new List<User>();
        public Guid? GameStateId { get; set; }
    }
}
