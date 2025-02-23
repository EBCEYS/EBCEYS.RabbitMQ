using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;

namespace EBCEYS.RabbitMQ.Client
{
    class RabbitMQClientResponse
    {
        public string? Response { get; set; }
        public ManualResetEvent? Event { get; set; }
        public RabbitMQRequestProcessingException? RequestProcessingException { get; set; }
    }
}
