using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    public static class ChannelExtensions
    {
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
        public static Task BasicQosAsync(this IChannel channel, QoSConfiguration? qoSConfiguration, CancellationToken token = default) 
        {
            return channel.BasicQosAsync(qoSConfiguration?.PrefetchSize ?? 0, qoSConfiguration?.PrefetchCount ?? 1, qoSConfiguration?.Global ?? false, token);
        }
    }
}
