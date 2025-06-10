using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class LastMessageDto
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public Guid LastMessageSender { get; set; }
}
