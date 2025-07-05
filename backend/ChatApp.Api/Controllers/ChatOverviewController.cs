using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatOverviewController: ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageRepository _messageRepository;
    public ChatOverviewController(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork, IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
    }

    [HttpGet()]
    public async Task<IActionResult> GetLatestMessages()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var latestMessages = await _messageRepository.GetLatestMessages(userId);
        var orderedMessages = latestMessages.OrderByDescending(c => c.SentAt).ToList();
        var chatDtos = orderedMessages.Select(m => new ChatDto(m)).ToList();
        return Ok(chatDtos);

        //var friends = await _friendshipRepository.GetAllFriendsAsync(userId);
        //var latestMessages = await _messageRepository.GetLatestMessages(userId);
        //var latestMessagesDict = latestMessages.ToDictionary(m => m.ChatThreadId, m => m);
        //var results = new List<ChatPreviewDto>();

        //foreach(var friend in friends)
        //{
        //    latestMessagesDict.TryGetValue(friend.Id, out var message);
        //    if(message == null)
        //    {
        //        message = new()
        //        {
        //            Content = "No messages yet",
        //            Id = Guid.NewGuid(),
        //            SenderId = Guid.Empty,
        //            ReceiverId = userId,
        //            IsSeen = false,
        //            SentAt = DateTime.MinValue,
        //        };
        //    }
        //    var chatDto = new ChatDto()
        //    {
        //        Id = message.Id,
        //        Content = message.Content ?? "No messages yet",
        //        SentAt = message.SentAt,
        //        IsSeen = false,
        //        ReceiverId = message.ReceiverId,
        //        SenderId = message.SenderId
        //    };

        //    var friendInfoDto = new SenderInfo()
        //    {
        //        Id = friend.Id,
        //        Username = friend.UserName,
        //    };

        //    results.Add(new ChatPreviewDto()
        //    {
        //        Chat = chatDto,
        //        SenderInfo = friendInfoDto,
        //    });
        //}
        //results = results.OrderByDescending(r => r.Chat.SentAt).ToList();
        //return Ok(latestMessages.OrderByDescending(c => c.SentAt).ToList());
    }
}
