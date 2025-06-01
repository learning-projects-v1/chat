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
    void DeleteFriendRequest(Friendship friendShip);
    Task<Friendship?> GetFriendshipAsync(Guid senderId, Guid receiverId);
}
