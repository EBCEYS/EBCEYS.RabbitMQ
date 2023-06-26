using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleSmartController.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using NLog;
using NLog.Web;

namespace EBCEYS.RabbitMQ.ExampleSmartController
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

            Logger logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSmartRabbitMQController<TestController>(configBuilder.Build());
                })
                .UseNLog()
                .ConfigureLogging(log =>
                {
                    log.ClearProviders();
                    log.AddNLog("nlog.config");
                })
                .Build();

            host.Run();
        }
    }
}