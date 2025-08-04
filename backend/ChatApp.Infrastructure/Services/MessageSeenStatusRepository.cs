using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class MessageSeenStatusRepository : BaseRepository<MessageSeenStatus>, IMessageSeenStatusRepository
{
    private readonly ChatAppDbContext _context;
    public MessageSeenStatusRepository(ChatAppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task BatchUpdateSeenStatusAsync(IEnumerable<MessageSeenStatus> messageSeenStatuses)
    {
        await _context.MessageSeenStatuses.AddRangeAsync(messageSeenStatuses);
    }

    public Task<MessageSeenStatus> GetSeenStatuses(Guid messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<MessageSeenStatus>> GetUserSeenMessages(Guid userId)
    {
        return await _context.MessageSeenStatuses.Where(m => m.UserId == userId).Include(m => m.Message).ToListAsync();
    }
}
