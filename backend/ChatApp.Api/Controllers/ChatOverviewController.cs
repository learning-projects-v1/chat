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
    private readonly IChatThreadRepository _chatThreadRepository;
    private readonly IChatThreadMemberRepository _chatThreadMemberRepository;
    public ChatOverviewController(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork, IMessageRepository messageRepository, IChatThreadRepository chatThreadRepository, IChatThreadMemberRepository chatThreadMemberRepository)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
        _chatThreadRepository = chatThreadRepository;
        _chatThreadMemberRepository = chatThreadMemberRepository;
    }

    [HttpGet()]
    public async Task<IActionResult> GetLatestMessages()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.Name)?.Value);
        var latestMessages = await _messageRepository.GetLatestMessages(userId);
        var orderedMessages = latestMessages.OrderByDescending(c => c.SentAt).ToList();
        var chatDtos = orderedMessages.Select(m => new ChatDto(m)).ToList();
        foreach(var chatDto in chatDtos)
        {
            var threadInfo = await _chatThreadRepository.GetItemByIdAsync(chatDto.ChatThreadId);
            if (threadInfo.IsGroup)
            {
                chatDto.ChatTitle = threadInfo.ThreadName;
            }
            else
            {
                var threadMembers = await _chatThreadMemberRepository.GetThreadMembersAsync(chatDto.ChatThreadId);
                var otherMember = threadMembers.FirstOrDefault(x => x.Id != userId);
                chatDto.ChatTitle = otherMember.UserName;
            }
        }
        return Ok(chatDtos);
    }
}
