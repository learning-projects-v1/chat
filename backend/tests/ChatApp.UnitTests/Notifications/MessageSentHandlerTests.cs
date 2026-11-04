using ChatApp.Application.DTOs;
using ChatApp.Application.Events;
using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ChatApp.UnitTests.Notifications;

/// <summary>
/// The duplicate guard in MessageSentHandler. Service Bus delivers at least once, so the
/// same queue message (same MessageId) can be handled twice; it must notify only once.
/// The real PostgreSQL primary key is covered by ServiceBusNotificationTests (integration).
/// </summary>
public class MessageSentHandlerTests
{
    private readonly FakeNotificationRepository _notifications = new();
    private readonly FakeProcessedMessageRepository _processedMessages = new();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRealTimeNotifier _notifier = Substitute.For<IRealTimeNotifier>();
    private readonly MessageSentHandler _sut;

    public MessageSentHandlerTests()
    {
        _sut = new MessageSentHandler(_notifications, _processedMessages, _unitOfWork, _notifier);
    }

    [Fact]
    public async Task HandleAsync_NewMessage_NotifiesEveryMemberExceptTheSender()
    {
        var sender = Guid.NewGuid();
        var friend = Guid.NewGuid();
        var evt = NewEvent(sender, friend);

        var handled = await _sut.HandleAsync(evt, evt.MessageId.ToString());

        Assert.True(handled);
        var notification = Assert.Single(_notifications.Added);
        Assert.Equal(friend, notification.UserId);
        Assert.Equal(evt.MessageId, notification.MessageId);
        Assert.Contains(evt.MessageId.ToString(), _processedMessages.Processed);
        // the SignalR push still goes to everyone in the thread, sender included (as before)
        await _notifier.Received(1).NotifyMessageToAll(
            Arg.Is<List<string>>(ids => ids.Count == 2), Arg.Is<ChatPreviewDto>(p => p.Chat.Id == evt.MessageId));
    }

    [Fact]
    public async Task HandleAsync_SameMessageIdTwice_CreatesOneNotification()
    {
        var evt = NewEvent(Guid.NewGuid(), Guid.NewGuid());
        var messageId = evt.MessageId.ToString();

        var first = await _sut.HandleAsync(evt, messageId);
        var second = await _sut.HandleAsync(evt, messageId); // a redelivery

        Assert.True(first);
        Assert.False(second);
        Assert.Single(_notifications.Added);
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _notifier.Received(1).NotifyMessageToAll(Arg.Any<List<string>>(), Arg.Any<ChatPreviewDto>());
    }

    [Fact]
    public async Task HandleAsync_LosesTheRaceOnTheInboxKey_SkipsWithoutPushing()
    {
        // Two deliveries ran at once: both saw "not processed yet", the other one committed
        // first, so our SaveChanges hits the primary key on ProcessedMessages.
        var evt = NewEvent(Guid.NewGuid(), Guid.NewGuid());
        _unitOfWork.SaveChangesAsync().ThrowsAsync(new DuplicateKeyException("PK_ProcessedMessages", new Exception()));

        var handled = await _sut.HandleAsync(evt, evt.MessageId.ToString());

        Assert.False(handled);
        await _notifier.DidNotReceive().NotifyMessageToAll(Arg.Any<List<string>>(), Arg.Any<ChatPreviewDto>());
        await _notifier.DidNotReceive().NotifyMessagesSeenStatus(Arg.Any<List<string>>(), Arg.Any<List<MessageSeenStatusDto>>());
    }

    private static MessageSent NewEvent(Guid sender, params Guid[] others) => new(
        MessageId: Guid.NewGuid(),
        ThreadId: Guid.NewGuid(),
        SenderId: sender,
        SenderUsername: "sender",
        Content: "hello",
        SentAt: DateTime.UtcNow,
        ReplyToMessageId: null,
        SenderSeenStatusId: Guid.NewGuid(),
        SenderSeenAt: DateTime.UtcNow,
        ThreadMemberIds: others.Prepend(sender).ToList());

    // Hand-written fakes instead of substitutes: the guard depends on state ("was this id
    // stored?"), which a list and a set show more plainly than mock setups.
    // Simplification: rows count as saved as soon as they're added (no SaveChanges step).
    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Added { get; } = new();

        public Task AddAsync(Notification entity, CancellationToken cancellationToken = default)
        {
            Added.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Notification?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Added.FirstOrDefault(n => n.Id == id));

        public void Remove(Notification entity, CancellationToken cancellationToken = default) => Added.Remove(entity);

        public void Update(Notification entity, CancellationToken cancellationToken = default) { }
    }

    private sealed class FakeProcessedMessageRepository : IProcessedMessageRepository
    {
        public HashSet<string> Processed { get; } = new();

        public Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Processed.Contains(messageId));

        public Task AddAsync(ProcessedMessage processedMessage, CancellationToken cancellationToken = default)
        {
            Processed.Add(processedMessage.MessageId);
            return Task.CompletedTask;
        }
    }
}
