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

        var users = GetUsers();
        var friendships = GetFriendships(users);
        var messages = GetMessages(users);

        _context.Users.AddRange(users);
        _context.Friendships.AddRange(friendships);
        _context.Messages.AddRange(messages);
        _context.SaveChanges();
    }
    
    private List<User> GetUsers()
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
        return users;
    }

    private List<Friendship> GetFriendships(List<User> users)
    {
        var friendships = new List<Friendship>();
        var userOne = users.First();
        // Everyone is friend of 1
        for (int i = 1; i < users.Count; i++)   
        {
            var status = FriendshipStatus.Accepted;
            //if (i > 5) status = FriendshipStatus.Accepted;
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

        // [1,5] is friend of 2. Rest has pending requests
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
        return friendships;
    }

    private List<Message> GetMessages(List<User> users) {

        var messages = new List<Message>();
        var customMessageContents = new List<string>()
        {
            "Hello world",
            "How are you",
            "Yo what's up",
            "hello hello",
            "ki ase jibone"
        };
        var random = new Random();
        for (int i = 1; i < users.Count; i++)
        {
            var content = customMessageContents[random.Next(customMessageContents.Count)];
            var message = new Message()
            {
                Content = content,
                Id = Guid.NewGuid(),
                IsSeen = false,
                ReceiverId = users[0].Id,
                SenderId = users[i].Id,
                SentAt = new DateTime(2025, 6, 11, random.Next(24), random.Next(60), random.Next(60), DateTimeKind.Utc)
            };
            messages.Add(message);
        }
        return messages;
    }
    private Guid GetGuid(int number, char ch)
    {
        var gd = $"00000000-0000-0000-0000-{ch}{new string('0', 11 - number.ToString().Length)}{number}";
        return Guid.Parse(gd);
    }


}
