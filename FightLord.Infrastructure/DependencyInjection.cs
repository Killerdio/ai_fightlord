using FightLord.Core.Interfaces;
using FightLord.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis;

namespace FightLord.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });

            // Register DbContext
            services.AddDbContext<FightLordDbContext>(options =>
                options.UseInMemoryDatabase("FightLordDb"));

            // Register Redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

            // Register Services
            services.AddSingleton<IRuleEngine, RuleEngine>();
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IGameRepository, EfGameRepository>();
            
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoomService, RoomService>();

            return services;
        }
    }
}
