using Microsoft.EntityFrameworkCore;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<FreelancingPlatformContext>(options =>
    options.UseSqlServer(connectionString)); 

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
