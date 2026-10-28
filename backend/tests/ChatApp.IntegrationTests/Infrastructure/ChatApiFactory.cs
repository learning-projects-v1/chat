using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChatApp.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API (the real Program.cs, DI, auth, EF Core) in memory on a TestServer,
/// pointed at the PostgreSQL container started by <see cref="ApiFixture"/>.
/// </summary>
public sealed class ChatApiFactory : WebApplicationFactory<Program>
{
    // Test-only values. Program.cs and AddDbContextServices read these from configuration
    // while building the host, before Build() is called.
    public const string JwtSecret = "integration-tests-only-secret-key-at-least-32-bytes";
    public const string JwtIssuer = "ChatApp.IntegrationTests";
    public const string JwtAudience = "ChatApp.IntegrationTests.Clients";

    private readonly string _connectionString;

    public ChatApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not "Development": skips Swagger and UseHttpsRedirection, which the tests don't need.
        builder.UseEnvironment("Testing");

        // UseSetting (not ConfigureAppConfiguration): Program.cs reads JwtSettings and the
        // connection string from builder.Configuration *before* builder.Build(), and only
        // UseSetting values are visible that early under minimal hosting.
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);

        // Program.cs would otherwise migrate AND run DataSeeder (15 demo users + threads).
        // The fixture applies migrations itself, so the database starts empty.
        builder.UseSetting("RunMigrations", "false");
        builder.UseSetting("ClearDatabase", "false");

        builder.UseSetting("JwtSettings:Secret", JwtSecret);
        builder.UseSetting("JwtSettings:Issuer", JwtIssuer);
        builder.UseSetting("JwtSettings:Audience", JwtAudience);
        builder.UseSetting("JwtSettings:AccessTokenExpirationMinutes", "60");
        builder.UseSetting("JwtSettings:RefreshTokenExpirationHours", "24");
    }
}
