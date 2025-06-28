using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Reflection.Metadata;
namespace ChatApp.API.Hubs;

public class RealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public RealTimeNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyFriendRequest(string ReceiverId, FriendRequestResponse payload)
    {
        await _hubContext.Clients.Groups(ReceiverId).SendAsync(GlobalConstants.FriendRequestReceived, payload);
    }

    public async Task NotifyMessage(Guid ReceiverId, ChatPreviewDto payload)
    {
        await _hubContext.Clients.Groups(ReceiverId.ToString()).SendAsync(GlobalConstants.MessageReceived, payload);
    }

    public Task NotifyMessageToAll(Guid threadId, ChatPreviewDto payload)
    {
        throw new NotImplementedException();
    }
}
