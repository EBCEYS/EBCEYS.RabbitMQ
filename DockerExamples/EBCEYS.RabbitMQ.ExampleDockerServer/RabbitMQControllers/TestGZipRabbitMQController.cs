using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;

namespace EBCEYS.RabbitMQ.ExampleDockerServer.RabbitMQControllers;

internal class TestGZipRabbitMQController(ILogger<TestGZipRabbitMQController> logger) : RabbitMQSmartControllerBase
{
    [RabbitMQMethod("TestMessageGZiped")]
    public Task TestMessageGZiped(string msg)
    {
        logger.LogInformation("Method {method} received msg {msg}", Request!.RequestData.Method, msg);
        return Task.CompletedTask;
    }

    [RabbitMQMethod("TestRequestGZiped")]
    public Task<string> TestRequestGZiped(string msg)
    {
        logger.LogInformation("Method {method} received msg {msg}", Request!.RequestData.Method, msg);
        return Task.FromResult($"{msg}" + " received!");
    }
}