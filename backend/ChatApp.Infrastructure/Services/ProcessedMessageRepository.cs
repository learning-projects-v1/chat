using ChatApp.Application.Interfaces;
using ChatApp.Domain.Models;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services;

// Not a BaseRepository<T>: that one is keyed by Guid, and this key is the string MessageId.
public class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly ChatAppDbContext _context;

    public ProcessedMessageRepository(ChatAppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default) =>
        _context.ProcessedMessages.AnyAsync(p => p.MessageId == messageId, cancellationToken);

    public Task AddAsync(ProcessedMessage processedMessage, CancellationToken cancellationToken = default)
    {
        _context.ProcessedMessages.Add(processedMessage);
        return Task.CompletedTask;
    }
}
