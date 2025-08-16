
using ChatApp.API.Hubs;
using ChatApp.API.Middleware;
using ChatApp.Infrastructure.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  
    });
});

builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDbContextServices(builder.Configuration)
    .AddAuthenticationServices(jwtSettings!)
    .AddCustomAuthorization()
    .AddServices()
    .AddSignalR();

var app = builder.Build();
app.MapHub<NotificationHub>("/hubs/notifications");
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var shouldMigrate = config.GetValue<bool>("RunMigrations");
    if (shouldMigrate)
    {
        var shouldClear = config.GetValue<bool>("ClearDatabase");
        Console.WriteLine("Db Migration Started...");
        if (shouldClear)
        {
            Console.WriteLine("Deleting Existing Db");
            dbContext.Database.EnsureDeleted();
        }
        dbContext.Database.Migrate();
        Console.WriteLine("Migration done");

        var seeder = new DataSeeder(dbContext);
        seeder.SeedInitialData();
    }
}
app.Run();
