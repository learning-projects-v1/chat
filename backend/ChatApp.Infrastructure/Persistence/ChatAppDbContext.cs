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
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<ChatThread> ChatThreads { get; set; }
    public DbSet<ChatThreadMember> ChatThreadMembers { get; set; }
    public DbSet<MessageSeenStatus> MessageSeenStatuses { get; set; }

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

            /// user - friendship
            entity.HasMany(u => u.FriendRequestsSent)
                .WithOne(f => f.Sender)
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            /// user - friendship
            entity.HasMany(u => u.FriendRequestsReceived)
                .WithOne(f => f.Receiver)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

        });
        //    /// user - seen_status
        //    entity
        //        .HasOne<MessageSeenStatus>()
        //        .WithMany()
        //        .HasForeignKey(x => x.);
        //});

        modelBuilder.Entity<Friendship>()
            .HasIndex(f => new { f.SenderId, f.ReceiverId})
            .IsUnique();

        modelBuilder.Entity<Message>((entity) =>
        {
            /// message - message
            entity
                .HasOne(m => m.ReplyToMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            /// message - user
            entity
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            ///message - thread
            entity
                .HasOne<ChatThread>()
                .WithMany()
                .HasForeignKey(m => m.ChatThreadId)
                .OnDelete(DeleteBehavior.Restrict);
        });
      
        modelBuilder.Entity<Reaction>((entity) =>
        {
            /// message - reaction
            entity
            .HasOne<Message>(r => r.ReactionToMessage)
            .WithMany()
            .HasForeignKey(m => m.ReactionToMessageId);

            entity
                .HasIndex(r => r.ReactionToMessageId);

            entity
            .HasOne<User>(r => r.User)
            .WithMany()
            .HasForeignKey(m => m.UserId);

            entity
                .HasIndex(r => new { r.ReactionToMessageId, r.UserId})
                .IsUnique();

            entity.Property(r => r.Type)
                  .IsRequired()
                  .HasMaxLength(20);
        });

        //modelBuilder.Entity<ChatThread>(entity =>
        //{

        //});

        modelBuilder.Entity<ChatThreadMember>(entity =>
        {
            entity
                .HasOne<ChatThread>()
                .WithMany()
                .HasForeignKey(c => c.ChatThreadId);

            entity
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChatThreadId);

        });

        modelBuilder.Entity<MessageSeenStatus>(entity =>
        {
            entity.HasOne<Message>()
            .WithMany()
            .HasForeignKey(m => m.MessageId);

            entity.HasIndex(e => e.MessageId);
        });
    }
}
