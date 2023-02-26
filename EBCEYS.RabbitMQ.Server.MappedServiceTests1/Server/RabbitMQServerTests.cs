using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EBCEYS.RabbitMQ.Configuration;

namespace EBCEYS.RabbitMQ.Server.Service.Tests
{
    [TestClass()]
    public class RabbitMQServerTests
    {
        private readonly ILogger logger = new Logger<RabbitMQServerTests>(new NullLoggerFactory());
        [TestMethod()]
        public void RabbitMQServerTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            using RabbitMQServer server = new(logger, configBuilder.Build());
            Assert.IsNotNull(server);
        }

        [TestMethod()]
        public void SetConsumerActionTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            using RabbitMQServer server = new(logger, configBuilder.Build());

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                server.SetConsumerAction(null);
            });

            server.SetConsumerAction(async (sender, args) =>
            {
                await Task.Delay(1);
            });

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                server.SetConsumerAction(async (sender, args) =>
                {
                    await Task.Delay(1);
                });
            });
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            using RabbitMQServer server = new(logger, configBuilder.Build());
            await server.StartAsync(CancellationToken.None);
        }

        [TestMethod()]
        public async Task AckMessageTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            using RabbitMQServer server = new(logger, configBuilder.Build());

            await server.StartAsync(CancellationToken.None);

            server.AckMessage(new());
        }

        [TestMethod()]
        public async Task SendResponseAsyncTest()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            using RabbitMQServer server = new(logger, configBuilder.Build());

            await server.StartAsync(CancellationToken.None);

            await server.SendResponseAsync(new(), new { resp = "OK" });
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            await server.StartAsync(CancellationToken.None);
            await server.StopAsync(CancellationToken.None);
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            await server.DisposeAsync();
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            server.Dispose();
        }
    }
}