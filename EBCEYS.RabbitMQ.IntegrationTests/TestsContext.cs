using System.Collections.Concurrent;
using System.Diagnostics;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.TestServer;
using Microsoft.AspNetCore.Mvc.Testing;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace EBCEYS.RabbitMQ.IntegrationTests;

[SetUpFixture]
public class TestsContext
{
    public const string ExchangeName = "TestExchange";
    public const string QueueName = "TestQueue";

    private RabbitMqContainer _rabbitMqContainer;
    private TestServerApplicationFactory _factory;
    private const string Username = "user";
    private const string Password = "password";
    private const string Hostname = "rabbitmq-tests";

    private static readonly int ExternalPort = Random.Shared.Next(6000, 7000);

    public static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> Tasks = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _rabbitMqContainer = new RabbitMqBuilder().WithUsername(Username).WithPassword(Password).WithHostname(Hostname)
            .WithPortBinding(ExternalPort, 5672)
            .WithCleanUp(true)
            .Build();
        await _rabbitMqContainer.StartAsync();

        _factory = new TestServerApplicationFactory(Username, Password, "localhost", ExternalPort);
        _factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _rabbitMqContainer.StopAsync();
        await _rabbitMqContainer.DisposeAsync();
        await _factory.DisposeAsync();
    }

    internal static RabbitMQConfiguration GetConfiguration()
    {
        return new RabbitMQConfigurationBuilder().AddConnectionFactory(new ConnectionFactory
            {
                HostName = "localhost",
                UserName = Username,
                Password = Password,
                Port = ExternalPort
            }).AddExchangeConfiguration(new ExchangeConfiguration(ExchangeName, ExchangeTypes.Fanout))
            .AddQueueConfiguration(new QueueConfiguration(QueueName, QueueName)).AddOnStartConfiguration(
                new RabbitMQOnStartConfigs
                {
                    ConnectionReties = 10,
                    DelayBeforeRetries = TimeSpan.FromSeconds(1),
                    ThrowServerExceptionsOnReceivingResponse = true
                }).AddCallbackConfiguration(new CallbackRabbitMQConfiguration(new QueueConfiguration("callback_qq"),
                TimeSpan.FromSeconds(5), new ExchangeConfiguration(ExchangeName + "callback", ExchangeTypes.Fanout)))
            .Build();
    }

    internal static RabbitMQConfiguration GetConfigurationGZiped()
    {
        return new RabbitMQConfigurationBuilder().AddConnectionFactory(new ConnectionFactory
            {
                HostName = "localhost",
                UserName = Username,
                Password = Password,
                Port = ExternalPort
            }).AddExchangeConfiguration(new ExchangeConfiguration(ExchangeName, ExchangeTypes.Fanout))
            .AddQueueConfiguration(new QueueConfiguration(QueueName + "gziped")).AddOnStartConfiguration(
                new RabbitMQOnStartConfigs
                {
                    ConnectionReties = 10,
                    DelayBeforeRetries = TimeSpan.FromSeconds(1),
                    ThrowServerExceptionsOnReceivingResponse = true
                }).AddCallbackConfiguration(new CallbackRabbitMQConfiguration(new QueueConfiguration("callback_qq"),
                TimeSpan.FromSeconds(5),
                new ExchangeConfiguration(ExchangeName + "callback_gziped", ExchangeTypes.Fanout))).Build();
    }
}

public class TestServerApplicationFactory(string username, string password, string hostname, int port)
    : WebApplicationFactory<Startup>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureLogging(opts =>
            {
                opts.ClearProviders();
                opts.SetMinimumLevel(LogLevel.Trace);
                opts.AddConsole();
            })
            .ConfigureWebHostDefaults(web =>
                web.UseEnvironment("Development").UseStartup<TestStartup>());
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Sources.Clear();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "RabbitMQ:HostName", hostname },
            { "RabbitMQ:Port", port.ToString() },
            { "RabbitMQ:UserName", username },
            { "RabbitMQ:Password", password },
            { "RabbitMQ:ExchangeName", TestsContext.ExchangeName },
            { "RabbitMQ:QueueName", TestsContext.QueueName }
        });
    }

    private class TestStartup(IConfiguration configuration) : Startup(configuration)
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(TestsContext.Tasks);
            base.ConfigureServices(services);
        }
    }
}