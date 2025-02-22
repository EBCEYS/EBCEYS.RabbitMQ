using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    /// <summary>
    /// A <see cref="ChannelExtensions"/> class.
    /// </summary>
    public static class ChannelExtensions
    {
        /// <summary>
        /// Declares an exchange.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="configuration">The exchange configuration.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static Task ExchangeDeclareAsync(this IChannel channel, ExchangeConfiguration configuration, CancellationToken token = default)
        {
            return channel.ExchangeDeclareAsync(
                    configuration.ExchangeName!,
                    configuration.ExchangeType!,
                    configuration.Durable,
                    configuration.AutoDelete,
                    configuration.Arguments,
                    configuration.Passive,
                    configuration.NoWait,
                    token);
        }
        /// <summary>
        /// Declares a queue.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static Task<QueueDeclareOk> QueueDeclareAsync(this IChannel channel, QueueConfiguration? configuration, CancellationToken token = default)
        {
            return channel.QueueDeclareAsync(
                configuration?.QueueName ?? "",
                configuration?.Durable ?? false,
                configuration?.Exclusive ?? true,
                configuration?.AutoDelete ?? true,
                configuration?.Arguments,
                configuration?.NoWait ?? false,
                token);
        }
        /// <summary>
        /// Configures a basic QoS configuration to channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="qoSConfiguration">The QoS configuration.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static Task BasicQosAsync(this IChannel channel, QoSConfiguration? qoSConfiguration, CancellationToken token = default) 
        {
            return channel.BasicQosAsync(qoSConfiguration?.PrefetchSize ?? 0, qoSConfiguration?.PrefetchCount ?? 1, qoSConfiguration?.Global ?? false, token);
        }
    }
}
