using ChatApp.API.Hubs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Services;

namespace ChatApp.API.Middleware;

public static class ServiceRegistrationMiddleware
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IMessageRepository, MessagesRepository>();
        services.AddScoped<IChatThreadMemberRepository, ChatThreadMemberRepository>();
        services.AddScoped<IChatThreadRepository, ChatThreadRepository>();
        services.AddScoped<IReactionRepository, ReactionRepository>();
        services.AddScoped<IMessageSeenStatusRepository, MessageSeenStatusRepository>();
        services.AddScoped<IRealTimeNotifier, RealTimeNotifier>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
