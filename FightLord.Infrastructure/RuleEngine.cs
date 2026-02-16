using System.Collections.Generic;
using System.Linq;
using FightLord.Core.Entities;
using FightLord.Core.Enums;
using FightLord.Core.Interfaces;

namespace FightLord.Infrastructure
{
    public class RuleEngine : IRuleEngine
    {
        public CardType AnalyzeType(List<Card> cards)
        {
            if (cards == null || cards.Count == 0)
                return CardType.Unknown;

            cards = cards.OrderBy(c => c.Rank).ToList();
            int count = cards.Count;

            // Single
            if (count == 1)
                return CardType.Single;

            // Pair
            if (count == 2 && cards[0].Rank == cards[1].Rank)
                return CardType.Pair;

            // Rocket (Joker + BigJoker)
            if (count == 2 && cards[0].Rank == Rank.Joker && cards[1].Rank == Rank.BigJoker)
                return CardType.Rocket;

            // Triple
            if (count == 3 && cards[0].Rank == cards[2].Rank)
                return CardType.Triple;

            // Bomb
            if (count == 4 && cards[0].Rank == cards[3].Rank)
                return CardType.Bomb;

            // TripleWithOne
            if (count == 4)
            {
                // 3 + 1
                if (cards[0].Rank == cards[2].Rank || cards[1].Rank == cards[3].Rank)
                    return CardType.TripleWithOne;
            }

            // TripleWithTwo
            if (count == 5)
            {
                // 3 + 2
                if ((cards[0].Rank == cards[2].Rank && cards[3].Rank == cards[4].Rank) ||
                    (cards[0].Rank == cards[1].Rank && cards[2].Rank == cards[4].Rank))
                    return CardType.TripleWithTwo;
            }

            // More types can be added (Straight, etc.), but prompt asked for basic logic
            // Single, Pair, Triple, Bomb, Rocket are explicitly mentioned.

            return CardType.Unknown;
        }

        public bool CompareMoves(Move newMove, Move lastMove)
        {
            if (lastMove == null) return true; // First move always valid if type is valid (checked elsewhere)

            // Rocket beats everything except Rocket
            if (newMove.Type == CardType.Rocket)
                return true;
            if (lastMove.Type == CardType.Rocket)
                return false;

            // Bomb beats everything except Rocket and bigger Bomb
            if (newMove.Type == CardType.Bomb)
            {
                if (lastMove.Type != CardType.Bomb)
                    return true;
                return newMove.Weight > lastMove.Weight;
            }

            // Same type comparison
            if (newMove.Type != lastMove.Type)
                return false;

            // Must have same length for some types (Single, Pair, Triple, Bomb usually fixed length but Straight varies)
            // For Single, Pair, Triple, Bomb, length is fixed by type definition mostly.
            // But Straight/PairChain need length check.
            if (newMove.Length != lastMove.Length)
                return false;

            return newMove.Weight > lastMove.Weight;
        }

        public List<Move> SearchAvailableMoves(List<Card> hand, Move? lastMove)
        {
            var moves = new List<Move>();
            if (hand == null || hand.Count == 0) return moves;

            // Sort hand for easier processing
            hand = hand.OrderBy(c => c.Rank).ToList();

            // If no last move, return all valid single cards as possible moves (simplified)
            // Or maybe return nothing and let frontend handle it? 
            // The prompt says "SearchAvailableMoves". I should implement basic search.
            
            if (lastMove == null)
            {
                // Return all singles
                foreach (var card in hand.DistinctBy(c => c.Rank))
                {
                    moves.Add(new Move(0, new List<Card> { card }, CardType.Single, (int)card.Rank, 1));
                }
                return moves;
            }

            // Search based on lastMove type
            if (lastMove.Type == CardType.Single)
            {
                foreach (var card in hand.Where(c => (int)c.Rank > lastMove.Weight))
                {
                    moves.Add(new Move(0, new List<Card> { card }, CardType.Single, (int)card.Rank, 1));
                }
            }
            else if (lastMove.Type == CardType.Pair)
            {
                var pairs = hand.GroupBy(c => c.Rank).Where(g => g.Count() >= 2);
                foreach (var pair in pairs)
                {
                    if ((int)pair.Key > lastMove.Weight)
                    {
                        var cards = pair.Take(2).ToList();
                        moves.Add(new Move(0, cards, CardType.Pair, (int)pair.Key, 2));
                    }
                }
            }
            // ... Add logic for Triple, Bomb etc.

            // Always check for Bombs and Rockets if not last move wasn't a Rocket
            if (lastMove.Type != CardType.Rocket)
            {
                // Bombs
                var bombs = hand.GroupBy(c => c.Rank).Where(g => g.Count() == 4);
                foreach (var bomb in bombs)
                {
                    if (lastMove.Type != CardType.Bomb || (int)bomb.Key > lastMove.Weight)
                    {
                        var cards = bomb.Take(4).ToList();
                        moves.Add(new Move(0, cards, CardType.Bomb, (int)bomb.Key, 4));
                    }
                }

                // Rocket
                var joker = hand.FirstOrDefault(c => c.Rank == Rank.Joker);
                var bigJoker = hand.FirstOrDefault(c => c.Rank == Rank.BigJoker);
                if (joker != null && bigJoker != null)
                {
                    moves.Add(new Move(0, new List<Card> { joker, bigJoker }, CardType.Rocket, (int)Rank.BigJoker, 2));
                }
            }

            return moves;
        }
    }
}
