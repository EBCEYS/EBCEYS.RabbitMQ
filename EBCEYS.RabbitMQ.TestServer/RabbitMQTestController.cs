using System.Collections.Concurrent;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Middlewares;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.TestServer;

public class RabbitMQTestController(ConcurrentDictionary<string, TaskCompletionSource<string>> tasks)
    : RabbitMQSmartControllerBase
{
    protected override void SetMiddlewares(RabbitMqControllerMiddlewaresCollection middlewares)
    {
        middlewares.Add<WasAnyMessageReceivedMiddleware>();
    }

    [RabbitMQMethod("TestMethod1")]
    public Task TestMethod1(string a, string b)
    {
        var result = a + b;

        tasks.TryGetValue(result, out var val);
        val?.SetResult(result);

        return Task.CompletedTask;
    }

    [RabbitMQMethod("ExampleMethod")]
    public Task<string> TestMethod2(string a, string b)
    {
        return Task.FromResult(a + b);
    }

    [RabbitMQMethod("ServiceMethod")]
    public Task<string> ServiceMethod(string msg, [RabbitMqFromService] IMyCustomService service,
        CancellationToken token)
    {
        return service.ExecuteAsync(msg, token);
    }

    [RabbitMQMethod("KeyedServiceMethod")]
    public Task<string> KeyedServiceMethod(string msg,
        [RabbitMqFromKeyedService(typeof(Program))] IMyCustomKeyedService service, CancellationToken token)
    {
        return service.ExecuteAsync(msg, token);
    }
}

public class WasAnyMessageReceivedMiddleware : IRabbitMqSmartControllerMiddleware
{
    public static bool WasAnyMessageReceived { get; private set; }

    public Task InvokeAsync(BasicDeliverEventArgs arguments, CancellationToken cancellationToken)
    {
        WasAnyMessageReceived = true;
        return Task.CompletedTask;
    }
}