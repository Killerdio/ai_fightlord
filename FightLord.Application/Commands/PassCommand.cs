using MediatR;

namespace FightLord.Application.Commands
{
    public class PassCommand : IRequest<bool>
    {
        public int PlayerId { get; set; }

        public PassCommand(int playerId)
        {
            PlayerId = playerId;
        }
    }
}
