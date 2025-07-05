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
        var (threads, threadMembers) = GetThreadInfo(friendships);
        var threadIds = threadMembers.Where(x => x.UserId == users[0].Id).Select(x => x.ChatThreadId).Distinct().ToList();
        //var messages = GetMessagesForFirstUser(users, threadIds, threadMembers);
        var messages = GetMessages(threadMembers);
        AddDummyUsers(users);
        _context.Users.AddRange(users);
        _context.Friendships.AddRange(friendships);
        _context.ChatThreadMembers.AddRange(threadMembers);
        _context.ChatThreads.AddRange(threads);
        _context.Messages.AddRange(messages);

        _context.SaveChanges();
    }

    private void AddDummyUsers(List<User> users)
    {
        for(int i = 20; i < 24; i++)
        {
            users.Add(CreateUser(i));
        }
    }

    private (List<ChatThread>, List<ChatThreadMember>) GetThreadInfo(List<Friendship> friendships)
    {
        var threads = new List<ChatThread>();
        var threadMembers = new List<ChatThreadMember>();
        foreach (var friendship in friendships) { 
            if(friendship.Status == FriendshipStatus.Accepted)
            {
                var thread = new ChatThread()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    IsGroup = false,
                    ThreadName = "User message"
                };

                var member1 = new ChatThreadMember()
                {
                    Id = Guid.NewGuid(),
                    ChatThreadId = thread.Id,
                    JoinedAt = DateTime.UtcNow,
                    UserId = friendship.SenderId
                };

                var member2 = new ChatThreadMember()
                {
                    Id = Guid.NewGuid(),
                    ChatThreadId = thread.Id,
                    JoinedAt = DateTime.UtcNow,
                    UserId = friendship.ReceiverId
                };

                threads.Add(thread);
                threadMembers.Add(member1);
                threadMembers.Add(member2);
            }
        }
        return (threads, threadMembers);
    }

    private List<User> GetUsers()
    {
        var users = new List<User>();
        int numberOfUser = 15;
        for (int i = 1; i <= numberOfUser; i++)
        {
            users.Add(CreateUser(i));
        }
        return users;
    }

    private User CreateUser(int i)
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
        return user;
    }
    private List<Friendship> GetFriendships(List<User> users)
    {
        var friendships = new List<Friendship>();
        var userOne = users.First();
        // Everyone is friend of 1
        for (int i = 1; i < users.Count; i++)   
        {
            var status = FriendshipStatus.Accepted;
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

    private List<Message> GetMessages(List<ChatThreadMember> members)
    {
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

        foreach (var member in members)
        {
            var messagePerMember = 8;

            for(var i = 0; i < messagePerMember; i++)
            {
                var messageText = customMessageContents[random.Next(customMessageContents.Count)];
                var message = new Message()
                {
                    Id = Guid.NewGuid(),
                    ChatThreadId = member.ChatThreadId,
                    Content = messageText,
                    IsSeen = false,
                    SenderId = member.UserId,
                    SentAt = new DateTime(2025, 6, 11, random.Next(24), random.Next(60), random.Next(60), DateTimeKind.Utc)
                };
                messages.Add(message);
            }
        }
        return messages;
    }
    private List<Message> GetMessagesForFirstUser(List<User> users, List<Guid> threadIds, List<ChatThreadMember> ThreadMembers) {
        var threadInfoDict = ThreadMembers.GroupBy(t => t.ChatThreadId, t => t).ToDictionary( g => g.Key, g => g.ToList());

    
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
            int totalMessages = 5;

            foreach (var (threadId , threadMembers) in threadInfoDict) {
                for(int j = 0; j < totalMessages; j++)
                {
                    var content = customMessageContents[random.Next(customMessageContents.Count)];
                    var memberIndex = random.Next(threadMembers.Count);
                    var message = new Message()
                    {
                        Content = content,
                        Id = Guid.NewGuid(),
                        IsSeen = false,
                        ChatThreadId = threadId,
                        SenderId = threadMembers[memberIndex].UserId,
                        SentAt = new DateTime(2025, 6, 11, random.Next(24), random.Next(60), random.Next(60), DateTimeKind.Utc)
                    };
                    messages.Add(message);


                }
            }
            for(int fdx = 0; fdx < totalMessages; fdx++)
            {
            }
        }
        return messages;
    }
    private Guid GetGuid(int number, char ch)
    {
        var gd = $"00000000-0000-0000-0000-{ch}{new string('0', 11 - number.ToString().Length)}{number}";
        return Guid.Parse(gd);
    }


}
