using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    /// <summary>
    /// A <see cref="BaseRabbitMQRequest"/>.
    /// </summary>
    public sealed class BaseRabbitMQRequest
    {
        /// <summary>
        /// The request data.
        /// </summary>
        public RabbitMQRequestData RequestData { get; }
        /// <summary>
        /// Initiates a new instance of the <see cref="BaseRabbitMQRequest"/>.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <param name="encoding">The encoding.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public BaseRabbitMQRequest(BasicDeliverEventArgs eventArgs, JsonSerializerSettings? serializerOptions = null, Encoding? encoding = default)
        {
            ArgumentNullException.ThrowIfNull(eventArgs);

            byte[] request = eventArgs.Body.ToArray();
            RabbitMQRequestData? data = JsonConvert.DeserializeObject<RabbitMQRequestData?>((encoding ?? Encoding.UTF8).GetString(request), serializerOptions);
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            RequestData = data;
        }
    }
}
