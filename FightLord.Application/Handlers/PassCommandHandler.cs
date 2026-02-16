using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FightLord.Application.Commands;
using FightLord.Core.Entities;
using FightLord.Core.Enums;
using FightLord.Core.Interfaces;
using MediatR;

namespace FightLord.Application.Handlers
{
    public class PassCommandHandler : IRequestHandler<PassCommand, bool>
    {
        private readonly IGameRepository _repository;

        public PassCommandHandler(IGameRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(PassCommand request, CancellationToken cancellationToken)
        {
            var gameState = _repository.GetGameState();
            if (gameState.Phase != Phase.Playing) return Task.FromResult(false);
            if (gameState.CurrentPlayerId != request.PlayerId) return Task.FromResult(false);

            // Cannot pass if it's a free turn (i.e., last move was mine or null)
            if (gameState.LastMove == null || gameState.LastMove.PlayerId == request.PlayerId)
            {
                return Task.FromResult(false);
            }

            gameState.PassCount++;
            
            var currentIndex = gameState.PlayerIds.IndexOf(request.PlayerId);
            var nextIndex = (currentIndex + 1) % gameState.PlayerIds.Count;
            gameState.CurrentPlayerId = gameState.PlayerIds[nextIndex];

            // Check if everyone passed
            if (gameState.PassCount >= gameState.PlayerIds.Count - 1)
            {
                // Next player gets free turn.
                // We don't need to clear LastMove because PlayCardHandler checks LastMove.PlayerId.
                // If LastMove.PlayerId == NextPlayerId, it's a free turn.
                // Wait, LastMove.PlayerId is the person who played the cards.
                // If PassCount >= 2, the next player IS LastMove.PlayerId.
                // So the logic holds.
            }

            _repository.SaveGameState(gameState);
            return Task.FromResult(true);
        }
    }
}
