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
    public DbSet<User> User { get; set; }
}
