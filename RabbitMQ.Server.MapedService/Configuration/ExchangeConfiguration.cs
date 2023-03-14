using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    public class ExchangeConfiguration
    {
        [Required]
        public string? ExchangeName { get; } = string.Empty;
        [Required]
        public string? ExchangeType { get; } = string.Empty;
        public bool Durable { get; } = false;
        public bool AutoDelete { get; } = false;
        public IDictionary<string, object>? Arguments { get; } = null;
        public ExchangeConfiguration(string exchangeName, string exchangeType, bool durable = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new ArgumentException($"\"{nameof(exchangeName)}\" не может быть пустым или содержать только пробел.", nameof(exchangeName));
            }

            if (string.IsNullOrWhiteSpace(exchangeType))
            {
                throw new ArgumentException($"\"{nameof(exchangeType)}\" не может быть пустым или содержать только пробел.", nameof(exchangeType));
            }

            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            Durable = durable;
            AutoDelete = autoDelete;
            Arguments = arguments;
        }
        public ExchangeConfiguration()
        {
            
        }
    }
}