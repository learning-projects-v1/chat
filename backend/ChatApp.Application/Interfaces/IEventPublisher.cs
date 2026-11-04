namespace ChatApp.Application.Interfaces;

public interface IEventPublisher
{
    /// <param name="messageId">
    /// Stable id for this event (we use the chat message id). The consumer's duplicate
    /// guard keys on it, so a retry of the same event must reuse the same id.
    /// </param>
    Task PublishAsync<T>(T evt, string messageId, CancellationToken cancellationToken = default) where T : class;
}
