namespace RabbitMQ.Server.MapedService.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RabbitMQMethodName : Attribute
    {
        public string Name { get; }
        public RabbitMQMethodName(string name) { Name = name; }
    }
}
