using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Services;
using freelance_marketplace_backend.Services.freelance_marketplace_backend.Services;

using Stripe;
using freelance_marketplace_backend.Models;

using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
        }
    );
});

//Stripe configuration
//Retrieve the Stripe API keys from appsettings.json
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.Configure<StripeSettings>(
  builder.Configuration.GetSection("Stripe")
);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FreelancingPlatformContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: new[] { 4060, 40197, 40501, 49918 } // Azure SQL transient errors
            );
            sqlOptions.CommandTimeout(120);
        }
    )
);

builder.Configuration.AddUserSecrets<Program>();

// Get the Redis connection string from configuration
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException(
        "Redis connection string 'RedisConnection' not found in configuration."
    );
}

// Configure Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "FreelancerMarketplace_";
});

// Add services to the container.
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ClientProjectRepository>();
builder.Services.AddScoped<IClientProjectService, ClientProjectService>();

builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<ProjectRepository>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(
        "Bearer",
        options =>
        {
            options.Authority = "https://securetoken.google.com/freelance-marketplace-caf38";
            options.TokenValidationParameters =
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/freelance-marketplace-caf38",
                    ValidateAudience = true,
                    ValidAudience = "freelance-marketplace-caf38",
                    ValidateLifetime = true,
                };
        }
    );

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before routing and authorization
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
