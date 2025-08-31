using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.ExampleClient;

internal class Program
{
    private static RabbitMQConfigurationBuilder? _configBuilder;

    private static readonly ILogger<RabbitMQClient> Logger = new Logger<RabbitMQClient>(LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }));

    private static async Task Main()
    {
        _configBuilder = new RabbitMQConfigurationBuilder();
        _configBuilder.AddConnectionFactory(new ConnectionFactory
        {
            HostName = "Kuznetsovy-Server",
            UserName = "ebcey2",
            Password = "123"
        });
        _configBuilder.AddQueueConfiguration(new QueueConfiguration("ExampleQueue", autoDelete: true));
        _configBuilder.AddCallbackConfiguration(
            new CallbackRabbitMQConfiguration(new QueueConfiguration("responseQueue"), TimeSpan.FromSeconds(10.0)));

        Logger.LogInformation("Start rabbitMQ client!");
        await RabbitMQClientProcess();
    }

    private static async Task RabbitMQClientProcess()
    {
        RabbitMQClient client = new(Logger, _configBuilder!.Build());
        await client.StartAsync(CancellationToken.None);

        while (true)
        {
            RabbitMQRequestData request = new()
            {
                Method = "ExampleMethod",
                Params = ["asd1", "asd2"]
            };
            Logger.LogInformation("Request is {@request}", request);
            var result = await client.SendRequestAsync<string?>(request);
            Logger.LogInformation("Result is: {result}", result);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}