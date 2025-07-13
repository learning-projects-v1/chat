using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services;

public class MessagesRepository : BaseRepository<Message>, IMessageRepository
{
    private readonly ChatAppDbContext _context;
    public MessagesRepository(ChatAppDbContext context): base(context)
    {
        _context = context;
    }

    public Task<List<Message>> GetAll(string userId)
    {
        return default;
    }

    public async Task<List<Message>> GetChats(Guid threadId)
    {
        var chats = await _context.Messages
            .Where(m => m.ChatThreadId == threadId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
        return chats;
    }

    public async Task<List<Message>> GetLatestMessages(Guid userId)
    {
        var messages = await _context.ChatThreadMembers
            .Where(c => c.UserId == userId)
            .Select(c => c.ChatThreadId)
            .Distinct()
            .Join(_context.Messages, a => a, b => b.ChatThreadId, (a, b) => new
            {
                message = b
            })
            .GroupBy(x => x.message.ChatThreadId, y => y.message)
            .Select(g => g.OrderByDescending(h => h.SentAt).First())
            //.OrderByDescending(c => c.SentAt) ///gives error: The given key 'EmptyProjectionMember' was not present in the dictionary.
            .ToListAsync();

        return messages;
    }

    public async Task<List<Message>> GetChatsWithReactions(Guid threadId)
    {
        var reactionsWithUsers = _context.Reactions.Include(r => r.User);
        var messages = await _context.Messages
            .Where(m => m.ChatThreadId == threadId)
            .OrderByDescending(m => m.SentAt)           /// may use limit later
            .GroupJoin(reactionsWithUsers, a => a.Id, b => b.ReactionToMessageId, (a, b) => new Message {
                Id = a.Id,
                ChatThreadId = a.ChatThreadId,
                Content = a.Content,
                ReplyToMessageId = a.ReplyToMessageId,
                SenderId = a.SenderId,
                SentAt = a.SentAt,
                Reactions = b.ToList()
            })
            .ToListAsync();

        return messages;
    }

}
