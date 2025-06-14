using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace ChatApp.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    #region Override
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("CONNECTED!!");
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined!");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Cleanup logic if needed
        await base.OnDisconnectedAsync(exception);
    }
    #endregion
    public async Task Register(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

}
