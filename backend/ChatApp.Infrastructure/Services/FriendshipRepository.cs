using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class FriendshipRepository : BaseRepository<Friendship>, IFriendshipRepository
{
    private readonly ChatAppDbContext _context;
    public FriendshipRepository(ChatAppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AddFriendRequestAsync(Friendship friendship)
    {
        await _context.Friendships.AddAsync(friendship);
    }

    public void ApproveFriendRequest(Friendship friendship)
    {
        friendship.Status = FriendshipStatus.Accepted;
        _context.Friendships.Update(friendship);
    }

    public void DeleteFriendRequest(Friendship friendShip)
    {
        friendShip.Status = FriendshipStatus.Rejected;
        _context.Friendships.Update(friendShip);
    }

    public async Task<IEnumerable<User>> GetAllFriendsAsync(Guid userId)
    {
        var friends = await _context.Friendships
             .Where(f => f.Status == FriendshipStatus.Accepted && (f.SenderId == userId || f.ReceiverId == userId))
             .Include(f => f.Sender)
             .Include(f => f.Receiver)
             .ToListAsync();

        return friends.Select(f => f.SenderId == userId ? f.Receiver : f.Sender);
    }

    public async Task<Friendship?> GetFriendshipAsync(Guid senderId, Guid receiverId)
    {
        return await _context.Friendships.
            FirstOrDefaultAsync(f => (f.SenderId == senderId && f.ReceiverId == receiverId) || (f.SenderId == receiverId && f.ReceiverId == senderId));
    }

    public async Task<IEnumerable<User>> GetPendingFriendsAsync(Guid userId)
    {
        var friends = await _context.Friendships
            .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
            .Include(f => f.Sender)
            .ToListAsync();

        return friends.Select(f => f.Sender);
    }
}
