using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FightLord.WebAPI.Middlewares
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            await _next(context);
            
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = context.Request.Path;
                _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)", 
                    requestName, elapsedMilliseconds);
            }
        }
    }
}
