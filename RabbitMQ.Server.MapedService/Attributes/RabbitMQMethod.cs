namespace EBCEYS.RabbitMQ.Server.MappedService.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RabbitMQMethod : Attribute
    {
        public string Name { get; }
        public RabbitMQMethod(string name) { Name = name; }
    }
}
