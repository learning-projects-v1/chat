using ChatApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Persistence;

public class DataSeeder
{

    private readonly ChatAppDbContext _context;

    public DataSeeder(ChatAppDbContext context)
    {
        _context = context;
    }

    public void SeedInitialData()
    {
        var users = new List<User>();
        int numberOfUser = 15;
        for (int i = 1; i <= numberOfUser; i++)
        {
            var user = new User
            {
                Id = GetGuid(i, 'a'), //must be compile time constant
                CreatedAt = DateTime.UtcNow,
                Email = $"u{i}@gmail.com",
                FullName = $"User {i}",
                UserName = $"chat-app-user{i}",
                HashedPassword = "123456",
            };
            users.Add(user);
        }

        var friendships = new List<Friendship>();
        // send friend request to all from user-1
        var userOne = users.First();
        for (int i = 1; i < users.Count; i++)
        {
            var status = FriendshipStatus.Pending;
            if (i > 5) status = FriendshipStatus.Accepted;
            var friendship = new Friendship()
            {
                Id = GetGuid(friendships.Count + 1, 'b'),
                ReceiverId = userOne.Id,
                SenderId = users[i].Id,
                CreatedAt = new DateTime(2025, 6, 8),
                Status = status
            };
            friendships.Add(friendship);
        }
        var userTwo = users[1];
        for (int i = 2; i < users.Count; i++)
        {
            var status = FriendshipStatus.Accepted;
            if (i > 5) status = FriendshipStatus.Pending;
            var friendship = new Friendship()
            {
                Id = GetGuid(friendships.Count + 1, 'b'),
                ReceiverId = userTwo.Id,
                SenderId = users[i].Id,
                CreatedAt = new DateTime(2025, 6, 8),
                Status = status,
            };
            friendships.Add(friendship);
        }

        _context.Users.AddRange(users);
        _context.Friendships.AddRange(friendships);
        _context.SaveChanges();
        //modelBuilder.Entity<User>().HasData(users);
        //modelBuilder.Entity<Friendship>().HasData(friendships);


        //var message1 = new Message
        //{
        //    Id = Guid.NewGuid(),
        //    SenderId = user1.Id,
        //    ReceiverId = user2.Id,
        //    Content = "Hey Bob!",
        //    SentAt = DateTime.UtcNow.AddMinutes(-5)
        //};

        //var message2 = new Message
        //{
        //    Id = Guid.NewGuid(),
        //    SenderId = user2.Id,
        //    ReceiverId = user1.Id,
        //    Content = "Hi Alice! How are you?",
        //    SentAt = DateTime.UtcNow.AddMinutes(-4)
        //};

        //var message3 = new Message
        //{
        //    Id = Guid.NewGuid(),
        //    SenderId = user1.Id,
        //    ReceiverId = user3.Id,
        //    Content = "Hi Charlie!",
        //    SentAt = DateTime.UtcNow.AddMinutes(-3)
        //};

        //modelBuilder.Entity<Message>().HasData(message1, message2, message3);
    }

    private Guid GetGuid(int number, char ch)
    {
        var gd = $"00000000-0000-0000-0000-{ch}{new string('0', 11 - number.ToString().Length)}{number}";
        return Guid.Parse(gd);
    }


}
