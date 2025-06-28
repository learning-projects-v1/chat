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

    public async Task<List<Message>> GetChatOverviews(Guid userId)
    {
        ///todo: need join here
        var threadIds = await _context.ChatThreadParticipents.Where(p => p.UserId == userId).Distinct().Select(t => t.ChatThreadId).ToListAsync();
        var messages = await _context.Messages.Where(m => threadIds.Contains(m.ChatThreadId)).ToListAsync();
        var latestMessages = messages.OrderBy(m => m.SentAt).GroupBy(m => m.ChatThreadId).Select(m => m.First()).ToList();

        //var messages = await _context.Messages
        //    .Where(m => (m.SenderId == userId) || (m.ReceiverId == userId))
        //    .OrderByDescending(m => m.SentAt)
        //    .ToListAsync();

        //var latestMessages = messages
        //    .GroupBy(m => (m.SenderId == userId ? m.ReceiverId : m.SenderId))
        //    .Select(g => g.First())
        //    .ToList();

        return latestMessages;
    }
}
