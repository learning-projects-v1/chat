using ChatApp.Application.DTOs;
using ChatApp.Application.Events;
using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;

namespace ChatApp.Application.Services;

/// <summary>
/// What used to happen inline in SendMessage, now run by the NotificationWorker for each
/// "message-sent" queue message: create the notification rows, then push via SignalR.
/// </summary>
public sealed class MessageSentHandler : IEventHandler<MessageSent>
{
    private readonly INotificationRepository _notifications;
    private readonly IProcessedMessageRepository _processedMessages;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public MessageSentHandler(
        INotificationRepository notifications,
        IProcessedMessageRepository processedMessages,
        IUnitOfWork unitOfWork,
        IRealTimeNotifier realTimeNotifier)
    {
        _notifications = notifications;
        _processedMessages = processedMessages;
        _unitOfWork = unitOfWork;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task<bool> HandleAsync(MessageSent evt, string messageId, CancellationToken cancellationToken = default)
    {
        // Duplicate guard, step 1 (the common case): a redelivery of a message we already
        // handled. Skip it without side effects; the caller still completes it.
        if (await _processedMessages.ExistsAsync(messageId, cancellationToken))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        foreach (var recipientId in evt.ThreadMemberIds.Where(id => id != evt.SenderId))
        {
            await _notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientId,
                MessageId = evt.MessageId,
                ChatThreadId = evt.ThreadId,
                SenderId = evt.SenderId,
                CreatedAt = now,
            }, cancellationToken);
        }
        await _processedMessages.AddAsync(new ProcessedMessage { MessageId = messageId, ProcessedAt = now }, cancellationToken);

        try
        {
            // One SaveChanges = one transaction: the notifications and the inbox row are
            // committed together or not at all.
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DuplicateKeyException)
        {
            // Duplicate guard, step 2 (the race): two deliveries of the same message both
            // passed step 1 at the same time. The primary key on ProcessedMessages.MessageId
            // lets only one commit; this one rolled back, so it's a duplicate: no side effects.
            return false;
        }

        // The push runs after the commit. If it throws, Service Bus redelivers, but step 1
        // then sees the inbox row and skips: the rows are created exactly once, the live push
        // is at most once. That's fine here: clients load the notifications from the DB anyway.
        var memberIds = evt.ThreadMemberIds.Select(id => id.ToString()).ToList();
        await _realTimeNotifier.NotifyMessageToAll(memberIds, ToChatPreview(evt));
        await _realTimeNotifier.NotifyMessagesSeenStatus(memberIds, new List<MessageSeenStatusDto>
        {
            new()
            {
                Id = evt.SenderSeenStatusId,
                MessageId = evt.MessageId,
                SeenAt = evt.SenderSeenAt,
                ThreadId = evt.ThreadId,
                UserId = evt.SenderId,
            },
        });

        return true;
    }

    // Same payload shape SendMessage used to push inline, so the frontend doesn't change.
    private static ChatPreviewDto ToChatPreview(MessageSent evt) => new()
    {
        Chat = new ChatDto
        {
            Id = evt.MessageId,
            Content = evt.Content,
            SenderId = evt.SenderId,
            ChatThreadId = evt.ThreadId,
            ReplyToMessageId = evt.ReplyToMessageId,
            SentAt = evt.SentAt,
        },
        SenderInfo = new UserInfoDto { Id = evt.SenderId, Username = evt.SenderUsername },
    };
}
