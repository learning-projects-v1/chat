using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;

namespace ChatApp.Infrastructure.Services;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ChatAppDbContext context) : base(context)
    {
    }
}
