namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions;

/// <summary>
///     A <see cref="RabbitMQClientException" /> class.
/// </summary>
public class RabbitMQClientException : Exception
{
    /// <summary>
    ///     Base rabbitmq exception call.
    /// </summary>
    public RabbitMQClientException()
    {
    }

    /// <summary>
    ///     Rabbitmq exception call with message.
    /// </summary>
    /// <param name="message">The message.</param>
    public RabbitMQClientException(string message) : base(message)
    {
    }
}