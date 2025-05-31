using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public bool IsSeen { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public Guid? ReplyToMessageId { get; set; }
    // references
    public User Sender { get; set; }
    public User Receiver { get; set; }
    public Message ReplyToMessage { get; set; }
}
