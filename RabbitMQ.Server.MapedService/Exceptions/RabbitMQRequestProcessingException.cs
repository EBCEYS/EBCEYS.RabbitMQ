using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions;

/// <summary>
///     The rabbit mq request processing exception. Based on <see cref="Exception" />. <br />
///     Use it to send your exception as response.
/// </summary>
public class RabbitMQRequestProcessingException : Exception
{
    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQRequestProcessingException" />.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception</param>
    public RabbitMQRequestProcessingException(string message, Exception? innerException = null) : base(message,
        innerException)
    {
        InnerProcessingException = base.ToString();
    }

    internal RabbitMQRequestProcessingException(string message, string? innerException = null) : base(message)
    {
        InnerProcessingException =
            StringExtensions.ConcatStrings(message, Environment.NewLine, innerException ?? string.Empty);
    }

    /// <summary>
    ///     The inner exception exception string represintation.
    /// </summary>
    public string? InnerProcessingException { get; }

    /// <summary>
    ///     Gets <see cref="string" /> represintation of the <see cref="RabbitMQRequestProcessingException" /> instance.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return StringExtensions.ConcatStrings(
            GetType().Name,
            Environment.NewLine,
            Message,
            Environment.NewLine,
            InnerProcessingException ?? string.Empty);
    }

    /// <summary>
    ///     Creates the <see cref="RabbitMQRequestProcessingExceptionDto" /> from
    ///     <see cref="RabbitMQRequestProcessingException" />.
    /// </summary>
    /// <returns>New <see cref="RabbitMQRequestProcessingExceptionDto" />.</returns>
    public RabbitMQRequestProcessingExceptionDto GetDto()
    {
        return new RabbitMQRequestProcessingExceptionDto(Message, InnerProcessingException);
    }

    /// <summary>
    ///     Creates the <see cref="RabbitMQRequestProcessingException" /> from
    ///     <see cref="RabbitMQRequestProcessingExceptionDto" />.
    /// </summary>
    /// <param name="dto">The DTO object.</param>
    /// <returns>New <see cref="RabbitMQRequestProcessingException" /> if DTO is not null; otherwise null.</returns>
    public static RabbitMQRequestProcessingException? CreateFromDto(RabbitMQRequestProcessingExceptionDto? dto)
    {
        return dto != null ? new RabbitMQRequestProcessingException(dto.Message, dto.InnerException) : null;
    }
}

/// <summary>
///     Initiates a new instance of the <see cref="RabbitMQRequestProcessingExceptionDto" />.
/// </summary>
/// <param name="message">The message.</param>
/// <param name="innerException">The inner exception.</param>
public class RabbitMQRequestProcessingExceptionDto(string message, string? innerException = null)
{
    /// <summary>
    ///     The message.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    ///     The inner exception.
    /// </summary>
    public string? InnerException { get; } = innerException;
}