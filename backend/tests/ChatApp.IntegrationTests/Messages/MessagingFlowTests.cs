using System.Net;
using System.Net.Http.Json;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Messaging;
using ChatApp.Infrastructure.Persistence;
using ChatApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.IntegrationTests.Messages;

[Collection(ApiCollection.Name)]
public class MessagingFlowTests
{
    private readonly ApiFixture _fixture;

    public MessagingFlowTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterLoginSendAndFetch_MessageIsStoredAndVisibleToTheOtherMember()
    {
        // Arrange: two real accounts, created and logged in through the API.
        var alice = await _fixture.RegisterAndLoginAsync("alice");
        var bob = await _fixture.RegisterAndLoginAsync("bob");

        // No API endpoint creates a thread (see ApiFixture.CreateThreadAsync), so arrange it in the DB.
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);
        var content = $"hello from an integration test {Guid.NewGuid():N}";

        // Act 1: Alice sends a message with her JWT.
        var aliceClient = _fixture.CreateClientWithToken(alice.AccessToken);
        var sendResponse = await aliceClient.PostAsJsonAsync($"/api/threads/{threadId}/messages", new ChatDto
        {
            ChatThreadId = threadId,
            Content = content,
            SentAt = DateTime.UtcNow,
        });

        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var sent = await sendResponse.Content.ReadFromJsonAsync<ChatDto>();
        Assert.NotNull(sent);
        Assert.NotEqual(Guid.Empty, sent.Id);

        // Act 2: Bob fetches the conversation with his own JWT (a separate request, so this
        // proves the message was persisted in PostgreSQL, not just echoed back).
        var bobClient = _fixture.CreateClientWithToken(bob.AccessToken);
        var conversation = await bobClient.GetFromJsonAsync<ChatThreadDto>($"/api/threads/{threadId}/messages");

        // Assert
        Assert.NotNull(conversation);
        var message = Assert.Single(conversation.Chats);
        Assert.Equal(sent.Id, message.Id);
        Assert.Equal(content, message.Content);
        Assert.Equal(alice.UserId, message.SenderId); // sender comes from Alice's token
        Assert.Equal(
            new[] { alice.UserId, bob.UserId }.OrderBy(id => id),
            conversation.MemberInfoList.Select(m => m.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task SendMessage_WithoutServiceBus_CreatesTheNotificationInProcess()
    {
        // This collection runs with ServiceBus:Enabled = false: the fallback path.
        Assert.IsType<InProcessEventPublisher>(_fixture.Factory.Services.GetRequiredService<IEventPublisher>());

        var alice = await _fixture.RegisterAndLoginAsync("alice");
        var bob = await _fixture.RegisterAndLoginAsync("bob");
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);

        var response = await _fixture.CreateClientWithToken(alice.AccessToken).PostAsJsonAsync(
            $"/api/threads/{threadId}/messages",
            new ChatDto { ChatThreadId = threadId, Content = "hi", SentAt = DateTime.UtcNow });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sent = await response.Content.ReadFromJsonAsync<ChatDto>();

        // No polling needed: in-process, the row exists by the time the request returns.
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
        var notification = Assert.Single(await db.Notifications.Where(n => n.MessageId == sent!.Id).ToListAsync());
        Assert.Equal(bob.UserId, notification.UserId);
    }

    [Fact]
    public async Task GetThreadMessages_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync($"/api/threads/{Guid.NewGuid()}/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetThreadMessages_NotInTheThread_CantRead()
    {
        var alice = await _fixture.RegisterAndLoginAsync("alice");
        var bob = await _fixture.RegisterAndLoginAsync("bob");
        var carol = await _fixture.RegisterAndLoginAsync("carol");
        var carolClient = _fixture.CreateClientWithToken(carol.AccessToken);

        // No API endpoint creates a thread (see ApiFixture.CreateThreadAsync), so arrange it in the DB.
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);
        var result = await carolClient.GetAsync($"/api/threads/{threadId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
}
