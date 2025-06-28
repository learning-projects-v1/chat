using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ChatDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid SenderId { get; set; }
    public Guid ChatThreadId { get; set; }
    public bool IsSeen { get; set; } = false;
    public Guid? ReplyToMessageId { get; set; }
    public DateTime SentAt { get; set; }
}

