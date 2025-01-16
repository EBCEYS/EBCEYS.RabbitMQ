using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
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
        public IDictionary<string, object?>? Arguments { get; } = null;
        public bool Passive { get; } = false;
        public bool NoWait { get; } = false;
        public ExchangeConfiguration(string exchangeName, string exchangeType, bool durable = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool passive = false, bool noWait = false)
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
            ExchangeType = ExchangeTypeExtensions.ParseFromEnum(exchangeType.GetExchangeType());
            Durable = durable;
            AutoDelete = autoDelete;
            Arguments = arguments;
            Passive = passive;
            NoWait = noWait;
        }
        public ExchangeConfiguration(string exchangeName, ExchangeTypes exchangeType, bool durable = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool passive = false, bool noWait = false)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new ArgumentException($"\"{nameof(exchangeName)}\" не может быть пустым или содержать только пробел.", nameof(exchangeName));
            }

            ExchangeName = exchangeName;
            ExchangeType = ExchangeTypeExtensions.ParseFromEnum(exchangeType);
            Durable = durable;
            AutoDelete = autoDelete;
            Arguments = arguments;
            Passive = passive;
            NoWait = noWait;
        }
        public ExchangeConfiguration()
        {
            
        }
    }
}