using System.Collections.Generic;
using FightLord.Core.Enums;

namespace FightLord.Core.Entities
{
    public class Move
    {
        public int PlayerId { get; set; }
        public List<Card> Cards { get; set; }
        public CardType Type { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }

        public Move(int playerId, List<Card> cards, CardType type, int weight, int length)
        {
            PlayerId = playerId;
            Cards = cards;
            Type = type;
            Weight = weight;
            Length = length;
        }
    }
}
