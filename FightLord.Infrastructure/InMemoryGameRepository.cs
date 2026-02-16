using FightLord.Core.Entities;
using FightLord.Core.Interfaces;
using System.Collections.Generic;

namespace FightLord.Infrastructure
{
    public class InMemoryGameRepository : IGameRepository
    {
        private static GameState _gameState = new GameState();
        private static Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public GameState GetGameState()
        {
            return _gameState;
        }

        public void SaveGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        public Player? GetPlayer(int playerId)
        {
            if (_players.ContainsKey(playerId))
            {
                return _players[playerId];
            }
            return null;
        }

        public void SavePlayer(Player player)
        {
            _players[player.Id] = player;
        }
    }
}
