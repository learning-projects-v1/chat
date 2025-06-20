using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class ChatPreviewDto
{
    public FriendInfoDto FriendInfo { get; set; }
    public ChatDto Chat { get; set; }
}
