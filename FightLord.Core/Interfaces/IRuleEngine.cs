using System.Collections.Generic;
using FightLord.Core.Entities;
using FightLord.Core.Enums;

namespace FightLord.Core.Interfaces
{
    public interface IRuleEngine
    {
        CardType AnalyzeType(List<Card> cards);
        bool CompareMoves(Move newMove, Move lastMove);
        List<Move> SearchAvailableMoves(List<Card> hand, Move? lastMove);
    }
}
