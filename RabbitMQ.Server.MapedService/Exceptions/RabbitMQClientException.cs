namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions
{
    public class RabbitMQClientException : Exception
    {
        /// <summary>
        /// Base rabbitmq exception call.
        /// </summary>
        public RabbitMQClientException() : base() { }
        /// <summary>
        /// Rabbitmq exception call with message.
        /// </summary>
        /// <param name="message">The message.</param>
        public RabbitMQClientException(string message) : base(message) { }
    }
}
