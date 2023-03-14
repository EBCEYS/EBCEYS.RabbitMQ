using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    public sealed class BaseRabbitMQRequest
    {
        public RabbitMQRequestData RequestData { get; }
        public BaseRabbitMQRequest(BasicDeliverEventArgs eventArgs, JsonSerializerSettings? serializerOptions = null)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            byte[] request = eventArgs.Body.ToArray();
            RabbitMQRequestData? data = JsonConvert.DeserializeObject<RabbitMQRequestData?>(Encoding.UTF8.GetString(request), serializerOptions);
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            RequestData = data;
        }
    }
}
