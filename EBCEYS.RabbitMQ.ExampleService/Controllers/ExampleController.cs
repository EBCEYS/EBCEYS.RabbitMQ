using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;

namespace EBCEYS.RabbitMQ.ExampleService.Controllers;

internal class ExampleController : RabbitMQControllerBase
{
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ILogger<ExampleController> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._logger.LogInformation("Create {name}", typeof(ExampleController).Name);
    }

    [RabbitMQMethod("ExampleMethod")]
    public async Task<string> ExampleMethod(string a, string b)
    {
        _logger.LogInformation("Get request with params {a}  and {b}! Result is: {result}", a, b, a + b);
        return await Task.FromResult(a + b);
    }
}