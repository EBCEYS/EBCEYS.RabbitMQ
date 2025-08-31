using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using NLog;
using NLog.Web;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.ExampleService;

[Obsolete("Old realization")]
public class Program
{
    private static RabbitMQConfigurationBuilder? _configBuilder;

    public static void Main(string[] args)
    {
        _configBuilder = new RabbitMQConfigurationBuilder();
        _configBuilder.AddConnectionFactory(new ConnectionFactory
        {
            HostName = "Kuznetsovy-Server",
            UserName = "ebcey1",
            Password = "123"
        });
        _configBuilder.AddQueueConfiguration(new QueueConfiguration("ExampleQueue", autoDelete: true));

        var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

        var config = _configBuilder.Build();

        logger.Info("RabbitMQ config {@config}", config);

        var host = Host.CreateDefaultBuilder(args)
            .UseNLog()
            .ConfigureLogging(log => { log.AddNLog("nlog.config"); })
            .ConfigureServices(services =>
            {
                services.AddRabbitMQController<ExampleController>();

                services.AddRabbitMQMappedServer(config);
            })
            .Build();
        host.Run();
    }
}