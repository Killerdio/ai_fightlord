using FightLord.Application;
using FightLord.Application.Commands;
using FightLord.Core.Entities;
using FightLord.Core.Enums;
using FightLord.Core.Interfaces;
using FightLord.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FightLord.Tests
{
    public class GameFlowTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGameRepository _repository;

        public GameFlowTests()
        {
            var services = new ServiceCollection();
            services.AddApplication();
            services.AddInfrastructure();
            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IGameRepository>();
        }

        private async Task<bool> SendAsync(IRequest<bool> command)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.Send(command);
        }

        [Fact]
        public async Task FullGameScenario()
        {
            // 1. Start Game (Initialize State in Repo)
            InitializeGameState();

            // 2. Player 1 Bids 3 (becomes Landlord)
            var bidResult = await SendAsync(new BidCommand(1, 3));
            Assert.True(bidResult, "Bid should be successful");

            var state = _repository.GetGameState();
            Assert.Equal(Phase.Playing, state.Phase);
            Assert.Equal(1, state.LandlordId);
            Assert.Equal(1, state.CurrentPlayerId); // Landlord starts first

            // 3. Player 1 Plays valid cards (e.g. 3,3,3,4)
            // Need to find card IDs for 3,3,3,4 in Player 1's hand
            var p1 = _repository.GetPlayer(1);
            var cardsToPlay = p1.HandCards
                .Where(c => c.Rank == Rank.Three).Take(3)
                .Concat(p1.HandCards.Where(c => c.Rank == Rank.Four).Take(1))
                .Select(c => c.Id)
                .ToList();

            var playResult = await SendAsync(new PlayCardCommand(1, cardsToPlay));
            Assert.True(playResult, "PlayCard should be successful");

            state = _repository.GetGameState();
            Assert.Equal(1, state.LastMove.PlayerId);
            Assert.Equal(4, state.LastMove.Cards.Count);
            Assert.Equal(2, state.CurrentPlayerId); // Turn passes to next player (2)

            // 4. Player 2 Passes
            var passResult2 = await SendAsync(new PassCommand(2));
            Assert.True(passResult2, "Player 2 Pass should be successful");
            
            state = _repository.GetGameState();
            Assert.Equal(3, state.CurrentPlayerId); // Turn passes to next player (3)

            // 5. Player 3 Passes
            var passResult3 = await SendAsync(new PassCommand(3));
            Assert.True(passResult3, "Player 3 Pass should be successful");

            state = _repository.GetGameState();
            Assert.Equal(1, state.CurrentPlayerId); // Turn passes back to Player 1
            // Since two players passed, Player 1 starts a new round of plays (LastMove should be cleared or handled by logic)
            // But let's just check it's Player 1's turn.

            // 6. Player 1 Plays again
            // Play a single 4
            var cardsToPlayAgain = p1.HandCards
                .Where(c => c.Rank == Rank.Four).Take(1)
                .Select(c => c.Id)
                .ToList();
            
            // Note: HandCards in p1 object might be stale if Repository returns a copy or if we don't refresh p1.
            // Let's refresh p1 from repo.
            p1 = _repository.GetPlayer(1);
            cardsToPlayAgain = p1.HandCards
               .Where(c => c.Rank == Rank.Four).Take(1)
               .Select(c => c.Id)
               .ToList();

            var playAgainResult = await SendAsync(new PlayCardCommand(1, cardsToPlayAgain));
            Assert.True(playAgainResult, "Player 1 second play should be successful");

            state = _repository.GetGameState();
            Assert.Equal(1, state.LastMove.PlayerId);
            Assert.Equal(1, state.LastMove.Cards.Count);
        }

        private void InitializeGameState()
        {
            var state = new GameState
            {
                Phase = Phase.Bidding,
                CurrentPlayerId = 1,
                PlayerIds = new List<int> { 1, 2, 3 },
                KittyCards = new List<Card>
                {
                    new Card(100, Rank.Ten, Suit.Diamond),
                    new Card(101, Rank.Jack, Suit.Diamond),
                    new Card(102, Rank.Queen, Suit.Diamond)
                }
            };
            _repository.SaveGameState(state);

            // Initialize Players with cards
            // Player 1 needs at least 3,3,3,4,4 to support the test scenario
            var p1Cards = new List<Card>
            {
                new Card(1, Rank.Three, Suit.Spade),
                new Card(2, Rank.Three, Suit.Heart),
                new Card(3, Rank.Three, Suit.Club),
                new Card(4, Rank.Four, Suit.Spade),
                new Card(5, Rank.Four, Suit.Heart),
                new Card(6, Rank.Five, Suit.Spade) // Extra
            };

            var p2Cards = new List<Card>
            {
                new Card(11, Rank.Six, Suit.Spade)
            };

            var p3Cards = new List<Card>
            {
                new Card(21, Rank.Seven, Suit.Spade)
            };

            var p1 = new Player(1, "Player 1");
            p1.AddCards(p1Cards);
            
            var p2 = new Player(2, "Player 2");
            p2.AddCards(p2Cards);

            var p3 = new Player(3, "Player 3");
            p3.AddCards(p3Cards);

            _repository.SavePlayer(p1);
            _repository.SavePlayer(p2);
            _repository.SavePlayer(p3);
        }
    }
}
