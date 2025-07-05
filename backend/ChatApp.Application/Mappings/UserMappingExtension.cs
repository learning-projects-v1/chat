using ChatApp.Application.DTOs;
using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Mappings;

public static class UserMappingExtension
{
    public static UserInfoDto ToUserResponseDto(this User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.UserName,
        };
    }
}
