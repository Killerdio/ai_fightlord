using FightLord.Core.Enums;

namespace FightLord.Core.Entities
{
    public class Card
    {
        public int Id { get; set; }
        public Rank Rank { get; set; }
        public Suit Suit { get; set; }
        public int Weight => (int)Rank;

        public Card(int id, Rank rank, Suit suit)
        {
            Id = id;
            Rank = rank;
            Suit = suit;
        }

        public override string ToString()
        {
            return $"{Suit} {Rank}";
        }
    }
}
