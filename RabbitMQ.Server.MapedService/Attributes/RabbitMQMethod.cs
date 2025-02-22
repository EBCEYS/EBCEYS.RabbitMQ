namespace EBCEYS.RabbitMQ.Server.MappedService.Attributes
{
    /// <summary>
    /// A <see cref="RabbitMQMethod"/> class.
    /// </summary>
    /// <remarks>
    /// Initiates a new instance of the <see cref="RabbitMQMethod"/>
    /// </remarks>
    /// <param name="name">The rabbitmq method name.</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RabbitMQMethod(string name) : Attribute
    {
        /// <summary>
        /// The rabbitmq method name.
        /// </summary>
        public string Name { get; } = name;
    }
}
