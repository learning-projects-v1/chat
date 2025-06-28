using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IChatThreadRepository : IRepository<ChatThread>
{
    Task<List<User>> GetThreadMembers(Guid threadId);
    Task<List<Thread>> GetJoinedThreads(Guid userId);
}
