using MediatR;

namespace FightLord.Application.Commands
{
    public class BidCommand : IRequest<bool>
    {
        public int PlayerId { get; set; }
        public int Score { get; set; }

        public BidCommand(int playerId, int score)
        {
            PlayerId = playerId;
            Score = score;
        }
    }
}
