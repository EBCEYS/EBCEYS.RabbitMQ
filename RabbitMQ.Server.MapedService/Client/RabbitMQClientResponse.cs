namespace EBCEYS.RabbitMQ.Client
{
    class RabbitMQClientResponse
    {
        public object? Response { get; set; }
        public ManualResetEvent? Event { get; set; }
    }
}
