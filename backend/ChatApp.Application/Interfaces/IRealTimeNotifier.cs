using ChatApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IRealTimeNotifier
{
    Task NotifyFriendRequest(string ReceiverId, FriendRequestResponse payload);
    Task NotifyMessage(Guid ReceiverId, ChatPreviewDto payload);
    Task NotifyMessageToAll(List<string> threadMembers, ChatPreviewDto payload);
    Task NotifyReact(List<string> threadMembers, ReactionDto reaction);
}
