using ChatApp.API.Helper;
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
    IMessageSeenStatusRepository _messageSeenStatusRepository;
    IUnitOfWork _unitOfWork;
    IRealTimeNotifier _realTimeNotifier;

    public MessagesController(IMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IRealTimeNotifier notifier, IChatThreadRepository chatThreadRepository, IChatThreadMemberRepository chatThreadMemberRepository, IMessageSeenStatusRepository messageSeenStatusRepository)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _realTimeNotifier = notifier;
        _chatThreadRepository = chatThreadRepository;
        _chatThreadMemberRepository = chatThreadMemberRepository;
        _messageSeenStatusRepository = messageSeenStatusRepository;
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
            .ToList(),
            MessageSeenStatuses = m.SeenStatuses
            .Select(x => new MessageSeenStatusDto { Id = x.Id, MessageId = x.MessageId, SeenAt = x.SeenAt, UserId = x.UserId, ThreadId = m.ChatThreadId})
            .ToList()
        }).ToList();
        var senders = await _chatThreadMemberRepository.GetThreadMembersAsync(threadId);
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
            Id = Guid.NewGuid(),
            Content = chatDto.Content,
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

        var seenStatus = new MessageSeenStatus() { Id = Guid.NewGuid(), MessageId = message.Id, SeenAt = DateTime.UtcNow, UserId = userId };
        ///Todo: begin transaction
        await _messageRepository.AddAsync(message);
        await _messageSeenStatusRepository.AddAsync(seenStatus);
        await _unitOfWork.SaveChangesAsync();

        //await _realTimeNotifier.NotifyMessage(message.ReceiverId, payload);
        chatDto.Id = message.Id;
        var seenStatusDto = new MessageSeenStatusDto() { Id = seenStatus.Id, MessageId = message.Id, SeenAt = seenStatus.SeenAt, ThreadId = message.ChatThreadId, UserId = userId };
        var threadMembers = await _chatThreadMemberRepository.GetThreadMembersAsync(chatDto.ChatThreadId);
        var threadMemberIds = threadMembers.Select(x => x.Id.ToString()).ToList();
        await _realTimeNotifier.NotifyMessageToAll(threadMemberIds, payload);
        await _realTimeNotifier.NotifyMessagesSeenStatus(threadMemberIds, new() { seenStatusDto });
        return Ok(chatDto); 
    }


    [HttpPost("seen-status")]
    public async Task<IActionResult> UpdateMessageSeenStatuses(List<Guid> messageIds, Guid threadId)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var userSeenMessages = await _messageSeenStatusRepository.GetUserSeenMessages(userId);
        var seenThreadMessageIds = userSeenMessages.Where(m => m.Message.ChatThreadId == threadId).Select(x => x.MessageId).ToList();
        var unseenMessageIds = messageIds.Except(seenThreadMessageIds).ToList();
        //var unseenMessages = userSeenMessages.Where(u => (u.Message.ChatThreadId == threadId) && !messageIds.Contains(u.Message.Id));
        var seenStatuses = unseenMessageIds.Select(id => new MessageSeenStatus
        {
            Id = Guid.NewGuid(),
            MessageId = id,
            SeenAt = DateTime.UtcNow,
            UserId = userId
        });
        await _messageSeenStatusRepository.BatchUpdateSeenStatusAsync(seenStatuses);
        await _unitOfWork.SaveChangesAsync();

        var seenStatusDtos = seenStatuses
            .Select(x => new MessageSeenStatusDto() {
                Id = x.Id,
                SeenAt = x.SeenAt,
                MessageId = x.MessageId,
                UserId = userId,
                ThreadId = threadId
            }).ToList();

        var threadMembers = await _chatThreadMemberRepository.GetThreadMembersAsync(threadId);
        await _realTimeNotifier.NotifyMessagesSeenStatus(threadMembers.Select(x => x.Id.ToString()).ToList(), seenStatusDtos);
        return Ok(messageIds);
    }
}