using ChatApp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Persistence;

public class ChatAppDbContext : DbContext
{
    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> dbContextOption) : base(dbContextOption)
    {

    }
    public DbSet<TestModel> TestModel { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.HashedPassword)
                .IsRequired();

            entity.HasMany(u => u.FriendRequestsSent)
                .WithOne(f => f.Sender)
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.FriendRequestsReceived)
                .WithOne(f => f.Receiver)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            //entity.HasMany(x => x.SentMessages)
            //    .WithOne(m => m.Sender)
            //    .HasForeignKey(x => x.SenderId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //entity.HasMany(u => u.ReceivedMessages)
            //    .WithOne(m => m.Receiver)
            //    .HasForeignKey(m => m.ReceiverId)
            //    .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Friendship>()
            .HasIndex(f => new { f.SenderId, f.ReceiverId})
            .IsUnique();

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ReplyToMessage)
            .WithMany()
            .HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedInitialData(modelBuilder);
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        var users = new List<User>();
        int numberOfUser = 15;
        for(int i = 1; i <= numberOfUser; i++) {
            var user = new User
            {
                Id = Guid.Parse($"00000000-0000-0000-0000-{new string('0', 12 - i.ToString().Length)}{i}"),
                CreatedAt = DateTime.UtcNow,
                Email = $"User-{i}@gmail.com",
                FullName = $"User {i}",
                UserName = $"user{i}",
                HashedPassword = "123456"
            };
            users.Add(user);
        }
        //var user1 = new User
        //{
        //    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        //    Email = "alice@example.com",
        //    HashedPassword = "hashedpwd1",
        //    CreatedAt = DateTime.UtcNow
        //};

        //var user2 = new User
        //{
        //    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        //    Email = "bob@example.com",
        //    HashedPassword = "hashedpwd2",
        //    CreatedAt = DateTime.UtcNow
        //};

        //var user3 = new User
        //{
        //    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        //    Email = "charlie@example.com",
        //    HashedPassword = "hashedpwd3",
        //    CreatedAt = DateTime.UtcNow
        //};

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

        modelBuilder.Entity<User>().HasData(users);
        //modelBuilder.Entity<Message>().HasData(message1, message2, message3);
    }

}
