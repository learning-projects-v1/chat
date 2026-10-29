using ChatApp.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Infrastructure.Messaging;

/// <summary>
/// Fallback when Service Bus is switched off (ServiceBus:Enabled = false): runs the same
/// handler the NotificationWorker would run, but straight away, inside the HTTP request.
/// Same result for the user, without a queue: no retries, and a failure fails the request.
/// </summary>
public sealed class InProcessEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _services;

    public InProcessEventPublisher(IServiceProvider services)
    {
        _services = services;
    }

    public Task PublishAsync<T>(T evt, string messageId, CancellationToken cancellationToken = default) where T : class =>
        _services.GetRequiredService<IEventHandler<T>>().HandleAsync(evt, messageId, cancellationToken);
}
