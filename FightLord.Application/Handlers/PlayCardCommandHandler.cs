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
    public class PlayCardCommandHandler : IRequestHandler<PlayCardCommand, bool>
    {
        private readonly IGameRepository _repository;
        private readonly IRuleEngine _ruleEngine;

        public PlayCardCommandHandler(IGameRepository repository, IRuleEngine ruleEngine)
        {
            _repository = repository;
            _ruleEngine = ruleEngine;
        }

        public Task<bool> Handle(PlayCardCommand request, CancellationToken cancellationToken)
        {
            var gameState = _repository.GetGameState();
            if (gameState.Phase != Phase.Playing) return Task.FromResult(false);
            if (gameState.CurrentPlayerId != request.PlayerId) return Task.FromResult(false);

            var player = _repository.GetPlayer(request.PlayerId);
            if (player == null) return Task.FromResult(false);

            // Verify player has cards
            var cardsToPlay = new List<Card>();
            foreach (var id in request.CardIds)
            {
                var card = player.HandCards.FirstOrDefault(c => c.Id == id);
                if (card == null) return Task.FromResult(false);
                cardsToPlay.Add(card);
            }

            // Analyze type
            var type = _ruleEngine.AnalyzeType(cardsToPlay);
            if (type == CardType.Unknown) return Task.FromResult(false);

            // Calculate weight
            int weight = CalculateWeight(cardsToPlay, type);
            var move = new Move(request.PlayerId, cardsToPlay, type, weight, cardsToPlay.Count);

            // Validate against last move
            if (gameState.LastMove != null && gameState.LastMove.PlayerId != request.PlayerId)
            {
                if (!_ruleEngine.CompareMoves(move, gameState.LastMove))
                    return Task.FromResult(false);
            }

            // Execute move
            player.RemoveCards(cardsToPlay);
            gameState.LastMove = move;
            gameState.PassCount = 0;
            
            // Next player
            var currentIndex = gameState.PlayerIds.IndexOf(request.PlayerId);
            var nextIndex = (currentIndex + 1) % gameState.PlayerIds.Count;
            gameState.CurrentPlayerId = gameState.PlayerIds[nextIndex];

            // Save
            _repository.SavePlayer(player);
            _repository.SaveGameState(gameState);

            // Check win
            if (player.HandCards.Count == 0)
            {
                gameState.Phase = Phase.Ended;
                _repository.SaveGameState(gameState);
            }

            return Task.FromResult(true);
        }

        private int CalculateWeight(List<Card> cards, CardType type)
        {
            var groups = cards.GroupBy(c => c.Rank).ToList();
            
            if (type == CardType.Rocket)
                return (int)Rank.BigJoker;

            if (type == CardType.Bomb)
                return (int)groups.First(g => g.Count() == 4).Key;

            if (type == CardType.Triple || type == CardType.TripleWithOne || type == CardType.TripleWithTwo)
                return (int)groups.First(g => g.Count() >= 3).Key;

            // Single, Pair, Straight, etc. usually sorted by first card
            // But Straight needs first card rank.
            return (int)cards.OrderBy(c => c.Rank).First().Rank;
        }
    }
}
