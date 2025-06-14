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
    public MessagesController(IMessageRepository messageRepository, IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
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
        return Ok(chatDtos);
    }
}
