using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;

namespace EBCEYS.RabbitMQ.Client;

internal class RabbitMQClientResponse
{
    public string? Response { get; set; }
    public RabbitMQRequestProcessingException? RequestProcessingException { get; set; }
}