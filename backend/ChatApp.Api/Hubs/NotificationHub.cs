using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ChatApp.API.Hubs;

public class NotificationHub : Hub
{
    public async Task Register(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Cleanup logic if needed
        await base.OnDisconnectedAsync(exception);
    }
}