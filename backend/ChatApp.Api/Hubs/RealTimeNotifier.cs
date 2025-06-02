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

    public async Task NotifyFriendRequest(Guid ReceiverId, object payload)
    {
        await _hubContext.Clients.Groups(ReceiverId.ToString()).SendAsync("FriendRequestReceived", payload);
    }

    public Task NotifyMessage(Guid ReceiverId, object payload)
    {
        throw new NotImplementedException();
    }
}
