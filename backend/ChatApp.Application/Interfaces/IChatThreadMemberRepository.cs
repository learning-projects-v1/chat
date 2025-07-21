using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IChatThreadMemberRepository : IRepository<ChatThreadMember>
{
    Task<List<User>> GetThreadMembersAsync(Guid threadId);
    Task<List<ChatThread>> GetAllJoinedThreadsAsync(Guid userId);
    Task<bool> IsMember(Guid threadId, Guid userId);
}
