using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class FriendInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
}
