using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleDockerServer.RabbitMQControllers;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

namespace EBCEYS.RabbitMQ.ExampleDockerServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSmartRabbitMQController<TestRabbitMQController>(CreateDefaultRabbitMQConfig());
            builder.Services.AddSmartRabbitMQController<TestGZipRabbitMQController>(CreateGZipRabbitMQConfig(), new(true));

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            IHost host = builder.Build();
            host.Run();
        }

        private static RabbitMQConfiguration CreateDefaultRabbitMQConfig()
        {
            return new()
            {
                Factory = new()
                {
                    HostName = "rabbitmq",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5672
                },
                ExchangeConfiguration = new ExchangeConfiguration("TestEx", ExchangeTypes.Fanout, durable: false),
                QueueConfiguration = new QueueConfiguration("TestQueue", autoDelete: true),
                QoSConfiguration = new(0, 1, false),
                OnStartConfigs = new()
                {
                    ConnectionReties = 3,
                    DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                    ThrowServerExceptionsOnReceivingResponse = true
                }
            };
        }
        private static RabbitMQConfiguration CreateGZipRabbitMQConfig()
        {
            return new()
            {
                Factory = new()
                {
                    HostName = "rabbitmq",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5672
                },
                ExchangeConfiguration = new ExchangeConfiguration("TestExGZip", ExchangeTypes.Fanout, durable: false),
                QueueConfiguration = new QueueConfiguration("TestQueueGZip", autoDelete: true),
                QoSConfiguration = new(0, 1, false),
                OnStartConfigs = new()
                {
                    ConnectionReties = 3,
                    DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                    ThrowServerExceptionsOnReceivingResponse = true
                }
            };
        }
    }
}