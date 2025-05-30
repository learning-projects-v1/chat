using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Infrastructure.Services;

namespace ChatApp.API.Middleware;

public static class ServiceRegistrationMiddleware
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
