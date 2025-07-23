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

    public async Task NotifyMessageToAll(List<string> threadMembers, ChatPreviewDto payload)
    {
        await Task.WhenAll(threadMembers.Select((t) => _hubContext.Clients.Group(t).SendAsync(GlobalConstants.MessageAllNotification, payload)));
    }

    public async Task NotifyReact(List<string> threadMembers, ReactionDto reaction)
    {
        Console.WriteLine("Reaction Notification sent!");
        await Task.WhenAll(threadMembers.Select((t) => _hubContext.Clients.Group(t).SendAsync(GlobalConstants.ReactionNotification, reaction)));
    }
}
