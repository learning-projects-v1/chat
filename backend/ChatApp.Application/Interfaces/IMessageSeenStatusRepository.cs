using ChatApp.Application.DTOs;
using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IMessageSeenStatusRepository : IRepository<MessageSeenStatus>
{
    Task<MessageSeenStatus> GetSeenStatuses(Guid messageId);
    Task BatchUpdateSeenStatusAsync(IEnumerable<MessageSeenStatus> messageSeenStatuses);
}
