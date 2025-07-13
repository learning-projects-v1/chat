using ChatApp.API.Helper;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/threads/{threadId}/messages/{messageId}")]
public class ReactionController : ControllerBase
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    public ReactionController(IReactionRepository reactionRepository, IUnitOfWork unitOfWork)
    {
        this._reactionRepository = reactionRepository;
        this._unitOfWork = unitOfWork;
    }
    [HttpPost("")]
    public async Task<IActionResult> AddReaction(Guid threadId, Guid messageId, ReactionDto reactionDto)
    {
        var userId = UserClaimsHelper.GetUserId(User);
        var reaction = new Reaction
        {
            UpdatedAt = DateTime.UtcNow,
            ReactionToMessageId = messageId,
            UserId = userId,
            Type = reactionDto.Type,
        };
        await _reactionRepository.AddAsync(reaction);
        await _unitOfWork.SaveChangesAsync();

        return Ok(reaction);
    }
}
