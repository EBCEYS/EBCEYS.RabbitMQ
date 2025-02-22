using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EBCEYS.RabbitMQ.Server.Service;

namespace EBCEYS.RabbitMQ.Server.MappedService.Server.Tests.Server
{
    [TestClass()]
    public class RabbitMQServerTests
    {
        private readonly ILogger<RabbitMQServer> serverLogger = new Logger<RabbitMQServer>(new NullLoggerFactory());
        private readonly BasicDeliverEventArgs baseArgs = new("", 1, false, "", "", new BasicProperties()
        {
            ReplyToAddress = new("direct", "", "queue")
        }, new byte[1]);
        [TestMethod()]
        public void RabbitMQServerTest()
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
            using RabbitMQServer server = new(serverLogger, configBuilder.Build());
        }

        [TestMethod()]
        public void SetConsumerActionTest()
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
            using RabbitMQServer server = new(serverLogger, configBuilder.Build());

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                server.SetConsumerAction(null!);
            });

            server.SetConsumerAction((sender, args) =>
            {
                return Task.CompletedTask;
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
            using RabbitMQServer server = new(serverLogger, configBuilder.Build());
            await server.StartAsync(CancellationToken.None);
        }

        [TestMethod()]
        public async Task AckMessageTest()
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
            using RabbitMQServer server = new(serverLogger, configBuilder.Build());

            await server.StartAsync(CancellationToken.None);

            await server.AckMessage(baseArgs);
        }

        [TestMethod()]
        public async Task SendResponseAsyncTest()
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
            using RabbitMQServer server = new(serverLogger, configBuilder.Build());

            await server.StartAsync(CancellationToken.None);

            await server.SendResponseAsync(baseArgs, new { resp = "OK" });
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
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            await server.StartAsync(CancellationToken.None);
            await server.StopAsync(CancellationToken.None);
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
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            await server.DisposeAsync();
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
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            server.Dispose();
        }
    }
}