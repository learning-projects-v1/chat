using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class ChatThread
{
    public Guid Id { get; set; }
    public string ThreadName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsGroup { get; set; }

    // navigation
    public ICollection<ChatThreadMember> ThreadMembers { get; set; }
}
