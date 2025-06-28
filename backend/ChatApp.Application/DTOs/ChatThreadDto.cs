using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ChatThreadDto
{
    public Dictionary<Guid, UserResponseDto> SenderInfo { get; set; }
    public List<ChatDto> Chats { get; set; }
}
