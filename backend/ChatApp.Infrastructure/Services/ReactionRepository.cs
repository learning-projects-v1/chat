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

    public async Task<List<Reaction>> GetAllReactions(Guid messageId)
    {
        return await _context.Reactions.Where(r => r.MessageId == messageId).ToListAsync();
    }
}
