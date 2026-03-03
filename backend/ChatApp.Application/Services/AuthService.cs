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
        if (request == null)
        {
            return AuthResult.Failed("invalid user info");
        }
        var existingUser = await _repository.GetUserByEmail(request.Email);
        if(existingUser != null)
        {
            return AuthResult.Failed("User with this email exists");    ///todo: magic string
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            //FullName = request.FullName,
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

        // test purpose
        if (user == null)
        {
            return AuthResult.Failed("Invalid email or password.");
        }
        var demoUsers = new List<string>
        {
            "u1@gmail.com",
            "u2@gmail.com",
            "u3@gmail.com",
            "u4@gmail.com",
            "u5@gmail.com",
            "u6@gmail.com",
            "u7@gmail.com",
            "u8@gmail.com",
            "u9@gmail.com",
            "u10@gmail.com",
            "u11@gmail.com",
            "u12@gmail.com",
            "u13@gmail.com",
            "u14@gmail.com",
            "u15@gmail.com"
        };
        if (!demoUsers.Contains(user.Email) && !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
        {
            return AuthResult.Failed("Invalid email or password.");
        }

        var token = _tokenGenerator.GenerateToken(user);
        return AuthResult.Succeeded(token.AccessToken, "", user.Id.ToString(), user.Email, user.UserName);
    }

    public Task<TokenResult> RefreshAsync(string token)
    {
        throw new NotImplementedException();
    }
}
