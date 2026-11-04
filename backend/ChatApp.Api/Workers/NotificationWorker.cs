using Azure.Messaging.ServiceBus;
using ChatApp.Application.Events;
using ChatApp.Application.Interfaces;

namespace ChatApp.API.Workers;

/// <summary>
/// Consumes the "message-sent" queue: for each message, runs MessageSentHandler
/// (notification rows + SignalR push), then completes the message.
///
/// Lives in the API host, not Infrastructure, because it needs the host's DI container
/// and its SignalR hub: IRealTimeNotifier is implemented here (Hubs/RealTimeNotifier) and
/// pushes to the clients connected to this process. Running it in the same process is the
/// simplest setup; it could move to its own worker app later (it would then need a SignalR
/// backplane, e.g. Azure SignalR Service, to reach clients connected to the API).
/// </summary>
public sealed class NotificationWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationWorker> _logger;
    private ServiceBusProcessor? _processor;

    public NotificationWorker(ServiceBusClient client, IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor(QueueNames.MessageSent, new ServiceBusProcessorOptions
        {
            // We complete each message ourselves, and only after the handler succeeded.
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 4,
        });
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // host is shutting down
        }
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        var evt = args.Message.Body.ToObjectFromJson<MessageSent>()
                  ?? throw new InvalidOperationException($"Message {args.Message.MessageId} has an empty body.");

        // A fresh DI scope per message: its own DbContext, like one HTTP request.
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<MessageSent>>();

        // No try/catch on purpose. If the handler throws (DB down, bug, ...), the processor
        // reports it to HandleErrorAsync and abandons the message, so Service Bus delivers it
        // again (DeliveryCount + 1). After MaxDeliveryCount attempts (5, see Config.json for the
        // emulator) Service Bus moves it to the queue's dead-letter queue for a human to look at.
        var handled = await handler.HandleAsync(evt, args.Message.MessageId, args.CancellationToken);
        if (handled)
        {
            // The line to look for in the Container Apps logs when checking the Azure flow end to end.
            _logger.LogInformation("Processed MessageSent {MessageId} from the {Queue} queue (attempt {DeliveryCount}).",
                args.Message.MessageId, QueueNames.MessageSent, args.Message.DeliveryCount);
        }
        else
        {
            _logger.LogInformation("Duplicate delivery of message {MessageId} (attempt {DeliveryCount}); skipped.",
                args.Message.MessageId, args.Message.DeliveryCount);
        }

        // Duplicates are completed too: they've been handled before, so remove them from the queue.
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error ({Source}) on {EntityPath}.", args.ErrorSource, args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
