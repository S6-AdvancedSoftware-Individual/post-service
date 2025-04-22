using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

var dbUser = Environment.GetEnvironmentVariable("DB_USERID") ?? "";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "";
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";

// Add DbContext
var connectionString = $"User Id={dbUser};Password={dbPassword};Server={dbServer};Port={dbPort};Database={dbDatabase}";

builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PostService.Application.Features.Posts.Commands.CreatePost).Assembly));

// Register DbContext interface
builder.Services.AddScoped<IPostDbContext>(provider =>
    provider.GetRequiredService<PostDbContext>());

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddHttpClient();

var app = builder.Build();

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
