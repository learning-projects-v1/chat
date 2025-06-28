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
    public ChatThreadMemberRepository(ChatAppDbContext context) : base(context)
    {
    }
}
