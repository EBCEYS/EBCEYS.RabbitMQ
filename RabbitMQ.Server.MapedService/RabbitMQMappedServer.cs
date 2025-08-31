using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService;

/// <summary>
///     A <see cref="RabbitMQMappedServer" /> class.
/// </summary>
[Obsolete($"It's better to use {nameof(RabbitMQSmartControllerBase)}")]
public class RabbitMQMappedServer : IHostedService, IAsyncDisposable, IDisposable
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQMappedServer" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="config">The rabbitmq configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="serializerOptions">The serializer options.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RabbitMQMappedServer(ILogger<RabbitMQMappedServer> logger, RabbitMQConfiguration config,
        IServiceProvider serviceProvider, JsonSerializerSettings? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._serviceProvider = serviceProvider;
        Server = new RabbitMQServer(serviceProvider.GetService<ILogger<RabbitMQServer>>()!, config, ConsumerAction,
            serializerOptions);

        logger.LogDebug("Create rabbitmq mapped server!");
    }

    /// <summary>
    ///     The rabbitmq server.
    /// </summary>
    public RabbitMQServer Server { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Server.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Server.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Server.StartAsync(cancellationToken);
        _logger.LogDebug("Start rabbitmq mapped server!");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Server.StopAsync(cancellationToken);
    }

    private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
    {
        _logger.LogDebug("Mapped server get request!");
        try
        {
            var ctrls = _serviceProvider.CreateScope().ServiceProvider.GetServices<RabbitMQControllerBase>();
            foreach (var c in ctrls)
            {
                var method = c.GetMethodToExecute(args, Server.SerializerOptions);
                if (method is null) continue;
                var returnParam = method.ReturnParameter;
                if (returnParam.ParameterType == typeof(Task) || returnParam.ParameterType == typeof(void))
                {
                    await c.ProcessRequestAsync(method);
                    break;
                }

                var result = await c.ProcessRequestWithResponseAsync(method);
                if (result is not null)
                {
                    await Server.SendResponseAsync(args, result, GZipSettings.Default);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on processing message! {@args}", args);
        }

        await Server.AckMessage(args);
    }
}