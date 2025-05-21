using DotNetEnv;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Infrastructure.Persistence;
using Serilog;

Env.Load();

var dbUser = Environment.GetEnvironmentVariable("DB_USERID") ?? "";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "";
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";

var dbSearchDatabase = Environment.GetEnvironmentVariable("DB_SEARCH_DATABASE") ?? "";
var dbSearchUser = Environment.GetEnvironmentVariable("DB_SEARCH_USERID") ?? "";
var dbSearchPassword = Environment.GetEnvironmentVariable("DB_SEARCH_PASSWORD") ?? "";
var dbSearchServer = Environment.GetEnvironmentVariable("DB_SEARCH_SERVER") ?? "";
var dbSearchPort = Environment.GetEnvironmentVariable("DB_SEARCH_PORT") ?? "";

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

Log.Information("Starting up the application...");
Log.Information("Checking environment variables...");

if(string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword) || string.IsNullOrEmpty(dbServer) ||
    string.IsNullOrEmpty(dbPort) || string.IsNullOrEmpty(dbDatabase))
{
    Log.Error("Database connection information is missing.");
    throw new SystemException("Application misconfigured, primary database environment variables are missing.");
}

if (string.IsNullOrEmpty(betterstack_endpoint) || string.IsNullOrEmpty(betterstack_sourceToken))
{
    Log.Error("BetterStack connection information is missing.");
    throw new SystemException("Application misconfigured, monitoring & logging environment variables are missing.");

}

if (string.IsNullOrEmpty(dbSearchDatabase) || string.IsNullOrEmpty(dbSearchUser) || string.IsNullOrEmpty(dbSearchPassword) ||
    string.IsNullOrEmpty(dbSearchServer) || string.IsNullOrEmpty(dbSearchPort))
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
var connectionString = $"User Id={dbUser};Password={dbPassword};Server={dbServer};Port={dbPort};Database={dbDatabase}";
var searchConnectionString = $"User Id={dbSearchUser};Password={dbSearchPassword};Server={dbSearchServer};Port={dbSearchPort};Database={dbSearchDatabase}";

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
