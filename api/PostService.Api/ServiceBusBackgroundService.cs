using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using PostService.Application.Features.Posts.Commands;
using MediatR;

public class ServiceBusBackgroundService : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<ServiceBusBackgroundService> _logger;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public ServiceBusBackgroundService(IMediator mediator, IServiceProvider serviceProvider, ILogger<ServiceBusBackgroundService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));


        Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("COCKATOO_Q") ?? "";
        var queueName = Environment.GetEnvironmentVariable("COCKATOO_Q_USERNAME_TOPIC") ?? "";

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        _client = new ServiceBusClient(connectionString, clientOptions);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        _logger.LogInformation("Starting the Service Bus processor.");
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();

        // Resolve a scoped mediator instance from the scope
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        string body = args.Message.Body.ToString();
        var trimmedResult = body.Split(":", StringSplitOptions.TrimEntries);

        var authorId = trimmedResult[0];
        var updatedUsername = trimmedResult[1];
        var authorIdParsed = Guid.Parse(authorId);

        await mediator.Send(new UpdateUsernameOnPostsCommand.Command(updatedUsername, authorIdParsed));

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message handler encountered an exception");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping the Service Bus processor.");
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
