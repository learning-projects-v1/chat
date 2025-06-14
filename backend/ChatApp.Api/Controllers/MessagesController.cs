using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class MessagesController: ControllerBase
{
    IMessageRepository _messageRepository;
    IUnitOfWork _unitOfWork;
    IUserRepository _userRepository;
    IRealTimeNotifier _realTimeNotifier;
    public MessagesController(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IRealTimeNotifier notifier)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _realTimeNotifier = notifier;
    }

    [HttpGet("thread/{friendId}")]
    public async Task<IActionResult> GetChatHistory(string friendId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var messages = await _messageRepository.GetChatHistory(userId, Guid.Parse(friendId));
        var chatDtos = messages.Select(m => new ChatDto()
        {
            Content = m.Content,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            IsSeen = m.IsSeen,
            Id = m.Id,
            ReplyToMessageId = m.ReplyToMessageId,
            SentAt = m.SentAt,
        }).ToList();

        var friendInfo = await _userRepository.GetByIdAsync(Guid.Parse(friendId));
        return Ok(new ChatThreadDto()
        {
            Chats = chatDtos,
            FriendInfo = new FriendInfoDto()
            {
                Id = Guid.Parse(friendId),
                Username = friendInfo.UserName,
            }
        });
    }


    [HttpPost("send")]
    public async Task SendMessage([FromBody] ChatDto payload)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var chatMessage = new Domain.Models.Message()
        {
            Content = payload.Content,
            IsSeen = false,
            ReceiverId = payload.ReceiverId,
            SenderId = userId,
            ReplyToMessageId = payload.ReplyToMessageId,
            SentAt = payload.SentAt,
            
        };
        ///Todo: begin transaction
        await _messageRepository.AddAsync(chatMessage);
        await _unitOfWork.SaveChangesAsync();
        await _realTimeNotifier.NotifyMessage(payload.ReceiverId, payload);
    }
}
