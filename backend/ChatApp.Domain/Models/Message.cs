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
    public Guid ChatThreadId { get; set; }
    public bool IsSeen { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public Guid? ReplyToMessageId { get; set; }
    // references
    public User Sender { get; set; }
    //public ChatThread ChatThread { get; set; }
    public Message ReplyToMessage { get; set; }
    //public ICollection<Reaction> Reactions { get; set; }
}
