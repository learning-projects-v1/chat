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
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public ChatApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new ChatApiFactory(_postgres.GetConnectionString());

        // The repo has EF migrations (Infrastructure/Migrations), so apply those rather than
        // EnsureCreated: the tests then run against the same schema production gets.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

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
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "API with PostgreSQL";
}
