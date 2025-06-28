using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/threads/{threadId}/messages")]
[Authorize]
public class MessagesController: ControllerBase
{
    IMessageRepository _messageRepository;
    IUserRepository _userRepository;
    IChatThreadRepository _chatThreadRepository;
    IUnitOfWork _unitOfWork;
    IRealTimeNotifier _realTimeNotifier;
    public MessagesController(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IRealTimeNotifier notifier, IChatThreadRepository chatThreadRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _realTimeNotifier = notifier;
        _chatThreadRepository = chatThreadRepository;
    }

    // get all messages in a thread
    [HttpGet("")]
    public async Task<IActionResult> GetThreadMessages(Guid threadId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var messages = await _messageRepository.GetChats(threadId);
        var chatDtos = messages.Select(m => new ChatDto()
        {
            Content = m.Content,
            SenderId = m.SenderId,
            ChatThreadId = threadId,
            Id = m.Id,
            ReplyToMessageId = m.ReplyToMessageId,
            SentAt = m.SentAt,
        }).ToList();
        var senders = await _chatThreadRepository.GetThreadMembers(threadId);
        var sendersDict = senders.ToDictionary(s => s.Id, t => new UserResponseDto()
        {
            FullName = t.FullName,
            Id = t.Id,
            UserName = t.UserName
        });

        return Ok(new ChatThreadDto()
        {
            Chats = chatDtos,
            SenderInfo = sendersDict
        });
    }


    // send message in a thread
    [HttpPost("")]
    public async Task<IActionResult> SendMessage([FromBody] ChatDto chatDto)
    {
        var message = new Message()
        {
            Content = chatDto.Content,
            IsSeen = chatDto.IsSeen,
            ChatThreadId = chatDto.ChatThreadId,
            ReplyToMessageId = chatDto.ReplyToMessageId,
            SentAt = chatDto.SentAt,
            SenderId = chatDto.SenderId
        };
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var friendInfo = await _userRepository.GetItemByIdAsync(chatDto.SenderId);
        if(friendInfo == null)
        {
            return BadRequest(new { message = "Sender not found" });
        }
        var thread = await _chatThreadRepository.GetItemByIdAsync(chatDto.ChatThreadId);
        if (thread == null) {
            return BadRequest(new { message = "Thread not found" });
        }
        var SenderInfoDto = new SenderInfo()
        {
            Id = friendInfo.Id,
            Username = friendInfo.UserName,
        };
        var payload = new ChatPreviewDto()
        {
            Chat = chatDto,
            SenderInfo = SenderInfoDto,
            Thread = thread
        };

        ///Todo: begin transaction
        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        //await _realTimeNotifier.NotifyMessage(message.ReceiverId, payload);
        var threadMembers = await _chatThreadRepository.GetThreadMembers(chatDto.ChatThreadId);
        var tasks = threadMembers.Where(t => t.Id != chatDto.SenderId)
            .Select(async t =>
            {
                try
                {
                   await _realTimeNotifier.NotifyMessage(t.Id, payload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to notify sent message to user {t.UserName}");
                }
            });
        await Task.WhenAll(tasks);
        chatDto.Id = message.Id;
        return Ok(chatDto); 
    }
}