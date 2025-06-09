using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UsersController(IUserRepository repository)
    {
        _userRepository = repository;
    }

    [Authorize]
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions()
    {
        var userId =User.FindFirst(ClaimTypes.Name)?.Value;

        var users = await _userRepository.GetSuggestedUsers(userId);
        var rng = new Random();
        int maxAmount = 10;
        var suggestedUsers = users.OrderBy(x => rng.Next()).Take(maxAmount);
        return Ok(suggestedUsers);
    }
}
    