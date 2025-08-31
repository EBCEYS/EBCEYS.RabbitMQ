using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;

namespace EBCEYS.RabbitMQ.ExampleSmartController.Controllers;

internal class TestController : RabbitMQSmartControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RabbitMQMethod("TestMethod1")]
    public Task TestMethod1(string a, string b)
    {
        _logger.LogInformation("TestMethod1 get command with args: a: {a}\tb: {b}", a, b);
        return Task.CompletedTask;
    }

    [RabbitMQMethod("ExampleMethod")]
    public async Task<string> TestMethod2(string a, string b)
    {
        _logger.LogInformation("TestMethod2 get command with args: a: {a}\tb: {b}", a, b);
        return await Task.FromResult(a + b);
    }
}