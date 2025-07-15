using ChatApp.API.Helper;
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
    public ReactionController(IReactionRepository reactionRepository, IUnitOfWork unitOfWork, IMessageRepository messageRepository)
    {
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
    }
    [HttpPost("")]
    public async Task<IActionResult> AddReaction(Guid threadId, Guid messageId, ReactionDto reactionDto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var message = await _messageRepository.GetItemByIdAsync(messageId);
        var reaction = new Reaction
        {
            UpdatedAt = DateTime.UtcNow,
            ReactionToMessageId = messageId,
            UserId = userId,
            Type = reactionDto.Type,
        };
        await _reactionRepository.AddAsync(reaction);
        await _unitOfWork.SaveChangesAsync();
        reactionDto.Id = reaction.Id;
        return Ok(reactionDto);
    }

    [HttpDelete("{reactId}")]
    public async Task<IActionResult> DeleteReaction(Guid threadId, Guid messageId, Guid reactId)
    {
        var reaction = await _reactionRepository.GetItemByIdAsync(reactId);
        _reactionRepository.RemoveAsync(reaction);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { message = "deleted" });
    }
}
