using ChatApp.Domain.Models;

namespace ChatApp.Application.Interfaces;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedMessage processedMessage, CancellationToken cancellationToken = default);
}
