using ChatApp.API.Helper;
using ChatApp.API.Hubs;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/threads/{threadId}/messages/{messageId}/reactions")]
public class ReactionController : ControllerBase
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatThreadMemberRepository _chatThreadMemberRepository;
    private readonly IRealTimeNotifier _realTimeNotifier;
    public ReactionController(IReactionRepository reactionRepository, IUnitOfWork unitOfWork, IMessageRepository messageRepository, IChatThreadMemberRepository chatThreadMemberRepository, IRealTimeNotifier notifier
        )
    {
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
        _chatThreadMemberRepository = chatThreadMemberRepository;
        _realTimeNotifier = notifier;
    }
    [HttpPost("")]
    public async Task<IActionResult> AddReaction(ReactionDto reactionDto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var message = await _messageRepository.GetItemByIdAsync(reactionDto.MessageId);
        var reaction = new Reaction
        {
            UpdatedAt = DateTime.UtcNow,
            ReactionToMessageId = reactionDto.MessageId,
            UserId = userId,
            Type = reactionDto.Type,
        };
        await _reactionRepository.AddAsync(reaction);
        await _unitOfWork.SaveChangesAsync();
        reactionDto.Id = reaction.Id;

        var threadParticipents = await _chatThreadMemberRepository.GetThreadMembersAsync(reactionDto.ThreadId);
        await _realTimeNotifier.NotifyReact(threadParticipents.Select(x => x.Id.ToString()).ToList(), reactionDto);
        return Ok(reactionDto);
    }

    [HttpDelete("{reactId}")]
    public async Task<IActionResult> DeleteReaction(Guid threadId, Guid messageId, Guid reactId)
    {
        var reaction = await _reactionRepository.GetItemByIdAsync(reactId);
        _reactionRepository.Remove(reaction);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { message = "deleted" });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateReaction(ReactionDto reactionDto)
    {
        var existingReaction = await _reactionRepository.GetReaction(reactionDto.MessageId, reactionDto.SenderId);
        if (existingReaction == null) {
            var reaction = new Reaction
            {
                Type = reactionDto.Type,
                UpdatedAt = DateTime.UtcNow,
                UserId = reactionDto.SenderId,
                ReactionToMessageId = reactionDto.MessageId,
            };
            await _reactionRepository.AddAsync(reaction);
            await _unitOfWork.SaveChangesAsync();
            reactionDto.Id = reaction.Id;
        }
        else
        {
            if(reactionDto.Type == existingReaction.Type)
            {
                _reactionRepository.Remove(existingReaction);
                reactionDto.Type = "";
            }
            else
            {
                existingReaction.Type = reactionDto.Type;
            }
            await _unitOfWork.SaveChangesAsync();
        }
        var threadMembers = await _chatThreadMemberRepository.GetThreadMembersAsync(reactionDto.ThreadId);
        await _realTimeNotifier.NotifyReact(threadMembers.Select(x => x.Id.ToString()).ToList(), reactionDto);
        return Ok(new { message = "OK" });
    }
}
