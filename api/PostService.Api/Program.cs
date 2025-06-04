using DotNetEnv;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Infrastructure.Persistence;
using Serilog;

const string API_VERSION = "v2.3";

Env.Load();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "";
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

var dbSearchHost = Environment.GetEnvironmentVariable("DB_SEARCH_HOST") ?? "";
var dbSearchDatabase = Environment.GetEnvironmentVariable("DB_SEARCH_DATABASE") ?? "";
var dbSearchUser = Environment.GetEnvironmentVariable("DB_SEARCH_USER") ?? "";
var dbSearchPassword = Environment.GetEnvironmentVariable("DB_SEARCH_PASSWORD") ?? "";

var betterstack_sourceToken = Environment.GetEnvironmentVariable("BETTERSTACK_SOURCETOKEN") ?? "";
var betterstack_endpoint = Environment.GetEnvironmentVariable("BETTERSTACK_ENDPOINT") ?? "";

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.BetterStack(
        sourceToken: betterstack_sourceToken,
        betterStackEndpoint: betterstack_endpoint
    )
    .MinimumLevel.Information()
    .CreateLogger();

Log.Information("API Version: {Version}", API_VERSION);
Log.Information("Starting up the application...");
Log.Information("Checking environment variables...");

if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword) || string.IsNullOrEmpty(dbHost) ||
   string.IsNullOrEmpty(dbDatabase))
{
    Log.Error("Database connection information is missing.");
    throw new SystemException("Application misconfigured, primary database environment variables are missing.");
}

if (string.IsNullOrEmpty(betterstack_endpoint) || string.IsNullOrEmpty(betterstack_sourceToken))
{
    Log.Error("BetterStack connection information is missing.");
    throw new SystemException("Application misconfigured, monitoring & logging environment variables are missing.");

}

if (string.IsNullOrEmpty(dbSearchDatabase) || string.IsNullOrEmpty(dbSearchUser) || string.IsNullOrEmpty(dbSearchPassword) | string.IsNullOrEmpty(dbSearchHost))
{
    Log.Error("Database search connection information is missing.");
    throw new SystemException("Application misconfigured, secondary database environment variables are missing.");
}

Log.Information("Environment variables checked, all variables are valid.");

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Add DbContext
var connectionString = $"Host={dbHost};Database={dbDatabase};Username={dbUser};Password={dbPassword}";
var searchConnectionString = $"Host={dbSearchHost};Database={dbSearchDatabase};Username={dbSearchUser};Password={dbSearchPassword}";

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<PostDbContextSecondary>(options =>
    options.UseNpgsql(searchConnectionString));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PostService.Application.Features.Posts.Commands.CreatePost).Assembly));

// Register DbContext interface
builder.Services.AddScoped<IPostDbContext>(provider =>
    provider.GetRequiredService<PostDbContext>());

// Register DbContext interface
builder.Services.AddScoped<IPostDbContextSecondary>(provider =>
    provider.GetRequiredService<PostDbContextSecondary>());

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure health checks
app.MapHealthChecks("/_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

Log.Information("Healthcheck set-up finished, query '/_health' to validate application health.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS middleware
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
