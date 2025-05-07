using Microsoft.EntityFrameworkCore;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Services;
using freelance_marketplace_backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure detailed logging
builder.Services.AddLogging(options =>
{
    options.AddConsole();
    options.AddDebug();
    options.SetMinimumLevel(LogLevel.Debug); // Enable detailed logging
});

// Add CORS policy with better WebSocket support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Required for SignalR
              .SetIsOriginAllowed(origin => true); // More permissive during development
    });
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FreelancingPlatformContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: new[] { 4060, 40197, 40501, 49918 } // Azure SQL transient errors
        );
        sqlOptions.CommandTimeout(120);
    }));

builder.Configuration.AddUserSecrets<Program>();

// Redis configuration
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string 'RedisConnection' not found in configuration.");
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "FreelancerMarketplace_";
});

// Enhanced SignalR configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; 
    options.MaximumReceiveMessageSize = 102400; 
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Service registrations
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before routing and authorization
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.MapHub<ChatHub>("/chatHub");

app.Run();