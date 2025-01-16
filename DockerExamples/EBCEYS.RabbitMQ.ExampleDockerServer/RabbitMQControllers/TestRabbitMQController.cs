using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;

namespace EBCEYS.RabbitMQ.ExampleDockerServer.RabbitMQControllers
{
    internal class TestRabbitMQController(ILogger<TestRabbitMQController> logger) : RabbitMQSmartControllerBase
    {
        [RabbitMQMethod("TestMethodMessage")]
        public Task TestMethodMessage(long a, long b)
        {
            logger.LogInformation("Get message from rabbitmq! a + b = {result}", a + b);
            return Task.CompletedTask;
        }
        [RabbitMQMethod("TestMethodRequest")]
        public Task<long> TestMethodRequest(long a, long b)
        {
            long result = a + b;
            logger.LogInformation("Get request from rabbitmq! a + b = {result}", result);
            return Task.FromResult(result);
        }
    }
}
