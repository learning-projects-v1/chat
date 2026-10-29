using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using ChatApp.Application.Events;
using ChatApp.Application.Interfaces;

namespace ChatApp.Infrastructure.Messaging;

/// <summary>
/// Sends events to Azure Service Bus queues as JSON. Registered as a singleton: the
/// ServiceBusClient and its senders hold an AMQP connection and are meant to be reused.
/// </summary>
public sealed class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusEventPublisher(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task PublishAsync<T>(T evt, string messageId, CancellationToken cancellationToken = default) where T : class
    {
        var queueName = QueueFor<T>();
        var sender = _senders.GetOrAdd(queueName, name => _client.CreateSender(name));

        var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(evt))
        {
            // The consumer's duplicate guard keys on this, so it must be stable per event.
            MessageId = messageId,
            ContentType = "application/json",
            Subject = typeof(T).Name,
        };

        await sender.SendMessageAsync(message, cancellationToken);
    }

    // One queue per event type. Only one event exists today; add a line per new event.
    private static string QueueFor<T>() => typeof(T) == typeof(MessageSent)
        ? QueueNames.MessageSent
        : throw new NotSupportedException($"No queue is configured for event type {typeof(T).Name}.");

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}
