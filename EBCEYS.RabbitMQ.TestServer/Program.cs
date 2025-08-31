using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using Microsoft.AspNetCore;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.TestServer;

public class Program
{
    public static void Main(string[] args)
    {
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run();
    }
}

public class Startup(IConfiguration configuration)
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        var configBuilder = new RabbitMQConfigurationBuilder();
        configBuilder.AddConnectionFactory(new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"]!,
            UserName = configuration["RabbitMQ:UserName"]!,
            Password = configuration["RabbitMQ:Password"]!,
            Port = int.Parse(configuration["RabbitMQ:Port"]!)
        });
        configBuilder.AddExchangeConfiguration(new ExchangeConfiguration(configuration["RabbitMQ:ExchangeName"]!,
            ExchangeTypes.Fanout));
        configBuilder.AddQueueConfiguration(new QueueConfiguration(configuration["RabbitMQ:QueueName"]!,
            configuration["RabbitMQ:QueueName"]!));
        configBuilder.AddOnStartConfiguration(new RabbitMQOnStartConfigs
        {
            ConnectionReties = 10,
            DelayBeforeRetries = TimeSpan.FromSeconds(1),
            ThrowServerExceptionsOnReceivingResponse = false
        });

        services.AddSmartRabbitMQController<RabbitMQTestController>(configBuilder.Build());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}