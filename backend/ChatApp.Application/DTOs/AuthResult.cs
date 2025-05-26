using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }

    public static AuthResult Failed(string message) => new AuthResult
    {
        Success = false,
        Message = message
    };

    public static AuthResult Succeeded(string accessToken, string refreshToken, string userId, string email, string username)
    => new AuthResult
    {
        Success = true,
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        UserId = userId,
        Email = email,
        Username = username
    };

    public static AuthResult Succeeded(string message)
    {
        return new AuthResult { Success = true, Message = message };
    }


}
