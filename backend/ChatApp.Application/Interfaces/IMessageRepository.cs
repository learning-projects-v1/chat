using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<List<Message>> GetAll(string userId);
    Task<List<Message>> GetLatestMessages(Guid userId);
    Task<List<Message>> GetChatHistory(Guid userId, Guid friendId);
}
