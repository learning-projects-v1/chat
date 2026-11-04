using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatApp.Application.DTOs;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ChatApp.IntegrationTests.Infrastructure;

/// <summary>
/// One PostgreSQL container and one running API for every test in <see cref="ApiCollection"/>.
/// Started once before the first test, disposed after the last one.
/// The database is NOT reset between tests, so each test creates its own users (unique emails)
/// and its own thread instead of relying on rows another test made.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public ChatApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var serviceBusConnectionString = await StartServiceBusAsync();

        Factory = new ChatApiFactory(_postgres.GetConnectionString(), serviceBusConnectionString);

        // The repo has EF migrations (Infrastructure/Migrations), so apply those rather than
        // EnsureCreated: the tests then run against the same schema production gets.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await StopServiceBusAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>No Service Bus here: the API runs with ServiceBus:Enabled = false (in-process path).</summary>
    protected virtual Task<string?> StartServiceBusAsync() => Task.FromResult<string?>(null);

    protected virtual Task StopServiceBusAsync() => Task.CompletedTask;

    /// <summary>
    /// Creates a one-to-one thread with the given members directly in the database.
    /// The API has no "create thread" endpoint: threads only come from DataSeeder today
    /// (accepting a friend request does not create one), so a test has to arrange it here.
    /// </summary>
    public async Task<Guid> CreateThreadAsync(params Guid[] memberIds)
    {
        var thread = new ChatThread
        {
            Id = Guid.NewGuid(),
            ThreadName = "Integration test thread",
            CreatedAt = DateTime.UtcNow,
            IsGroup = false,
        };

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
        db.ChatThreads.Add(thread);
        db.ChatThreadMembers.AddRange(memberIds.Select(userId => new ChatThreadMember
        {
            Id = Guid.NewGuid(),
            ChatThreadId = thread.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
        }));
        await db.SaveChangesAsync();

        return thread.Id;
    }

    public HttpClient CreateClientWithToken(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    /// <summary>Registers a fresh account through the API and logs it in.</summary>
    public async Task<LoggedInUser> RegisterAndLoginAsync(string name)
    {
        // Unique per run: the database is shared across tests and Users.Email is unique.
        var email = $"{name}-{Guid.NewGuid():N}@example.test";
        const string password = "Integration-Test-Pa55!";
        var client = Factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = password, Username = name });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.AccessToken));

        return new LoggedInUser(Guid.Parse(body.UserId), body.AccessToken);
    }

    // AuthController.Login returns an anonymous object, so the shape is mirrored here.
    private sealed record LoginResponse(string AccessToken, string UserId, string Email, string Username);
}

public sealed record LoggedInUser(Guid UserId, string AccessToken);

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "API with PostgreSQL";
}
