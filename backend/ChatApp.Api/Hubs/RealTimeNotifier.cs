using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
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
        await _hubContext.Clients.Groups(ReceiverId).SendAsync("FriendRequestReceived", payload);
    }

    public async Task NotifyMessage(string ReceiverId, object payload)
    {
        await _hubContext.Clients.Groups(ReceiverId).SendAsync("FriendRequestReceived", payload);
    }
}
