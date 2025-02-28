using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    /// <summary>
    /// A <see cref="BaseRabbitMQRequest"/> class.
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
        /// <param name="gZipSettings">The gzip settings.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <param name="encoding">The encoding.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseRabbitMQRequest(BasicDeliverEventArgs eventArgs, GZipSettings? gZipSettings = null, JsonSerializerSettings? serializerOptions = null, Encoding? encoding = default)
        {
            ArgumentNullException.ThrowIfNull(eventArgs);

            byte[] request = GZipSettings.GZipDecompress(eventArgs.Body.ToArray(), gZipSettings);
            string json = (encoding ?? Encoding.UTF8).GetString(request);
            RabbitMQRequestData? data = JsonConvert.DeserializeObject<RabbitMQRequestData?>(json, serializerOptions);
            ArgumentNullException.ThrowIfNull(data);
            RequestData = data;
        }
    }
}
