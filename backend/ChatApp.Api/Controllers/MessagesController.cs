using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
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
    public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
    {
        var message = new Message()
        {
            Content = chatDto.Content,
            IsSeen = chatDto.IsSeen,
            ReceiverId = chatDto.ReceiverId,
            ReplyToMessageId = chatDto.ReplyToMessageId,
            SentAt = chatDto.SentAt,
            SenderId = chatDto.SenderId
        };
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var friendinfo = await _userRepository.GetByIdAsync(chatDto.ReceiverId);
        var friendInfoDto = new FriendInfoDto()
        {
            Id = friendinfo.Id,
            Username = friendinfo.UserName
        };
        var payload = new ChatPreviewDto()
        {
            Chat = chatDto,
            FriendInfo = friendInfoDto
        };
        ///Todo: begin transaction
        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        await _realTimeNotifier.NotifyMessage(message.ReceiverId, payload);
        chatDto.Id = message.Id;
        return Ok(chatDto);
    }
}