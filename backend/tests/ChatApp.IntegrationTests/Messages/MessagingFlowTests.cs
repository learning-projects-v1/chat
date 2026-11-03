using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatApp.Application.DTOs;
using ChatApp.IntegrationTests.Infrastructure;

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
        var alice = await RegisterAndLoginAsync("alice");
        var bob = await RegisterAndLoginAsync("bob");

        // No API endpoint creates a thread (see ApiFixture.CreateThreadAsync), so arrange it in the DB.
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);
        var content = $"hello from an integration test {Guid.NewGuid():N}";

        // Act 1: Alice sends a message with her JWT.
        var aliceClient = CreateClientWithToken(alice.AccessToken);
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
        var bobClient = CreateClientWithToken(bob.AccessToken);
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
    public async Task GetThreadMessages_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync($"/api/threads/{Guid.NewGuid()}/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetThreadMessages_NotInTheThread_CantRead()
    {
        // Arrange: two real accounts, created and logged in through the API.
        var alice = await RegisterAndLoginAsync("alice");
        var bob = await RegisterAndLoginAsync("bob");
        var carol = await RegisterAndLoginAsync("carol");
        var carolClient = CreateClientWithToken(carol.AccessToken);
        
        // No API endpoint creates a thread (see ApiFixture.CreateThreadAsync), so arrange it in the DB.
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);
        var result = await carolClient.GetAsync($"/api/threads/{threadId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    private HttpClient CreateClientWithToken(string accessToken)
    {
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private async Task<LoggedInUser> RegisterAndLoginAsync(string name)
    {
        // Unique per run: the database is shared across tests and Users.Email is unique.
        var email = $"{name}-{Guid.NewGuid():N}@example.test";
        const string password = "Integration-Test-Pa55!";
        var client = _fixture.Factory.CreateClient();

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

    private sealed record LoggedInUser(Guid UserId, string AccessToken);
}
