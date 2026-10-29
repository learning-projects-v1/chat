namespace ChatApp.Domain.Models;

/// <summary>
/// Inbox row for the duplicate guard: "this queue message has already been handled".
/// Service Bus delivers at least once, so the same message can arrive twice; its
/// MessageId is the primary key here, so the database itself refuses a second row.
/// </summary>
public class ProcessedMessage
{
    public string MessageId { get; set; } = "";   // ServiceBusMessage.MessageId (= the chat message id)
    public DateTime ProcessedAt { get; set; }
}
