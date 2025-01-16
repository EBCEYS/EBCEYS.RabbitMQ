using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    public class RabbitMQConfiguration
    {
        [Required]
        public ConnectionFactory Factory { get; set; } = null!;
        [Required]
        public QueueConfiguration QueueConfiguration { get; set; } = null!;
        public ExchangeConfiguration? ExchangeConfiguration { get; set; }
        public CreateChannelOptions? CreateChannelOptions { get; set; }
        public CallbackRabbitMQConfiguration? CallBackConfiguration { get; set; }
        /// <summary>
        /// Initiates a new instance of the <see cref="RabbitMQConfiguration"/>.
        /// </summary>
        /// <param name="factory">The RabbitMQ connection factory.</param>
        /// <param name="queueConfiguration">The queue configuration.</param>
        /// <param name="exchangeConfiguration">The exchange configuration. [optional]</param>
        /// <param name="callBackConfig">The callback exchange and queue configurations. [optional for <see cref="Client.RabbitMQClient"/> only]</param>
        /// <param name="createChannelOptions">The create channel options. [optinal]</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConfiguration(ConnectionFactory factory, QueueConfiguration queueConfiguration, ExchangeConfiguration? exchangeConfiguration = null, CallbackRabbitMQConfiguration? callBackConfig = null, CreateChannelOptions? createChannelOptions = null)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            ExchangeConfiguration = exchangeConfiguration;
            CreateChannelOptions = createChannelOptions;
            CallBackConfiguration = callBackConfig;
        }
        public RabbitMQConfiguration()
        {
            
        }
    }

    public class CallbackRabbitMQConfiguration(QueueConfiguration queueConfig, ExchangeConfiguration? exchangeConfig = null)
    {
        public ExchangeConfiguration? ExchangeConfiguration { get; set; } = exchangeConfig;
        [Required]
        public QueueConfiguration QueueConfiguration { get; set; } = queueConfig;
    }
}