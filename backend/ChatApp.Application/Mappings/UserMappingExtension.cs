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
    public static UserResponseDto ToUserResponseDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
        };
    }
}
