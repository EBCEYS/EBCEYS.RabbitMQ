namespace EBCEYS.RabbitMQ.TestServer;

public class MyCustomService : IMyCustomService
{
    public static string Prefix => nameof(MyCustomService);

    public Task<string> ExecuteAsync(string message, CancellationToken token)
    {
        return Task.FromResult($"{Prefix}{message}");
    }
}

public interface IMyCustomService
{
    Task<string> ExecuteAsync(string message, CancellationToken token);
}

public class MyCustomKeyedService : IMyCustomKeyedService
{
    public static string Prefix => nameof(MyCustomKeyedService);

    public Task<string> ExecuteAsync(string message, CancellationToken token)
    {
        return Task.FromResult($"{Prefix}{message}");
    }
}

public interface IMyCustomKeyedService
{
    Task<string> ExecuteAsync(string message, CancellationToken token);
}