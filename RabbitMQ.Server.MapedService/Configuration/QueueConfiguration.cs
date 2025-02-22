using System.ComponentModel.DataAnnotations;

namespace EBCEYS.RabbitMQ.Configuration
{
    /// <summary>
    /// A <see cref="QueueConfiguration"/> class.
    /// </summary>
    public class QueueConfiguration
    {
        /// <summary>
        /// The queue name.
        /// </summary>
        [Required]
        public string QueueName { get; } = string.Empty;
        private readonly string? routingKey;
        /// <summary>
        /// The routing key.<br/>
        /// Returns <see cref="QueueName"/> if routing key was set as <c>null</c>.
        /// </summary>
        public string RoutingKey 
        { 
            get
            {
                return routingKey ?? QueueName;
            }
        }
        /// <summary>
        /// The durable.
        /// </summary>
        public bool Durable { get; } = false;
        /// <summary>
        /// The exclusive.
        /// </summary>
        public bool Exclusive { get; } = false;
        /// <summary>
        /// The autodelete.
        /// </summary>
        public bool AutoDelete { get; } = false;
        /// <summary>
        /// The arguments.
        /// </summary>
        public IDictionary<string, object?>? Arguments { get; } = null;
        /// <summary>
        /// The nowait.
        /// </summary>
        public bool NoWait { get; } = false;
        /// <summary>
        /// Initiates a new instance of the <see cref="QueueConfiguration"/>.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="routingKey">The routing key. [optional].<br/>
        /// <see cref="RoutingKey"/> will return <see cref="QueueName"/> if set <c>null</c>.</param>
        /// <param name="durable">The durable.</param>
        /// <param name="exclusive">The exclusive.</param>
        /// <param name="autoDelete">The autodelete.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="noWait">The nowait.</param>
        /// <exception cref="ArgumentException"></exception>
        public QueueConfiguration(string queueName, string? routingKey = null, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool noWait = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));

            QueueName = queueName;
            this.routingKey = routingKey ?? queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            Arguments = arguments;
            NoWait = noWait;
        }
        /// <summary>
        /// Initiates a new instance of the <see cref="QueueConfiguration"/>.
        /// </summary>
        public QueueConfiguration()
        {
            
        }
    }
}