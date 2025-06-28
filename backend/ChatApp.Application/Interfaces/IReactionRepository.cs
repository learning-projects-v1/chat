using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IReactionRepository : IRepository<Reaction>
{
    Task<List<Reaction>> GetAllReactions(Guid messageId);
}
