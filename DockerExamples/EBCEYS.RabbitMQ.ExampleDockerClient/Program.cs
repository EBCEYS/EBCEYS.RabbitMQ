using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.ExampleDockerClient;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddRabbitMQClient(CreateDefaultRabbitMQConfig());
        builder.Services.AddRabbitMQClient(
            new GZipedRabbitMQClient(NullLoggerFactory.Instance.CreateLogger<RabbitMQClient>(),
                CreateGZipRabbitMQConfig()));

        builder.Configuration.AddJsonFile("appsettings.json", false);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
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
            CallBackConfiguration = new CallbackRabbitMQConfiguration(
                new QueueConfiguration("rabbitmqclient_callback", autoDelete: true), TimeSpan.FromSeconds(10)),
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
            CallBackConfiguration = new CallbackRabbitMQConfiguration(
                new QueueConfiguration("rabbitmqclient_callback_gziped", autoDelete: true), TimeSpan.FromSeconds(10)),
            OnStartConfigs = new RabbitMQOnStartConfigs
            {
                ConnectionReties = 3,
                DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                ThrowServerExceptionsOnReceivingResponse = true
            }
        };
    }
}

public class GZipedRabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration config)
    : RabbitMQClient(logger, config)
{
}