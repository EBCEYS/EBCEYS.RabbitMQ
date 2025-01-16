using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    public class QueueConfiguration
    {
        [Required]
        public string QueueName { get; } = string.Empty;
        private readonly string? routingKey;
        public string RoutingKey 
        { 
            get
            {
                return routingKey ?? QueueName;
            }
        }
        public bool Durable { get; } = false;
        public bool Exclusive { get; } = false;
        public bool AutoDelete { get; } = false;
        public IDictionary<string, object?>? Arguments { get; } = null;
        public bool NoWait { get; } = false;
        public QueueConfiguration(string queueName, string? routingKey = null, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool noWait = false)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException($"\"{nameof(queueName)}\" не может быть пустым или содержать только пробел.", nameof(queueName));
            }

            QueueName = queueName;
            this.routingKey = routingKey ?? queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            Arguments = arguments;
            NoWait = noWait;
        }
        public QueueConfiguration()
        {
            
        }
    }
}