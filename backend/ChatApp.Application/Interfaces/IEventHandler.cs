namespace ChatApp.Application.Interfaces;

public interface IEventHandler<in T> where T : class
{
    /// <returns>true if the event was handled now; false if it was a duplicate and skipped.</returns>
    Task<bool> HandleAsync(T evt, string messageId, CancellationToken cancellationToken = default);
}
