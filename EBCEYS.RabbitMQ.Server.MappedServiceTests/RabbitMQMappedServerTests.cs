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
        private readonly ILogger logger = new Logger<RabbitMQMappedServerTests>(new NullLoggerFactory());
        [TestMethod()]
        public void Ctor_Func_Test()
        {
            using RabbitMQMappedServer server = new(logger, () =>
            {
                RabbitMQConfigurationBuilder configBuilder = new();
                configBuilder.AddConnectionFactory(new()
                {
                    HostName = "Kuznetsovy-Server",
                    UserName = "ebcey1",
                    Password = "123"
                });
                configBuilder.AddQueueConfiguration(new("TestQueue"));
                return new RabbitMQServer(logger, configBuilder.Build());
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            using RabbitMQMappedServer mapedServer = new(logger, server);
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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            using RabbitMQMappedServer mapedServer = new(logger, server);

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
            configBuilder.AddQueueConfiguration(new("TestQueue"));
            RabbitMQServer server = new(logger, configBuilder.Build());
            RabbitMQMappedServer mapedServer = new(logger, server);

            await mapedServer.StartAsync(CancellationToken.None);
            await mapedServer.StopAsync(CancellationToken.None);
        }
    }
}