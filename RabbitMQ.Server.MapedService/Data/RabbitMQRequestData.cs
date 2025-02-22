namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    /// <summary>
    /// A <see cref="RabbitMQRequestData"/> class.
    /// </summary>
    public class RabbitMQRequestData
    {
        /// <summary>
        /// The params.
        /// </summary>
        public object[]? Params { get; set; }
        /// <summary>
        /// The method to execute name.
        /// </summary>
        public string? Method { get; set; }
    }
}
