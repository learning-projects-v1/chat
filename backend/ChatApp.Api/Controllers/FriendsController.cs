using ChatApp.Application.DTOs;
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
    [HttpPost("requests/{receiverId}")]
    public async Task<IActionResult> SendFriendRequest(string receiverId)
    {
        var senderId = User.FindFirst(ClaimTypes.Name)?.Value;
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

    [Authorize]
    [HttpPut("request/{senderId}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(string senderId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendShip = await _friendshipRepository.GetFriendshipAsync(Guid.Parse(senderId), Guid.Parse(userId));
        if (friendShip == null) {
            return BadRequest("Friendship not found!");
        }
        
        if(friendShip.Status != FriendshipStatus.Pending)
        {
            return BadRequest("Friendship Status not valid");
        }

        friendShip.Status = FriendshipStatus.Accepted;
        _friendshipRepository.ApproveFriendRequest(friendShip);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Friend request accepted." });
    }

    [Authorize]
    [HttpGet()]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();


        var friends = await _friendshipRepository.GetAllFriendsAsync(Guid.Parse(userId));
        return Ok(friends);
    }

    [Authorize]
    [HttpGet("requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var friends = await _friendshipRepository.GetPendingFriendsAsync(Guid.Parse(userId));
        var friendsDto = friends.
            Select(
                u => new UserResponse {
                    Email = u.Email,
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName,
                }
            );

        return Ok(friendsDto);
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
