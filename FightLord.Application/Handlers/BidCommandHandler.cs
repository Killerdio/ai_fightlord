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
    public class BidCommandHandler : IRequestHandler<BidCommand, bool>
    {
        private readonly IGameRepository _repository;

        public BidCommandHandler(IGameRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(BidCommand request, CancellationToken cancellationToken)
        {
            var gameState = _repository.GetGameState();
            if (gameState.Phase != Phase.Bidding) return Task.FromResult(false);
            if (gameState.CurrentPlayerId != request.PlayerId) return Task.FromResult(false);

            int currentMax = gameState.Bids.Any() ? gameState.Bids.Values.Max() : 0;
            
            // Validate bid: must be higher than current max, or pass (0)
            if (request.Score > 0 && request.Score <= currentMax)
                return Task.FromResult(false);
            if (request.Score < 0 || request.Score > 3)
                return Task.FromResult(false);

            gameState.Bids[request.PlayerId] = request.Score;

            // Check if bidding ends
            // 1. Someone bid 3 -> instant win
            // 2. All players have bid
            bool biddingEnds = false;
            if (request.Score == 3)
            {
                biddingEnds = true;
            }
            else if (gameState.Bids.Count == gameState.PlayerIds.Count)
            {
                biddingEnds = true;
            }

            if (biddingEnds)
            {
                FinishBidding(gameState);
            }
            else
            {
                // Next player
                var currentIndex = gameState.PlayerIds.IndexOf(request.PlayerId);
                var nextIndex = (currentIndex + 1) % gameState.PlayerIds.Count;
                gameState.CurrentPlayerId = gameState.PlayerIds[nextIndex];
            }

            _repository.SaveGameState(gameState);
            return Task.FromResult(true);
        }

        private void FinishBidding(GameState gameState)
        {
            // Determine landlord
            int maxScore = 0;
            int landlordId = gameState.PlayerIds[0];

            foreach (var kvp in gameState.Bids)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    landlordId = kvp.Key;
                }
            }

            // If all passed (maxScore == 0), maybe default to first player with score 1 or restart
            if (maxScore == 0)
            {
                maxScore = 1;
                // landlordId remains first player
            }

            gameState.LandlordId = landlordId;
            gameState.Multipliers["Base"] = maxScore * 15; // Example base calculation
            gameState.Phase = Phase.Playing;
            gameState.CurrentPlayerId = landlordId;

            // Update Roles and Give kitty cards
            foreach (var pid in gameState.PlayerIds)
            {
                var p = _repository.GetPlayer(pid);
                if (p != null)
                {
                    if (pid == landlordId)
                    {
                        p.Role = Role.Landlord;
                        p.AddCards(gameState.KittyCards);
                    }
                    else
                    {
                        p.Role = Role.Peasant;
                    }
                    _repository.SavePlayer(p);
                }
            }
        }
    }
}
