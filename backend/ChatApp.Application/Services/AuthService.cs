using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;

using ChatApp.Domain.Models;

namespace ChatApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenGenerator _tokenGenerator;
    public AuthService(IUserRepository repository, IUnitOfWork unitOfWork, ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
    }
    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _repository.GetUserByEmail(request.Email);
        if(existingUser != null)
        {
            return AuthResult.Failed("User with this email exists");    ///todo: magic string
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Paswword);
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            UserName = request.Username,
            HashedPassword = hashedPassword
        };
        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return AuthResult.Succeeded("User Registered successfully");
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetUserByEmail(request.Email);
        if (user == null || BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return AuthResult.Failed("Username or password is wrong.");
        }

        var token = _tokenGenerator.GenerateToken(user);
        return AuthResult.Succeeded(token.AccessToken, "", user.Id.ToString(), user.Email, user.UserName);
    }

    public Task<TokenResult> RefreshAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResult> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }
}
