using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;

namespace RabbitMQ.Configuration
{
    public class RabbitMQConfiguration
    {
        [Required]
        public ConnectionFactory Factory { get; set; }
        [Required]
        public QueueConfiguration QueueConfiguration { get; set; }
        public ExchangeConfiguration? ExchangeConfiguration { get; set; }
        public RabbitMQConfiguration(ConnectionFactory factory, QueueConfiguration queueConfiguration, ExchangeConfiguration? exchangeConfiguration = null)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            ExchangeConfiguration = exchangeConfiguration;
        }
    }
}