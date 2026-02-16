using System.Collections.Generic;
using System.Linq;
using FightLord.Core.Enums;

namespace FightLord.Core.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Role Role { get; set; }
        public List<Card> HandCards { get; set; } = new List<Card>();
        public bool IsRobot { get; set; }

        public Player(int id, string name, bool isRobot = false)
        {
            Id = id;
            Name = name;
            IsRobot = isRobot;
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            HandCards.AddRange(cards);
            SortHandCards();
        }

        public void RemoveCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                var cardToRemove = HandCards.FirstOrDefault(c => c.Id == card.Id);
                if (cardToRemove != null)
                {
                    HandCards.Remove(cardToRemove);
                }
            }
        }

        private void SortHandCards()
        {
            // Sort by Weight descending, then Suit
            HandCards.Sort((a, b) =>
            {
                int weightCompare = b.Weight.CompareTo(a.Weight);
                if (weightCompare != 0)
                    return weightCompare;
                return b.Suit.CompareTo(a.Suit);
            });
        }
    }
}
