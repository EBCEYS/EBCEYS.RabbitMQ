namespace EBCEYS.RabbitMQ.Server.MappedService.Exceptions
{
    internal class RabbitMQControllerException : Exception
    {
        public RabbitMQControllerException(string message): base(message) { }
    }
}
