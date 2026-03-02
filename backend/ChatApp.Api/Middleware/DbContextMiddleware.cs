using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Middleware;

public static class DbContextMiddleware
{
    public static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

        // Render provides PostgreSQL URIs (postgresql://user:pass@host:port/db)
        // but Npgsql expects ADO.NET format (Host=...;Port=...;Database=...;Username=...;Password=...)
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            connectionString = ConvertPostgresUri(connectionString);
        }

        services.AddDbContext<ChatAppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }

    private static string ConvertPostgresUri(string uri)
    {
        var parsedUri = new Uri(uri);
        var userInfo = parsedUri.UserInfo.Split(':');
        var host = parsedUri.Host;
        var port = parsedUri.Port > 0 ? parsedUri.Port : 5432;
        var database = parsedUri.AbsolutePath.TrimStart('/');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
