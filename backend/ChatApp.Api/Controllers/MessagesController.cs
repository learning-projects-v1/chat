using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Mappings;
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
    IChatThreadMemberRepository _chatThreadMemberRepository;
    IUnitOfWork _unitOfWork;
    IRealTimeNotifier _realTimeNotifier;
    public MessagesController(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IRealTimeNotifier notifier, IChatThreadRepository chatThreadRepository, IChatThreadMemberRepository chatThreadMemberRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _realTimeNotifier = notifier;
        _chatThreadRepository = chatThreadRepository;
        _chatThreadMemberRepository = chatThreadMemberRepository;
    }

    // get all messages in a thread
    [HttpGet("")]
    public async Task<IActionResult> GetThreadContents(Guid threadId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var messages = await _messageRepository.GetChatsWithReactions(threadId);
        var chatDtos = messages.Select(m => new ChatDto()
        {
            Content = m.Content,
            SenderId = m.SenderId,
            ChatThreadId = m.ChatThreadId,
            Id = m.Id,
            ReplyToMessageId = m.ReplyToMessageId,
            SentAt = m.SentAt,
            Reactions = m.Reactions
            .Select( r => new ReactionDto { Id = r.Id, Type = r.Type, SenderId = r.UserId})
            .ToList()
        }).ToList();
        var senders = await _chatThreadMemberRepository.GetThreadMembers(threadId);
        var sendersDict = senders.ToDictionary(s => s.Id, t => new UserInfoDto()
        {
            FullName = t.FullName,
            Id = t.Id,
            Username = t.UserName
        });

        return Ok(new ChatThreadDto()
        {
            Chats = chatDtos,
            MemberInfoList = senders.Select(x => x.ToUserResponseDto()).ToList()
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
        var userInfoDto = new UserInfoDto()
        {
            Id = friendInfo.Id,
            Username = friendInfo.UserName,
        };
        var payload = new ChatPreviewDto()
        {
            Chat = chatDto,
            SenderInfo = userInfoDto
        };

        ///Todo: begin transaction
        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
        //await _realTimeNotifier.NotifyMessage(message.ReceiverId, payload);
        chatDto.Id = message.Id;
        _ = Task.Run(async () =>
        {
            var threadMembers = await _chatThreadMemberRepository.GetThreadMembers(chatDto.ChatThreadId);
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
        });
        return Ok(chatDto); 
    }
}