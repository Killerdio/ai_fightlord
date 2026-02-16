using FightLord.Core.Entities;

namespace FightLord.Core.Interfaces
{
    public interface IGameRepository
    {
        GameState GetGameState();
        void SaveGameState(GameState gameState);
        Player? GetPlayer(int playerId);
        void SavePlayer(Player player);
    }
}
