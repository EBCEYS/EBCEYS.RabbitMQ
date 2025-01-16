using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    internal static class ConnectionFactoryExtensions
    {
        internal static IConnection CreateConnection(this ConnectionFactory factory, CancellationToken token = default)
        {
            Task<IConnection> connection = factory.CreateConnectionAsync(token);
            connection.Wait(token);
            return connection.Result;
        }
    }
}
