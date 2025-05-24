using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs;

public class TokenResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }

    public static TokenResult Failed(string message) => new TokenResult
    {
        Success = false,
        Message = message
    };

    public static TokenResult Succeeded(string accessToken, string refreshToken) => new TokenResult
    {
        Success = true,
        AccessToken = accessToken,
        RefreshToken = refreshToken
    };
}

