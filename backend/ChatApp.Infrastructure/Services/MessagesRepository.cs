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

    public async Task<List<Message>> GetChatHistory(Guid user1, Guid user2)
    {
        var chats = await _context.Messages
            .Where(m => (m.SenderId == user1 && m.ReceiverId == user2) || (m.SenderId == user2 && m.ReceiverId == user1))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
        return chats;
    }

    public async Task<List<Message>> GetLatestMessages(Guid userId)
    {
        var messages = await _context.Messages
            .Where(m => (m.SenderId == userId) || (m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var latestMessages = messages
            .GroupBy(m => (m.SenderId == userId ? m.ReceiverId : m.SenderId))
            .Select(g => g.First())
            .ToList();

        return latestMessages;
    }
}
