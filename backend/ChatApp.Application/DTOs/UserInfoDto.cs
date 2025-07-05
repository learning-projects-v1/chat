using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class UserInfoDto

{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
}

