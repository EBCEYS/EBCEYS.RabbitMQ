namespace RabbitMQ.Server.MapedService.Exceptions
{
    internal class RabbitMQControllerException : Exception
    {
        public RabbitMQControllerException(string message): base(message) { }
    }
}
