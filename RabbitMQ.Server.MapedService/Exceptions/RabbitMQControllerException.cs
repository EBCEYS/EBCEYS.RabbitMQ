namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions;

/// <summary>
///     Initiates a new instance of the <see cref="RabbitMQControllerException" />.
/// </summary>
/// <param name="message">The message.</param>
public class RabbitMQControllerException(string message) : Exception(message)
{
}