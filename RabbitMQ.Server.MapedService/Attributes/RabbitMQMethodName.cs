namespace EBCEYS.RabbitMQ.Server.MappedService.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RabbitMQMethodName : Attribute
    {
        public string Name { get; }
        public RabbitMQMethodName(string name) { Name = name; }
    }
}
