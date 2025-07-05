using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class ChatThreadRepository : BaseRepository<ChatThread>, IChatThreadRepository
{
    private readonly ChatAppDbContext _context;
    public ChatThreadRepository(ChatAppDbContext context) : base(context)
    {
        _context = context;
    }
}
