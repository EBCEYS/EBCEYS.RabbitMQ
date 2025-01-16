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
                QueueConfiguration = new QueueConfiguration("TestQueue1", autoDelete: true)
            };
        }
    }
}