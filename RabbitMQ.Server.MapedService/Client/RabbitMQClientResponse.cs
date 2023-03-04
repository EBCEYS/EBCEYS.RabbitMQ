namespace EBCEYS.RabbitMQ.Client
{
    class RabbitMQClientResponse
    {
        public string? Response { get; set; }
        public ManualResetEvent? Event { get; set; }
    }
}
