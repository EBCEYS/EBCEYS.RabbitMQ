using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    public sealed class BaseRabbitMQRequest
    {
        public RabbitMQRequestData RequestData { get; }
        public BaseRabbitMQRequest(BasicDeliverEventArgs eventArgs, JsonSerializerOptions? serializerOptions = null)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            serializerOptions ??= new()
                {
                    Converters = { new JsonStringEnumConverter() },
                    WriteIndented = false,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

            byte[] request = eventArgs.Body.ToArray();
            RabbitMQRequestData? data = JsonSerializer.Deserialize<RabbitMQRequestData>(request, serializerOptions);
            if (data is null)
            {
                throw new ArgumentException(nameof(data));
            }
            RequestData = data;
        }
    }
}
