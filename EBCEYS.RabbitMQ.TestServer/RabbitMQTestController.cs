using System.Collections.Concurrent;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;

namespace EBCEYS.RabbitMQ.TestServer;

public class RabbitMQTestController(ConcurrentDictionary<string, TaskCompletionSource<string>> tasks) : RabbitMQSmartControllerBase
{
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
}