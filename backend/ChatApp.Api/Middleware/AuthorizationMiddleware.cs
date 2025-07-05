using ChatApp.Domain.Constants;
using ChatApp.Infrastructure.Authorization;

namespace ChatApp.API.Middleware;

public static class AuthorizationMiddleware
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        return services.AddAuthorization(options =>
        {
            options.AddPolicy(GlobalConstants.AuthorizationPolicy.ThreadMember, (policy) =>
            {
                policy.Requirements.Add(new ThreadMemberRequirement());
            });
        });
    }
}
