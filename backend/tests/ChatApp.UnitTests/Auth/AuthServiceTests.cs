using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Domain.Models;
using NSubstitute;

namespace ChatApp.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly AuthService _sut;

    public AuthServiceTests() => _sut = new AuthService(_users, _unitOfWork, _tokens);

    [Fact]
    public async Task Register_newEmail_SavesUserWithHashedPassword()
    {
        _users.GetUserByEmail("a@test.com").Returns((User?)null);
        User? saved = null;
        await _users.AddAsync(Arg.Do<User>(u => saved = u), Arg.Any<CancellationToken>());

        var result = await _sut.RegisterAsync(new()
        {
            Email = "a@test.com",
            Password = "123456",
            Username = "a"
        });
        
        Assert.NotNull(saved);
        Assert.True(result.Success);
        Assert.True(BCrypt.Net.BCrypt.Verify("123456", saved.HashedPassword));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Register_ExistingEmail_ReturnsFailed_AndDoesNotSave()
    {
        var email = "a@test.com";
        var pass = "123456";
        var user = new User
        {
            Email = email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(pass),
            CreatedAt = DateTime.UtcNow
        };
        _users.GetUserByEmail(email).Returns(user);

        var result = await _sut.RegisterAsync(new ()
        {
            Email = email,
            Password = pass,
            Username = "a"
        });

        Assert.False(result.Success);
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Register_NullRequest_ReturnsFailed()
    {
        var result = await _sut.RegisterAsync(null!);
        Assert.False(result.Success);
        await _users.DidNotReceive().GetUserByEmail(Arg.Any<string>());
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsFailed()
    {
        var email = "a@test.com";
        _users.GetUserByEmail(email).Returns((User?)null);

        var res = await _sut.LoginAsync(new() { Email = email });
        _tokens.DidNotReceive().GenerateToken(Arg.Any<User>());
        Assert.False(res.Success);
    }
 
    [Fact]
    public async Task Login_WrongPassword_ReturnsFailed()
    {
        var email = "a@test.com";
        var pass = "123456";
        _users.GetUserByEmail(email).Returns(new User
            { Email = email, HashedPassword = BCrypt.Net.BCrypt.HashPassword(pass) });
        var res = await _sut.LoginAsync(new() { Email = email, Password = "12345"});
        
        Assert.False(res.Success);
        _tokens.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsTokenFromGenerator()
    {
        var email = "a@test.com";
        var pass = "123456";
        _users.GetUserByEmail(email).Returns(new User
            { Email = email, HashedPassword = BCrypt.Net.BCrypt.HashPassword(pass) });
        _tokens.GenerateToken(Arg.Any<User>()).Returns(new TokenResult(){AccessToken = "Token-123"});
        var res = await _sut.LoginAsync(new() { Email = email, Password = pass});
        
        Assert.True(res.Success);
        Assert.Equal("Token-123", res.AccessToken);
        _tokens.Received(1).GenerateToken(Arg.Any<User>());
    }
}