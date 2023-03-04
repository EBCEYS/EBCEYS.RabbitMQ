using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EBCEYS.RabbitMQ.Server.MappedService.Tests
{
    [TestClass()]
    public class RabbitMQMappedServerTests
    {
        private readonly ILogger<RabbitMQServer> serverLogger = new Logger<RabbitMQServer>(new NullLoggerFactory());
        private readonly ILogger<RabbitMQMappedServer> mappedServerLogger = new Logger<RabbitMQMappedServer>(new NullLoggerFactory());
        [TestMethod()]
        public void Ctor_Func_Test()
        {
            using RabbitMQMappedServer server = new(mappedServerLogger, () =>
            {
                RabbitMQConfigurationBuilder configBuilder = new();
                configBuilder.AddConnectionFactory(new()
                {
                    HostName = "Kuznetsovy-Server",
                    UserName = "ebcey1",
                    Password = "123"
                });
                configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
                return new RabbitMQServer(serverLogger, configBuilder.Build());
            });
            Assert.IsNotNull(server);
        }
        [TestMethod()]
        public void Ctor_Server_Test()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            using RabbitMQMappedServer mapedServer = new(mappedServerLogger, server);
            Assert.IsNotNull(mapedServer);
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
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            using RabbitMQMappedServer mapedServer = new(mappedServerLogger, server);

            await mapedServer.StartAsync(CancellationToken.None);
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
            RabbitMQServer server = new(serverLogger, configBuilder.Build());
            RabbitMQMappedServer mapedServer = new(mappedServerLogger, server);

            await mapedServer.StartAsync(CancellationToken.None);
            await mapedServer.StopAsync(CancellationToken.None);
        }
    }
}