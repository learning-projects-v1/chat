using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatOverviewController: ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUnitOfWork _unitOfWork;
    public ChatOverviewController(IUserRepository userRepository,IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet()]
    public IActionResult GetConnectedUsers()
    {
        var 
        
        return Ok();
    }
}
