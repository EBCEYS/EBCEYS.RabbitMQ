using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions
{
    /// <summary>
    /// The rabbit mq request processing exception. Based on <see cref="Exception"/>. <br/>
    /// Use it to send your exception as response.
    /// <param name="message">The custom message.</param>
    /// <param name="innerException">The inner exception [optional]</param>
    /// </summary>
    public class RabbitMQRequestProcessingException : Exception
    {
        public string? InnerProcessingException { get; private set; }

        public RabbitMQRequestProcessingException(string message, Exception? innerException = null) : base(message, innerException)
        {
            InnerProcessingException = base.ToString();
        }

        internal RabbitMQRequestProcessingException(string message, string? innerException = null) : base(message)
        {
            InnerProcessingException = StringExtensions.ConcatStrings(message, Environment.NewLine, innerException ?? string.Empty);
        }

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
        /// Creates the <see cref="RabbitMQRequestProcessingExceptionDTO"/> from <see cref="RabbitMQRequestProcessingException"/>.
        /// </summary>
        /// <returns>New <see cref="RabbitMQRequestProcessingExceptionDTO"/>.</returns>
        public RabbitMQRequestProcessingExceptionDTO GetDTO()
        {
            return new(Message, InnerProcessingException);
        }
        /// <summary>
        /// Creates the <see cref="RabbitMQRequestProcessingException"/> from <see cref="RabbitMQRequestProcessingExceptionDTO"/>.
        /// </summary>
        /// <param name="dto">The DTO object.</param>
        /// <returns>New <see cref="RabbitMQRequestProcessingException"/> if DTO is not null; otherwise null.</returns>
        public static RabbitMQRequestProcessingException? CreateFromDTO(RabbitMQRequestProcessingExceptionDTO? dto)
        {
            return dto != null ? new(dto.Message, dto.InnerException) : null;
        }
    }
    public class RabbitMQRequestProcessingExceptionDTO(string message, string? innerException = null)
    {
        public string Message { get; private set; } = message;
        public string? InnerException { get; private set; } = innerException;
    }
}
