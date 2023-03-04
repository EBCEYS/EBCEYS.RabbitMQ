using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using NLog;
using NLog.Web;

namespace EBCEYS.RabbitMQ.ExampleService
{
    public class Program
    {
        private static RabbitMQConfigurationBuilder? configBuilder;
        
        public static void Main(string[] args)
        {
            configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("ExampleQueue", autoDelete: true));

            Logger logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            RabbitMQConfiguration config = configBuilder.Build();

            logger.Info("RabbitMQ config {@config}", config);

            IHost host = Host.CreateDefaultBuilder(args)
                .UseNLog()
                .ConfigureLogging(log =>
                {
                    log.AddNLog("nlog.config");
                })
                .ConfigureServices(services =>
                {
                    services.AddRabbitMQController<ExampleController>();

                    services.AddRabbitMQMappedServer(config);

                    services.FixRabbitMQControllers();
                })
                .Build();
            host.Run();
        }
    }
}