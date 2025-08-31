namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     Initiates a new instance of the <see cref="QoSConfiguration" />.
/// </summary>
/// <param name="prefetchSize">The prefetch size.</param>
/// <param name="prefetchCount">The prefetch count.</param>
/// <param name="global">The global.</param>
public class QoSConfiguration(uint prefetchSize, ushort prefetchCount, bool global)
{
    /// <summary>
    ///     The prefetch size.
    /// </summary>
    public uint PrefetchSize { get; set; } = prefetchSize;

    /// <summary>
    ///     The prefetch count.
    /// </summary>
    public ushort PrefetchCount { get; set; } = prefetchCount;

    /// <summary>
    ///     The global.
    /// </summary>
    public bool Global { get; set; } = global;
}