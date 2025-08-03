using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Models;

public class Reaction
{
    public Guid Id { get; set; }
    public Guid ReactionToMessageId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; }
    public DateTime UpdatedAt { get; set; }

    //// navigation
    public Message ReactionToMessage { get; set; }
    public User User { get; set; }
}
