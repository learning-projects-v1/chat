using ChatApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly ChatDb
    public AuthService()
    {
        
    }
    public Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        
    }
    public Task<AuthResult> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<TokenResult> RefreshAsync(string token)
    {
        throw new NotImplementedException();
    }


}
