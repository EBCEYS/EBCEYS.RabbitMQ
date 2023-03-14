using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    public class QueueConfiguration
    {
        [Required]
        public string? QueueName { get; } = string.Empty;
        public bool Durable { get; } = false;
        public bool Exclusive { get; } = false;
        public bool AutoDelete { get; } = false;
        public IDictionary<string, object>? Arguments { get; } = null;
        public QueueConfiguration(string queueName, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException($"\"{nameof(queueName)}\" не может быть пустым или содержать только пробел.", nameof(queueName));
            }

            QueueName = queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            Arguments = arguments;
        }
        public QueueConfiguration()
        {
            
        }
    }
}