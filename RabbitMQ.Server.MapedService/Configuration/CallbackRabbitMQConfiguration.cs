using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    public class CallbackRabbitMQConfiguration(QueueConfiguration queueConfig, ExchangeConfiguration? exchangeConfig = null)
    {
        public ExchangeConfiguration? ExchangeConfiguration { get; set; } = exchangeConfig;
        [Required]
        public QueueConfiguration QueueConfiguration { get; set; } = queueConfig;
    }
}