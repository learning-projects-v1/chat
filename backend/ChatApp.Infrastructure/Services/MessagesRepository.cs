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
        ///todo: need join here
        //var threadIds = await _context.ChatThreadParticipents.Where(p => p.UserId == userId).Distinct().Select(t => t.ChatThreadId).ToListAsync();
        //var messages = await _context.Messages.Where(m => threadIds.Contains(m.ChatThreadId)).ToListAsync();
        //var latestMessages = messages.OrderBy(m => m.SentAt).GroupBy(m => m.ChatThreadId).Select(m => m.First()).ToList();
        //var messages = await _context.ChatThreadParticipents
        //    .Where(c => c.UserId == userId)
        //    .Select(c => c.ChatThreadId)
        //    .Distinct()
        //    .Join(_context.Messages, a => a, b => b.ChatThreadId, (a, b) => new {
        //        message = b
        //    })
        //    .GroupBy(x => x.message.ChatThreadId, y => y.message)
        //    .Select(g => g.OrderByDescending(h => h.SentAt).First())
        //    .OrderByDescending(c => c.SentAt)
        //    .ToListAsync();

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

        //.Select(g => g.OrderByDescending(h => h.SentAt).First())
        //.OrderByDescending(c => c.SentAt)
        //.ToListAsync();
        return messages;
        //var messages = await _context.Messages
        //    .Where(m => (m.SenderId == userId) || (m.ReceiverId == userId))
        //    .OrderByDescending(m => m.SentAt)
        //    .ToListAsync();

        //var latestMessages = messages
        //    .GroupBy(m => (m.SenderId == userId ? m.ReceiverId : m.SenderId))
        //    .Select(g => g.First())
        //    .ToList();
        //return latestMessages;
    }
}
