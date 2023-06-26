using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;

namespace SmartController.Tests
{
    [TestClass]
    public class RabbitMQSmartControllerTests
    {
        private TestSmartController? testController;
        [TestInitialize]
        public void Initialize()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "Kuznetsovy-Server",
                UserName = "ebcey1",
                Password = "123"
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            testController = RabbitMQSmartControllerBase.InitializeNewController<TestSmartController>(configBuilder.Build(), null);
            testController.StartAsync(CancellationToken.None).Wait();
        }
        [TestMethod]
        public async Task TestMethod1()
        {
            var res = await testController!.TestMethod1("a", "b");
            Assert.IsNotNull(res);
        }
        [TestMethod]
        public async Task TestMethod2()
        {
            await testController!.TestMethod2("a", "b");
            Assert.IsNotNull(testController);
        }
    }

    internal class TestSmartController : RabbitMQSmartControllerBase
    {
        public TestSmartController()
        {

        }

        [RabbitMQMethod("TestMethod1")]
        public async Task<string> TestMethod1(string a, string b)
        {
            return await Task.FromResult(a + b);
        }

        [RabbitMQMethod("TestMethod2")]
        public async Task TestMethod2(string a, string b)
        {
            await Task.Delay(100);
        }
    }
}