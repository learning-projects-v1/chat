using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.API.Helper;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Models;
using ChatApp.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.UnitTests.Helpers;

public class HelpersTests
{
    private static readonly JwtSettings Settings = new()
    {
        Secret = "test-secret-that-is-at-least-32-bytes-long!!",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenExpirationMinutes = 5
    };

    [Fact]
    public void JwtTokenGenerator_GenerateToken_ContainsUserIdClaim()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "a" };
        var sut = new JwtTokenGenerator(Options.Create(Settings));

        var token = sut.GenerateToken(user).AccessToken;

        // Validate the token the way the API does, so the claim is read back exactly as
        // [Authorize] sees it. Inside the raw JWT, ClaimTypes.Name is stored as "unique_name";
        // the handler maps it back to ClaimTypes.Name on the way in.
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Settings.Secret)),
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience
        }, out _);

        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Id, UserClaimsHelper.GetUserId(principal));
    }

    [Fact]
    public void UserClaimsHelper_GetUserId_NoClaim_Throws()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<ArgumentException>(() => UserClaimsHelper.GetUserId(anonymous));
    }
}
