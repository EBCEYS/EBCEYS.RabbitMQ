using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EBCEYS.RabbitMQ.ExampleDockerClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddRabbitMQClient(CreateDefaultRabbitMQConfig());
            builder.Services.AddRabbitMQClient(new GZipedRabbitMQClient(NullLoggerFactory.Instance.CreateLogger<RabbitMQClient>(), CreateGZipRabbitMQConfig()));

            builder.Configuration.AddJsonFile("appsettings.json", false);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
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
                CallBackConfiguration = new(new QueueConfiguration("rabbitmqclient_callback", autoDelete: true), TimeSpan.FromSeconds(10)),
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
                CallBackConfiguration = new(new QueueConfiguration("rabbitmqclient_callback_gziped", autoDelete: true), TimeSpan.FromSeconds(10)),
                OnStartConfigs = new()
                {
                    ConnectionReties = 3,
                    DelayBeforeRetries = TimeSpan.FromSeconds(3.0),
                    ThrowServerExceptionsOnReceivingResponse = true
                }
            };
        }
    }
    public class GZipedRabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration config) : RabbitMQClient(logger, config)
    {
    }
}
