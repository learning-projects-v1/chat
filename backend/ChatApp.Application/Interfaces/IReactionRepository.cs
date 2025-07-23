using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IReactionRepository : IRepository<Reaction>
{
    Task<List<Reaction>> GetReactions(Guid messageId);
    Task<List<Reaction>> GetAllReactions(Guid ThreadId);
    Task<Reaction> GetReaction(Guid messageId, Guid userId);
}
