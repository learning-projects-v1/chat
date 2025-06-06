using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FriendsController : ControllerBase
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealTimeNotifier _notifier;

    public FriendsController(IFriendshipRepository friendsRepository, IUnitOfWork unitOfWork, IRealTimeNotifier notifier)
    {
        _friendshipRepository = friendsRepository;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    [Authorize]
    [HttpPost("request")]
    public async Task<IActionResult> SendFriendRequest(string receiverId)
    {
        var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var friendship = new Friendship
        {
            CreatedAt = DateTime.UtcNow,
            ReceiverId = Guid.Parse(receiverId),
            Status = FriendshipStatus.Pending,
            SenderId = Guid.Parse(senderId),
        };
        
        await _friendshipRepository.AddFriendRequestAsync(friendship);
        await _unitOfWork.SaveChangesAsync();

        await _notifier.NotifyFriendRequest(receiverId, new
        {
            SenderId = senderId,
            Message = "You have a new friend request"
        });

        return Ok(new {message = "Friend request sent!"});
    }
}
