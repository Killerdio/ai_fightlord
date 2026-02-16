using FightLord.Application.Handlers;
using FightLord.Infrastructure;
using FightLord.WebAPI.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Keep existing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Application (MediatR)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PlayCardCommandHandler).Assembly));

// Register Infrastructure
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Keep existing
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
