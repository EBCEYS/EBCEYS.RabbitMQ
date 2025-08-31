using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions;

internal static class ConnectionFactoryExtensions
{
    /// <summary>
    ///     Creates a connection to RabbitMQ instance with specific settings <see cref="RabbitMQOnStartConfigs" />.
    /// </summary>
    /// <param name="factory">The connection factory.</param>
    /// <param name="conf">The on start configuration.</param>
    /// <param name="clientName">The client name.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="BrokerUnreachableException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns>The connection instance.</returns>
    internal static async Task<IConnection> CreateConnectionAsync(this ConnectionFactory factory,
        RabbitMQOnStartConfigs conf, string? clientName = null, CancellationToken token = default)
    {
        BrokerUnreachableException? connectionEx = null;
        for (byte i = 0; i < conf.ConnectionReties; i++)
        {
            try
            {
                return await factory.CreateConnectionAsync(clientName, token);
            }
            catch (BrokerUnreachableException ex)
            {
                connectionEx = ex;
            }

            await Task.Delay(conf.DelayBeforeRetries, token);
        }

        throw connectionEx ?? new Exception("Error on connection to RabbitMQ instance!");
    }
}