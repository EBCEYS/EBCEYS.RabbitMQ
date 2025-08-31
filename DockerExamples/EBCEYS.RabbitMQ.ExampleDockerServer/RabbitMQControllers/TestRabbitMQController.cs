using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using Newtonsoft.Json.Linq;

namespace EBCEYS.RabbitMQ.ExampleDockerServer.RabbitMQControllers;

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
        var result = a + b;
        logger.LogInformation("Get request from rabbitmq! a + b = {result}", result);
        return Task.FromResult(result);
    }

    [RabbitMQMethod("TestMethodRequestJToken")]
    public Task<JToken> TestMethodRequestJToken(JToken token)
    {
        logger.LogInformation("Get request {request}", Request?.RequestData.Method);
        token["TestValue"] = 2;
        return Task.FromResult(token);
    }

    [RabbitMQMethod("TestMethodException")]
    public Task<object> TestMethodException(string message)
    {
        logger.LogInformation("Get request {request}", Request?.RequestData.Method);
        throw new RabbitMQRequestProcessingException(message);
    }

    [RabbitMQMethod("TestMethodWithInnerException")]
    public Task<object> TestMethodWithInnerException(string message)
    {
        logger.LogInformation("Get request {request}", Request?.RequestData.Method);
        try
        {
            throw new InvalidOperationException("Some exception");
        }
        catch (Exception ex)
        {
            throw new RabbitMQRequestProcessingException(message, ex);
        }
    }

    [RabbitMQMethod("TestMethodWithUnexpectedException")]
    public Task<object> TestMethodWithUnexpectedException(string message)
    {
        logger.LogInformation("Get request {request}", Request?.RequestData.Method);
        throw new Exception(message);
    }
}