using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private ChatAppDbContext _context;
    public UnitOfWork(ChatAppDbContext context)
    {
        _context = context;
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
