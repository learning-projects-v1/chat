using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services;

public class ReactionRepository : BaseRepository<Reaction>, IReactionRepository
{
    private readonly ChatAppDbContext _context;
    public ReactionRepository(ChatAppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Reaction>> GetReactions(Guid messageId)
    {
        return await _context.Reactions.Where(r => r.ReactionToMessageId == messageId).ToListAsync();
    }

    public async Task<List<Reaction>> GetAllReactions(Guid ThreadId)
    {
        
        var reactions = await _context.Messages
            .Where(m => m.ChatThreadId == ThreadId)
            .OrderByDescending(m => m.SentAt)
            .Join(_context.Reactions, a => a.Id, b => b.ReactionToMessageId, (a, b) => b)
            .ToListAsync();
        return reactions;
    }
}
