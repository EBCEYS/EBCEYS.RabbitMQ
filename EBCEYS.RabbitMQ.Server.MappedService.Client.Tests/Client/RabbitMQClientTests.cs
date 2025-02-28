using System.Linq.Expressions;
using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EBCEYS.RabbitMQ.Server.MappedService.Client.Tests.Client
{
    [TestClass()]
    public class RabbitMQClientTests
    {
        private readonly ILogger<RabbitMQClient> clientLogger = new Logger<RabbitMQClient>(new NullLoggerFactory());
        [TestMethod()]
        public void RabbitMQClientTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build());
        }

        [TestMethod()]
        public void RabbitMQClient_Func_Test()
        {
            using RabbitMQClient client = new(clientLogger, () =>
            {
                RabbitMQConfigurationBuilder configBuilder = new();
                configBuilder.AddConnectionFactory(new()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5675
                });
                configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
                return configBuilder.Build();
            });
        }

        [TestMethod()]
        public async Task StartAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build());

            await client.StartAsync(CancellationToken.None);
        }

        [TestMethod()]
        public async Task StopAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            RabbitMQClient client = new(clientLogger, configBuilder.Build());

            await client.StartAsync(CancellationToken.None);
            await client.StopAsync(CancellationToken.None);
        }

        [TestMethod()]
        public async Task SendMessageAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build());

            await client.StartAsync(CancellationToken.None);

            await client.SendMessageAsync(new()
            {
                Method = "TestMethod"
            });
        }

        [TestMethod()]
        public async Task SendRequestAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            configBuilder.AddCallbackConfiguration(new(new("ResponseQueue", autoDelete: true), TimeSpan.FromSeconds(10.0)));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build());

            await client.StartAsync(CancellationToken.None);

            object? result = await client.SendRequestAsync<object?>(new()
            {
                Method = "TestMethod"
            });

            Assert.AreEqual(default, result);
        }

        [TestMethod()]
        public void DisposeTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            RabbitMQClient client = new(clientLogger, configBuilder.Build());
            client.Dispose();
        }

        [TestMethod()]
        public async Task DisposeAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5675
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            RabbitMQClient client = new(clientLogger, configBuilder.Build());
            await client.DisposeAsync();
        }
    }
}