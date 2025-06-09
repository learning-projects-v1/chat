using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IFriendshipRepository : IRepository<Friendship>
{
    Task<IEnumerable<User>> GetAllFriendsAsync(Guid userId);
    Task<IEnumerable<User>> GetPendingFriendsAsync(Guid userId);
    Task AddFriendRequestAsync(Friendship friendship);
    void ApproveFriendRequest(Friendship friendship);
    void RejectFriendRequest(Friendship friendShip);
    /// <summary>
    /// get friendship between 2 users
    /// </summary>
    /// <param name="user1"></param>
    /// <param name="user2"></param>
    /// <returns></returns>
    Task<Friendship?> GetFriendshipAsync(Guid senderId, Guid receiverId);
}
