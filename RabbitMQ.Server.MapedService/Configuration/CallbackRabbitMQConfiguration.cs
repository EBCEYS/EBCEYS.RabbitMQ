using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     A <see cref="CallbackRabbitMQConfiguration" /> class.
/// </summary>
public class CallbackRabbitMQConfiguration
{
    /// <summary>
    ///     Initiates a new instance of the <see cref="CallbackRabbitMQConfiguration" />.
    /// </summary>
    /// <param name="queueConfig">The queue configuration.</param>
    /// <param name="requestsTimeout">
    ///     The response awaiting timeout.<br /><see cref="TimeSpan" /> should be more than
    ///     <see cref="TimeSpan.Zero" />.
    /// </param>
    /// <param name="exchangeConfig">The exchange configuration.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public CallbackRabbitMQConfiguration(QueueConfiguration queueConfig, TimeSpan requestsTimeout,
        ExchangeConfiguration? exchangeConfig = null)
    {
        if (requestsTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(requestsTimeout), "Timeout shoud be more than zero timespan!");
        ExchangeConfiguration = exchangeConfig;
        RequestsTimeout = requestsTimeout;
        QueueConfiguration = queueConfig;
    }

    /// <summary>
    ///     The exchange configuration.
    /// </summary>
    public ExchangeConfiguration? ExchangeConfiguration { get; set; }

    /// <summary>
    ///     The queue configuration.
    /// </summary>
    [Required]
    [NotNull]
    public QueueConfiguration QueueConfiguration { get; set; }

    /// <summary>
    ///     The response awaiting timeout.
    /// </summary>
    [Required]
    [NotNull]
    public TimeSpan RequestsTimeout { get; set; }
}