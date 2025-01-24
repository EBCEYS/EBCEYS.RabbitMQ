namespace EBCEYS.RabbitMQ.Configuration
{
    public class QoSConfiguration(uint prefetchSize, ushort prefetchCount, bool global)
    {
        public uint PrefetchSize { get; set; } = prefetchSize;
        public ushort PrefetchCount { get; set; } = prefetchCount;
        public bool Global { get; set; } = global;
    }
}