using System.Collections.Generic;
using FightLord.Core.Enums;

namespace FightLord.Core.Entities
{
    public class GameState
    {
        public int Id { get; set; }
        public int LandlordId { get; set; }
        public int CurrentPlayerId { get; set; }
        public int PassCount { get; set; }
        public Dictionary<int, int> Bids { get; set; } = new Dictionary<int, int>();
        public List<int> PlayerIds { get; set; } = new List<int>();
        public List<Card> KittyCards { get; set; } = new List<Card>();
        public Move? LastMove { get; set; }
        public Dictionary<string, int> Multipliers { get; set; } = new Dictionary<string, int>();
        public Phase Phase { get; set; }

        public GameState()
        {
            Phase = Phase.Preparing;
            Multipliers["Base"] = 15; // Base score? Or base multiplier 1? Let's say 1.
            Multipliers["Bomb"] = 0;
            Multipliers["Rocket"] = 0;
            Multipliers["Spring"] = 0;
        }

        public int CalculateTotalMultiplier()
        {
            // Simplified calculation logic
            int total = 1;
            // logic to multiply base by 2^bombs * 2^rocket * 2^spring...
            // This is just a placeholder.
            return total;
        }
    }
}
