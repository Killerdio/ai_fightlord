using System.Linq;
using FightLord.Core.Entities;
using FightLord.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FightLord.Infrastructure
{
    public class EfGameRepository : IGameRepository
    {
        private readonly FightLordDbContext _context;

        public EfGameRepository(FightLordDbContext context)
        {
            _context = context;
        }

        public GameState GetGameState()
        {
            // Assuming single game scenario for now, or get the first one.
            // In a real app, we'd pass a GameId.
            var gameState = _context.GameStates
                .Include(g => g.KittyCards)
                .FirstOrDefault();

            if (gameState == null)
            {
                gameState = new GameState();
                _context.GameStates.Add(gameState);
                _context.SaveChanges();
            }

            return gameState;
        }

        public void SaveGameState(GameState gameState)
        {
            var existing = _context.GameStates.Find(gameState.Id);
            if (existing == null)
            {
                // If ID is 0, it might be new.
                // If ID is set but not found, we might want to add it.
                 _context.GameStates.Add(gameState);
            }
            else
            {
                // If it's already tracked, SaveChanges will handle it.
                // But if 'gameState' is a detached object (e.g. from API), we need to update.
                // Since this is likely a stateful service or short-lived context, we need to be careful.
                // For simplicity, we update the values.
                _context.Entry(existing).CurrentValues.SetValues(gameState);
                
                // Handle collections manually or let EF handle if tracking is on.
                // If KittyCards changed, we need to sync them.
                // This is complex with disconnected entities.
                // Assuming connected scenario for now or simple update.
            }
            
            _context.SaveChanges();
        }

        public Player? GetPlayer(int playerId)
        {
            return _context.Players
                .Include(p => p.HandCards)
                .FirstOrDefault(p => p.Id == playerId);
        }

        public void SavePlayer(Player player)
        {
            var existing = _context.Players.Find(player.Id);
            if (existing == null)
            {
                _context.Players.Add(player);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(player);
                // Updating HandCards would require more logic for disconnected graph
            }
            _context.SaveChanges();
        }
    }
}
