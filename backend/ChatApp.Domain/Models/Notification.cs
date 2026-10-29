namespace ChatApp.Domain.Models;

/// <summary>
/// "You have a new message": one row per recipient (every thread member except the sender).
/// Created by MessageSentHandler, never by the request that sent the message.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }          // the recipient
    public Guid MessageId { get; set; }
    public Guid ChatThreadId { get; set; }
    public Guid SenderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
