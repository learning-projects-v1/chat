using Azure.Messaging.ServiceBus;
using ChatApp.API.Workers;
using ChatApp.Application.Events;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Infrastructure.Messaging;

namespace ChatApp.API.Middleware;

public static class MessagingRegistrationMiddleware
{
    /// <summary>
    /// Feature flag "ServiceBus:Enabled":
    ///   true  -> SendMessage publishes to the "message-sent" queue (ConnectionStrings:ServiceBus)
    ///            and NotificationWorker consumes it.
    ///   false -> (default) no queue: InProcessEventPublisher runs the same handler inside the
    ///            request, so the app still works without Service Bus (tests, plain local runs).
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        // Used by both paths.
        services.AddScoped<IEventHandler<MessageSent>, MessageSentHandler>();

        if (!configuration.GetValue<bool>("ServiceBus:Enabled"))
        {
            services.AddScoped<IEventPublisher, InProcessEventPublisher>();
            return services;
        }

        var connectionString = configuration.GetConnectionString("ServiceBus");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Fail at startup rather than on the first message.
            throw new InvalidOperationException(
                "ServiceBus:Enabled is true but ConnectionStrings:ServiceBus is not set.");
        }

        services.AddSingleton(new ServiceBusClient(connectionString));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        services.AddHostedService<NotificationWorker>();
        return services;
    }
}
