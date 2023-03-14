using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Reflection;

namespace EBCEYS.RabbitMQ.Server.MappedService
{
    public class RabbitMQMappedServer : IHostedService, IAsyncDisposable, IDisposable
    {
        public RabbitMQServer Server { get; }

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public RabbitMQMappedServer(ILogger<RabbitMQMappedServer> logger, RabbitMQConfiguration config, IServiceProvider serviceProvider, JsonSerializerSettings? serializerOptions = null)
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
            this.serviceProvider = serviceProvider;
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
                IEnumerable<RabbitMQControllerBase> ctrls = serviceProvider.CreateScope().ServiceProvider.GetServices<RabbitMQControllerBase>();
                foreach (RabbitMQControllerBase c in ctrls)
                {
                    MethodInfo? method = c.GetMethodToExecute(args, Server.SerializerOptions);
                    if (method is null)
                    {
                        continue;
                    }
                    ParameterInfo returnParam = method.ReturnParameter;
                    if (returnParam.ParameterType == typeof(Task) || returnParam.ParameterType == typeof(void))
                    {
                        await c.ProcessRequestAsync(method);
                        break;
                    }
                    object? result = await c.ProcessRequestWithResponseAsync(method);
                    if (result is not null)
                    {
                        await Server.SendResponseAsync(args, result);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on processing message! {@args}", args);
            }
            Server.AckMessage(args);
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