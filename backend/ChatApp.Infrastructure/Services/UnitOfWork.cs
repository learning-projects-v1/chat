using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatApp.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    // PostgreSQL error code for unique_violation (covers primary keys and unique indexes).
    private const string UniqueViolation = "23505";

    private readonly ChatAppDbContext _context;

    public UnitOfWork(ChatAppDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation } pg)
        {
            throw new DuplicateKeyException($"Unique constraint '{pg.ConstraintName}' was violated.", ex);
        }
    }
}
