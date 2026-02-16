using FightLord.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;

namespace FightLord.Infrastructure
{
    public class FightLordDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<GameState> GameStates { get; set; }

        public FightLordDbContext(DbContextOptions<FightLordDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Card Configuration
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(c => c.Id);
                // Assuming Card Id is not auto-generated for now as they are specific cards (0-53)
                // But usually DB generates IDs. 
                // Given the game logic, we might pre-seed cards.
                // For simplicity, let's let DB generate if needed, or assume they are assigned.
                entity.Property(c => c.Id).ValueGeneratedNever(); 
            });

            // Player Configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedNever(); // Assuming IDs come from external system or are fixed for now
                
                // Player - HandCards (One-to-Many)
                entity.HasMany(p => p.HandCards)
                      .WithOne()
                      .HasForeignKey("PlayerId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // GameState Configuration
            modelBuilder.Entity<GameState>(entity =>
            {
                entity.HasKey(g => g.Id);

                // GameState - KittyCards (One-to-Many)
                entity.HasMany(g => g.KittyCards)
                      .WithOne()
                      .HasForeignKey("GameStateId")
                      .OnDelete(DeleteBehavior.Cascade);

                // JSON Conversions for complex types
                
                entity.Property(g => g.Bids)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<int, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<int, int>()
                    );

                entity.Property(g => g.PlayerIds)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>()
                    );

                entity.Property(g => g.Multipliers)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>()
                    );

                entity.Property(g => g.LastMove)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Move>(v, (JsonSerializerOptions?)null)
                    );
            });
        }
    }
}
