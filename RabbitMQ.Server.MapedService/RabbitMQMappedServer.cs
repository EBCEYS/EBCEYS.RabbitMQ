using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text.Json;

namespace EBCEYS.RabbitMQ.Server.MappedService
{
    public class RabbitMQMappedServer : IHostedService, IAsyncDisposable, IDisposable
    {
        public RabbitMQServer Server { get; }

        private readonly ILogger logger;
        private readonly IEnumerable<IRabbitMQControllerBase> controllers;

        public RabbitMQMappedServer(ILogger<RabbitMQMappedServer> logger, RabbitMQConfiguration config, IServiceProvider serviceProvider, IEnumerable<IRabbitMQControllerBase> controllers, JsonSerializerOptions? serializerOptions = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.controllers = controllers ?? throw new ArgumentNullException(nameof(controllers));
            Server = new(serviceProvider.GetService<ILogger<RabbitMQServer>>()!, config, ConsumerAction, serializerOptions);

            logger.LogDebug("Create rabbitmq mapped server!");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Server.StartAsync(cancellationToken);
            logger.LogDebug("Start rabbitmq mapped server!");
        }

        private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
        {
            logger.LogDebug("Mapped server get request!");
            try
            {
                foreach (IRabbitMQControllerBase c in controllers)
                {
                    MethodInfo? method = c.GetMethodToExecute(args, Server.SerializerOptions);
                    if (method is null)
                    {
                        continue;
                    }
                    ParameterInfo returnParam = method.ReturnParameter;
                    if (returnParam.ParameterType == typeof(Task))
                    {
                        await c.ProcessRequestAsync(method);
                        break;
                    }
                    object? result = await c.ProcessRequestWithResponseAsync(method);
                    if (result is not null)
                    {
                        //result = Convert.ChangeType(result, returnParam.ParameterType);
                        await Server.SendResponseAsync(args, result);
                        return;
                    }
                }
                Server.AckMessage(args);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on processing message! {@args}", args);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Server.StopAsync(cancellationToken);
        }

        public void Dispose()
        {
            Server.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Server.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}