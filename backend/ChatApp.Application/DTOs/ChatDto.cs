using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

// todo: later add IsSeen for optimizing current user seen status get/update
public class ChatDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid SenderId { get; set; }
    public Guid ChatThreadId { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsSeen { get; set; } = false;
    public List<MessageSeenStatus>? MessageSeenStatuses { get; set; }
    public List<ReactionDto>? Reactions { get; set; }
    public string? ChatTitle { get; set; }
    public ChatDto()
    {
           
    }
    public ChatDto(Message message)
    {
        Id = message.Id;
        Content = message.Content;
        SenderId = message.SenderId;
        ChatThreadId = message.ChatThreadId;
        ReplyToMessageId = message.ReplyToMessageId;
        SentAt = message.SentAt;
    }
}

