using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Middleware;

public static class DbContextMiddleware
{
    public static IServiceCollection AddDbContextMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatAppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        return services;
    }
}
