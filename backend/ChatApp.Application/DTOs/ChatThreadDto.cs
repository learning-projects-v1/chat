using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ChatThreadDto
{
    public FriendInfoDto FriendInfo { get; set; }
    public List<ChatDto> Chats { get; set; }
}
