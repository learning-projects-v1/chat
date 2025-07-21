using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class ChatThreadMemberRepository : BaseRepository<ChatThreadMember>, IChatThreadMemberRepository
{
    private readonly ChatAppDbContext _context;
    
    public ChatThreadMemberRepository(ChatAppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ChatThread>> GetAllJoinedThreadsAsync(Guid userId)
    {
       return  await _context.ChatThreadMembers
            .Where(x => x.UserId == userId)
            .Select(x => x.ChatThreadId)
            .Join(_context.ChatThreads, a => a, b => b.Id, (x, y) =>
              y
              ).ToListAsync();
    }

    public async Task<List<User>> GetThreadMembersAsync(Guid threadId)
    {
        /// maybe use navigation properties later instead of join
        var y = await _context.ChatThreadMembers
            .Where(x => x.ChatThreadId == threadId)
            .Join(_context.Users, a => a.UserId, b => b.Id, (a,b) => b)
            .ToListAsync();
        return y;
    }

    public async Task<bool> IsMember(Guid threadId, Guid userId)
    {
        var threadMember = await _context.ChatThreadMembers
            .Where(x => x.ChatThreadId == threadId && x.UserId == userId)
            .ToListAsync();
        return threadMember != null;
    }
}
