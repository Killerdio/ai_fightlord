using FightLord.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FightLord.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
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

            // Register Services
            services.AddSingleton<IRuleEngine, RuleEngine>();
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IGameRepository, EfGameRepository>();

            return services;
        }
    }
}
