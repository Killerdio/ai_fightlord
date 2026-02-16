using MediatR;
using System.Collections.Generic;

namespace FightLord.Application.Commands
{
    public class PlayCardCommand : IRequest<bool>
    {
        public int PlayerId { get; set; }
        public List<int> CardIds { get; set; }

        public PlayCardCommand(int playerId, List<int> cardIds)
        {
            PlayerId = playerId;
            CardIds = cardIds;
        }
    }
}
