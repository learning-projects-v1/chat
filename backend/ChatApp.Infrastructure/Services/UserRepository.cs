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
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(x => x.UserName == username);
    }
}
