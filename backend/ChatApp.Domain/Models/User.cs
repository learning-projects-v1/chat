using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public string UserName { get; set; }
    public bool IsOnline { get; set; }
    public string? FullName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Friendship> FriendRequestsSent { get; set; }
    public ICollection<Friendship> FriendRequestsReceived { get; set; }

    public ICollection<Message> SentMessages { get; set; }
    public ICollection<Message> ReceivedMessages { get; set; }
}
