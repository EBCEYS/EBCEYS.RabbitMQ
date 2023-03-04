using EBCEYS.RabbitMQ.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EBCEYS.RabbitMQ.Client.Tests
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
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build());
            Assert.IsNotNull(client);
        }

        [TestMethod()]
        public void RabbitMQClient_Func_Test()
        {
            using RabbitMQClient client = new(clientLogger, () =>
            {
                RabbitMQConfigurationBuilder configBuilder = new();
                configBuilder.AddConnectionFactory(new()
                {
                    HostName = "Kuznetsovy-Server",
                    UserName = "ebcey1",
                    Password = "123"
                });
                configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
                return configBuilder.Build();
            });
            Assert.IsNotNull(client);
        }

        [TestMethod()]
        public async Task StartAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
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
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
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
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete:true));
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
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            using RabbitMQClient client = new(clientLogger, configBuilder.Build(), TimeSpan.FromSeconds(1));

            await client.StartAsync(CancellationToken.None);

            object? result = await client.SendRequestAsync<object?>(new()
            {
                Method = "TestMethod"
            });

            Assert.IsTrue(result == default);
        }

        [TestMethod()]
        public void DisposeTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
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
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            RabbitMQClient client = new(clientLogger, configBuilder.Build());
            await client.DisposeAsync();
        }
    }
}