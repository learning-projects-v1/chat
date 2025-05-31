using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;
public enum FriendshipStatus
{
    Pending,
    Accepted,
    Rejected
}

public class Friendship
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt = DateTime.UtcNow;

    public User Sender { get; set; }
    public User Receiver { get; set; }
}
