using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using Microsoft.Extensions.Logging;

namespace EBCEYS.RabbitMQ.ExampleClient
{
    internal class Program
    {
        private static RabbitMQConfigurationBuilder? configBuilder;
        private static readonly ILogger<RabbitMQClient> logger = new Logger<RabbitMQClient>(LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }));
        static async Task Main()
        {
            configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey2",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("ExampleQueue", autoDelete: true));
            configBuilder.AddCallbackConfiguration(new(new("responseQueue"), TimeSpan.FromSeconds(10.0)));

            logger.LogInformation("Start rabbitMQ client!");
            await RabbitMQClientProcess();
        }
        private static async Task RabbitMQClientProcess()
        {
            RabbitMQClient client = new(logger, configBuilder!.Build());
            await client.StartAsync(CancellationToken.None);

            while (true)
            {
                RabbitMQRequestData request = new()
                {
                    Method = "ExampleMethod",
                    Params = ["asd1", "asd2"]
                };
                logger.LogInformation("Request is {@request}", request);
                string? result = await client.SendRequestAsync<string?>(request);
                logger.LogInformation("Result is: {result}", result);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}