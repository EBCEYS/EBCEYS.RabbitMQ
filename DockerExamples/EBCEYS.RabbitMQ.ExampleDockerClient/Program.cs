using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

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
            builder.Services.AddRabbitMQClient(CreateDefaultRabbitMQConfig(), TimeSpan.FromSeconds(5.0));

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
                CallBackConfiguration = new(new QueueConfiguration("rabbitmqclient_callback", autoDelete: true)),
                QoSConfiguration = new(0, 1, false)
            };
        }
    }
}
