using System.Collections.Generic;

namespace FightLord.Application.DTOs
{
    public class PlayCardRequest
    {
        public int PlayerId { get; set; }
        public List<int> CardIds { get; set; } = new List<int>();
    }
}
