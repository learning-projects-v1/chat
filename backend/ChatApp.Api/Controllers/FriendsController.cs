using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Mappings;
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
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealTimeNotifier _notifier;

    public FriendsController(IFriendshipRepository friendsRepository, IUnitOfWork unitOfWork, IRealTimeNotifier notifier, IUserRepository userRepository)
    {
        _friendshipRepository = friendsRepository;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
        _userRepository = userRepository;
    }

    [Authorize]
    [HttpPost("requests/{receiverId}")]
    public async Task<IActionResult> SendFriendRequest(string receiverId)
    {
        var senderId = User.FindFirst(ClaimTypes.Name)?.Value;
        if(senderId == receiverId)
        {
            return BadRequest(new { Message = "Can't send friend request to self" });
        }
        var existingFriendship = await _friendshipRepository.GetFriendshipAsync(Guid.Parse(senderId), Guid.Parse(receiverId));
        if (existingFriendship != null)
        {
            string message = existingFriendship.Status == FriendshipStatus.Pending ? "Already have pending friend requests" : "Already friends";
            return BadRequest(new { Message = message });
        } 
        var friendship = new Friendship
        {
            CreatedAt = DateTime.UtcNow,
            ReceiverId = Guid.Parse(receiverId),
            Status = FriendshipStatus.Pending,
            SenderId = Guid.Parse(senderId),
        };
        await _friendshipRepository.AddFriendRequestAsync(friendship);
        await _unitOfWork.SaveChangesAsync();

        var sender = await _userRepository.GetItemByIdAsync(Guid.Parse(senderId));
        //need transaction here
        if (sender == null) return BadRequest("Sender not found!");

        await _notifier.NotifyFriendRequest(receiverId, new
        (){
            Sender = sender.ToUserResponseDto(),
            Message = "You have a new friend request"
        });

        return Ok(new {message = "Friend request sent!"});
    }

    [Authorize]
    [HttpPut("requests/{senderId}/accept")]
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
            Select(u => u.ToUserResponseDto());

        return Ok(friendsDto);
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
