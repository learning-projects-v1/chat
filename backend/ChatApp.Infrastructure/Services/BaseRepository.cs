using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Services;

public class BaseRepository<T> : IRepository<T> where T : class
{
    private readonly ChatAppDbContext _context;
    public BaseRepository(ChatAppDbContext context)
    {
        _context = context;   
    }
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Add<T>(entity);
    }

    public async Task<T?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FindAsync<T>(id);
    }

    public void RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Remove<T>(entity);
    }

    public void UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Update<T>(entity);
    }
}
