using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;

namespace EBCEYS.RabbitMQ.ExampleService.Controllers
{
    internal class ExampleController : RabbitMQControllerBase
    {
        private readonly ILogger<ExampleController> logger;

        public ExampleController(ILogger<ExampleController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.logger.LogInformation("Create {name}", typeof(ExampleController).Name);
        }
        [RabbitMQMethod("ExampleMethod")]
        public async Task<string> ExampleMethod(string a, string b)
        {
            logger.LogInformation("Get request with params {a}  and {b}! Result is: {result}", a, b, a + b);
            return await Task.FromResult(a + b);
        }
    }
}
