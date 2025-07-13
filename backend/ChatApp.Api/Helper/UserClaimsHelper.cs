using System.Security.Claims;

namespace ChatApp.API.Helper;

public static class UserClaimsHelper
{
    public static Guid GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("UserId not found in claims");
        }
        return Guid.Parse(userId);
    }
}
