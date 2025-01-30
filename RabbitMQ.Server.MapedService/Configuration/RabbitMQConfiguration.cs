using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
        public QoSConfiguration QoSConfiguration { get; set; } = new(0, 1, false);
        public CallbackRabbitMQConfiguration? CallBackConfiguration { get; set; }
        public RabbitMQOnStartConfigs OnStartConfigs { get; set; } = new();
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// Initiates a new instance of the <see cref="RabbitMQConfiguration"/>.
        /// </summary>
        /// <param name="factory">The RabbitMQ connection factory.</param>
        /// <param name="queueConfiguration">The queue configuration.</param>
        /// <param name="exchangeConfiguration">The exchange configuration. [optional]</param>
        /// <param name="callBackConfig">The callback exchange and queue configurations. [optional for <see cref="Client.RabbitMQClient"/> only]</param>
        /// <param name="createChannelOptions">The create channel options. [optinal]</param>
        /// <param name="qoSConfiguration">The QoS configuration. [optional]</param>
        /// <param name="encoding">The encoding. Default is <see cref="Encoding.UTF8"/>. [optional]</param>
        /// <param name="onStartConfigs">The on start config. [optional]</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConfiguration(ConnectionFactory factory, QueueConfiguration queueConfiguration, ExchangeConfiguration? exchangeConfiguration = null, CallbackRabbitMQConfiguration? callBackConfig = null, CreateChannelOptions? createChannelOptions = null, QoSConfiguration? qoSConfiguration = null, Encoding? encoding = null, RabbitMQOnStartConfigs? onStartConfigs = null)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            ExchangeConfiguration = exchangeConfiguration;
            CreateChannelOptions = createChannelOptions;
            QoSConfiguration = qoSConfiguration ?? new(0, 1, false);
            CallBackConfiguration = callBackConfig;
            Encoding = encoding ?? Encoding.UTF8;
            OnStartConfigs = onStartConfigs ?? new();
        }
        public RabbitMQConfiguration()
        {
            
        }
    }
}