using ChatApp.Application.DTOs;
using ChatApp.Application.Services;
using ChatApp.Domain.Constants;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace ChatBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ChatAppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly JwtSettings _jwtSettings;
    public AuthController(ChatAppDbContext chatAppDbContext, IAuthService authService, IOptions<JwtSettings>jwtOptions, IConfiguration configuration)
    {
        _context = chatAppDbContext;
        _configuration = configuration;
        _authService = authService;
        _jwtSettings = jwtOptions.Value;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        if(response != null && response.Success)
        {
            return Ok(new { msg = "Registered successfully!" });
        }
        return BadRequest(new { message = response.Message});
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null || !response.Success) return BadRequest(new { msg = response?.Message ?? "Log in failed" });
        
        Response.Cookies.Append(AuthConstants.RefreshTokenKey, response.RefreshToken, new CookieOptions()
        {
            Domain = "myDomain",
            Path = "/",
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.RefreshTokenExpirationHours),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false
        });
        return Ok(new
        {
            AccessToken = response.AccessToken,
        });
    }

    [HttpGet("Test")]
    public async Task<IActionResult> Test(string message)
    {
        var newModel = new TestModel();
        newModel.Message = message;
        newModel.Id = Guid.NewGuid().ToString();
        var res = await _context.TestModel.OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync();
        if(res == null)
        {
            res = newModel;
        }
        _context.TestModel.Add(newModel);
        await _context.SaveChangesAsync();
        return Ok(new {Message = res.Message});
    }
}
