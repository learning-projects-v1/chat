namespace ChatApp.Application.Events;

/// <summary>
/// Published by MessagesController.SendMessage after the message is saved.
/// It carries everything the notifier needs (a snapshot taken at send time),
/// so the handler doesn't have to load the message, sender or thread again.
/// </summary>
public sealed record MessageSent(
    Guid MessageId,
    Guid ThreadId,
    Guid SenderId,
    string SenderUsername,
    string Content,
    DateTime SentAt,
    Guid? ReplyToMessageId,
    Guid SenderSeenStatusId,      // the sender has "seen" their own message
    DateTime SenderSeenAt,
    IReadOnlyList<Guid> ThreadMemberIds); // includes the sender: everyone gets the SignalR push

public static class QueueNames
{
    public const string MessageSent = "message-sent";
}
