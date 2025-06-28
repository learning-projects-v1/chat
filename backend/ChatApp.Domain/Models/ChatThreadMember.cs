using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class ChatThreadMember
{
    public Guid Id { get; set; }
    public Guid ChatThreadId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }

}
