using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Azure.Messaging.ServiceBus;
using ChatApp.Application.DTOs;
using ChatApp.Application.Events;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Messaging;
using ChatApp.Infrastructure.Persistence;
using ChatApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.IntegrationTests.Notifications;

/// <summary>
/// The whole flow through a real queue:
/// POST message -> API publishes MessageSent -> Service Bus emulator ("message-sent")
/// -> NotificationWorker -> MessageSentHandler -> Notifications row in PostgreSQL.
/// </summary>
[Collection(ServiceBusCollection.Name)]
public class ServiceBusNotificationTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    private readonly ServiceBusApiFixture _fixture;

    public ServiceBusNotificationTests(ServiceBusApiFixture fixture)
    {
        _fixture = fixture;
    }

    [ServiceBusFact]
    public async Task SendMessage_NotificationArrivesThroughTheQueue()
    {
        // This collection runs with ServiceBus:Enabled = true.
        Assert.IsType<ServiceBusEventPublisher>(_fixture.Factory.Services.GetRequiredService<IEventPublisher>());

        var alice = await _fixture.RegisterAndLoginAsync("alice");
        var bob = await _fixture.RegisterAndLoginAsync("bob");
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);

        var sent = await SendMessageAsync(alice, threadId, "hello through Service Bus");

        // The worker runs in the background, so the row shows up some time after the request returns.
        var notifications = await WaitForNotificationsAsync(sent.Id, expected: 1);
        var notification = Assert.Single(notifications);
        Assert.Equal(bob.UserId, notification.UserId);        // the recipient, not the sender
        Assert.Equal(alice.UserId, notification.SenderId);
        Assert.Equal(threadId, notification.ChatThreadId);
        Assert.True(await WithDbAsync(db => db.ProcessedMessages.AnyAsync(p => p.MessageId == sent.Id.ToString())));
    }

    [ServiceBusFact]
    public async Task SameMessageDeliveredTwice_CreatesOneNotification()
    {
        var alice = await _fixture.RegisterAndLoginAsync("alice");
        var bob = await _fixture.RegisterAndLoginAsync("bob");
        var threadId = await _fixture.CreateThreadAsync(alice.UserId, bob.UserId);
        var sent = await SendMessageAsync(alice, threadId, "say it once");
        await WaitForNotificationsAsync(sent.Id, expected: 1);

        // Simulate at-least-once delivery: the same event, with the same MessageId, lands on
        // the queue again (e.g. the publisher retried after a timeout). The queue has
        // RequiresDuplicateDetection = false, so Service Bus delivers it; our guard must drop it.
        var publisher = _fixture.Factory.Services.GetRequiredService<IEventPublisher>();
        await publisher.PublishAsync(new MessageSent(
            MessageId: sent.Id, ThreadId: threadId, SenderId: alice.UserId, SenderUsername: "alice",
            Content: sent.Content, SentAt: sent.SentAt, ReplyToMessageId: null,
            SenderSeenStatusId: Guid.NewGuid(), SenderSeenAt: DateTime.UtcNow,
            ThreadMemberIds: new[] { alice.UserId, bob.UserId }), sent.Id.ToString());

        // A message leaves the queue only when the worker completes it, i.e. after the handler ran.
        await WaitForQueueToDrainAsync();

        var notifications = await WithDbAsync(db => db.Notifications.Where(n => n.MessageId == sent.Id).ToListAsync());
        Assert.Single(notifications);
    }

    private async Task<ChatDto> SendMessageAsync(LoggedInUser sender, Guid threadId, string content)
    {
        var response = await _fixture.CreateClientWithToken(sender.AccessToken).PostAsJsonAsync(
            $"/api/threads/{threadId}/messages",
            new ChatDto { ChatThreadId = threadId, Content = content, SentAt = DateTime.UtcNow });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sent = await response.Content.ReadFromJsonAsync<ChatDto>();
        Assert.NotNull(sent);
        return sent;
    }

    private async Task<List<Notification>> WaitForNotificationsAsync(Guid messageId, int expected)
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            var rows = await WithDbAsync(db => db.Notifications.Where(n => n.MessageId == messageId).ToListAsync());
            if (rows.Count >= expected) return rows;
            if (clock.Elapsed > Timeout)
                Assert.Fail($"Expected {expected} notification(s) for message {messageId} within {Timeout.TotalSeconds}s, found {rows.Count}.");
            await Task.Delay(200);
        }
    }

    private async Task WaitForQueueToDrainAsync()
    {
        // Peek sees every message still on the queue, including ones locked by the worker.
        var client = _fixture.Factory.Services.GetRequiredService<ServiceBusClient>();
        var clock = Stopwatch.StartNew();
        while (true)
        {
            await using var receiver = client.CreateReceiver(QueueNames.MessageSent);
            if (await receiver.PeekMessageAsync() is null) return;
            if (clock.Elapsed > Timeout) Assert.Fail("The message-sent queue did not drain in time.");
            await Task.Delay(200);
        }
    }

    private async Task<T> WithDbAsync<T>(Func<ChatAppDbContext, Task<T>> query)
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<ChatAppDbContext>());
    }
}
