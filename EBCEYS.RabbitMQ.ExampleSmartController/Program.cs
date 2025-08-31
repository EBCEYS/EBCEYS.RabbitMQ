using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.ExampleSmartController.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using NLog;
using NLog.Web;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.ExampleSmartController;

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

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSmartRabbitMQController<TestController>(_configBuilder.Build());
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