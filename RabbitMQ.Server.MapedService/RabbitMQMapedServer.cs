using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Server.MapedService.Controllers;
using RabbitMQServerService;
using System.Reflection;

namespace RabbitMQ.Server.MapedService
{
    public class RabbitMQMapedServer : IHostedService, IAsyncDisposable, IDisposable
    {
        public RabbitMQServer Server { get; }

        private readonly ILogger logger;
        private readonly IEnumerable<RabbitMQControllerBase>? controllers;

        public RabbitMQMapedServer(ILogger logger, Func<RabbitMQServer> serverCreation, IEnumerable<RabbitMQControllerBase>? controllers = null)
        {
            if (serverCreation is null)
            {
                throw new ArgumentNullException(nameof(serverCreation));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Server = serverCreation.Invoke();
            this.controllers = controllers;
        }
        public RabbitMQMapedServer(ILogger logger, RabbitMQServer server, IEnumerable<RabbitMQControllerBase>? controllers = null)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Server = server ?? throw new ArgumentNullException(nameof(server));
            this.controllers = controllers;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Server.SetConsumerAction(ConsumerAction);
            await Server.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                if (controllers is null)
                {
                    return;
                }
                foreach (RabbitMQControllerBase c in controllers)
                {
                    MethodInfo? method = c.GetMethodToExecute(args);
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
                        result = Convert.ChangeType(result, returnParam.ParameterType);
                        await Server.SendResponseAsync(args, result);
                        return;
                    }
                }
                Server.AckMessage(args);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on processing message!");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Server.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            DisposeAllControllers();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Server.DisposeAsync();
            DisposeAllControllers();
            GC.SuppressFinalize(this);
        }

        private void DisposeAllControllers()
        {
            if (controllers == null)
            {
                return;
            }
            foreach(RabbitMQControllerBase controller in controllers)
            {
                controller.Dispose();
            }
        }
    }
}