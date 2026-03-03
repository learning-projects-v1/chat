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

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly ChatAppDbContext _context;
    public UserRepository(ChatAppDbContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users
            .Include(x => x.FriendRequestsSent)
                .ThenInclude(y => y.Receiver)
            .Include(x => x.FriendRequestsReceived)
                .ThenInclude(y => y.Sender)
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<List<User>> GetSuggestedUsers(string userId, string? usernameQuery = null)
    {
        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            return new List<User>();
        }

        var friendIds = await _context.Set<Friendship>()
            .Where(f => f.SenderId == userIdGuid || f.ReceiverId == userIdGuid)
            .Select(f => f.SenderId == userIdGuid? f.ReceiverId: f.SenderId)
            .ToListAsync();

        var suggestedUsersQuery = _context.Set<User>()
            .AsNoTracking()
            .Where(u => u.Id != userIdGuid && !friendIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(usernameQuery))
        {
            var normalizedQuery = usernameQuery.Trim().ToLower();
            suggestedUsersQuery = suggestedUsersQuery.Where(u =>
                u.UserName.ToLower().Contains(normalizedQuery));
        }

        return await suggestedUsersQuery.ToListAsync();
    }

}
