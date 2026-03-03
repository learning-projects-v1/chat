using ChatApp.Application.Interfaces;
using ChatApp.Application.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UsersController(IUserRepository repository)
    {
        _userRepository = repository;
    }


    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string? username = null)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;

        var users = await _userRepository.GetSuggestedUsers(userId, username);

        IEnumerable<Domain.Models.User> suggestedUsers;
        if (string.IsNullOrWhiteSpace(username))
        {
            var rng = new Random();
            const int maxAmount = 10;
            suggestedUsers = users.OrderBy(x => rng.Next()).Take(maxAmount);
        }
        else
        {
            const int maxAmount = 20;
            suggestedUsers = users
                .OrderBy(x => x.UserName)
                .Take(maxAmount);
        }

        return Ok(suggestedUsers.Select(x => x.ToUserResponseDto()).ToList());
    }
}
    
