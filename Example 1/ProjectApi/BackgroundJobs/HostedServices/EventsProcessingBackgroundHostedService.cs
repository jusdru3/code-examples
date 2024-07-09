using ProjectUtilities.Events.Dtos;
using ProjectUtilities.Events.Services.Interfaces;
using ProjectApi.Core.Events.Services;

namespace ProjectApi.BackgroundJobs.HostedServices;

public class EventsProcessingBackgroundHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EventsProcessingBackgroundHostedService> _logger;

    public EventsProcessingBackgroundHostedService(IServiceScopeFactory serviceScopeFactory,
        ILogger<EventsProcessingBackgroundHostedService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWork();
        }
    }

    private async Task DoWork()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var externalQueueService = serviceProvider.GetRequiredService<projectExternalQueueService>();
        var eventProcessor = serviceProvider.GetRequiredService<IEventProcessor>();

        var message = await externalQueueService.GetMessage();

        if (message?.Body is IEvent typedBody)
        {
            _logger.LogInformation($"Processing message {message.MessageId}");

            var failed = await (Task<bool>)eventProcessor.GetType().GetMethod("ProcessEvent")
                .MakeGenericMethod(typedBody.GetType())
                .Invoke(eventProcessor, new object[] { message.MessageId, typedBody });
            if (!failed)
            {
                await externalQueueService.DeleteMessage(message.ReceiptHandle);
                _logger.LogInformation("Deleted message {MessageMessageId}", message.MessageId);
            }
            else
            {
                _logger.LogWarning("One or more event handler has failed, keeping message {MessageMessageId}",
                    message.MessageId);
            }
        }
    }
}