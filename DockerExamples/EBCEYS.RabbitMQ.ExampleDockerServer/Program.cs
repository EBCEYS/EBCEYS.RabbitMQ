using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleDockerServer.RabbitMQControllers;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.ExampleDockerServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSmartRabbitMQController<TestRabbitMQController>(CreateDefaultRabbitMQConfig());
        builder.Services.AddSmartRabbitMQController<TestGZipRabbitMQController>(CreateGZipRabbitMQConfig(),
            new GZipSettings(true));

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var host = builder.Build();
        host.Run();
    }

    private static RabbitMQConfiguration CreateDefaultRabbitMQConfig()
    {
        return new RabbitMQConfiguration
        {
            Factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            },
            ExchangeConfiguration = new ExchangeConfiguration("TestEx", ExchangeTypes.Fanout),
            QueueConfiguration = new QueueConfiguration("TestQueue", autoDelete: true),
            QoSConfiguration = new QoSConfiguration(0, 1, false),
            OnStartConfigs = new RabbitMQOnStartConfigs
            {
                ConnectionReties = 3,
                DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                ThrowServerExceptionsOnReceivingResponse = true
            }
        };
    }

    private static RabbitMQConfiguration CreateGZipRabbitMQConfig()
    {
        return new RabbitMQConfiguration
        {
            Factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            },
            ExchangeConfiguration = new ExchangeConfiguration("TestExGZip", ExchangeTypes.Fanout),
            QueueConfiguration = new QueueConfiguration("TestQueueGZip", autoDelete: true),
            QoSConfiguration = new QoSConfiguration(0, 1, false),
            OnStartConfigs = new RabbitMQOnStartConfigs
            {
                ConnectionReties = 3,
                DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                ThrowServerExceptionsOnReceivingResponse = true
            }
        };
    }
}