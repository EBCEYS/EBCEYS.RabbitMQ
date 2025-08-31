using System.ComponentModel.DataAnnotations;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     A <see cref="ExchangeConfiguration" /> class.
/// </summary>
public class ExchangeConfiguration
{
    /// <summary>
    ///     Initiates a new instance of the <see cref="ExchangeConfiguration" />.
    /// </summary>
    /// <param name="exchangeName">The exchange name.</param>
    /// <param name="exchangeType">The exchange type.</param>
    /// <param name="durable">The durable.</param>
    /// <param name="autoDelete">The autodelete.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="passive">The passive.</param>
    /// <param name="noWait">The nowait.</param>
    /// <exception cref="ArgumentException"></exception>
    [Obsolete("Constructor will be removed in future versions.")]
    public ExchangeConfiguration(string exchangeName, string exchangeType, bool durable = false,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool passive = false,
        bool noWait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(exchangeName));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(exchangeType));

        ExchangeName = exchangeName;
        ExchangeType = ExchangeTypeExtensions.ParseFromEnum(exchangeType.GetExchangeType());
        Durable = durable;
        AutoDelete = autoDelete;
        Arguments = arguments;
        Passive = passive;
        NoWait = noWait;
    }

    /// <summary>
    ///     Initiates a new instance of the <see cref="ExchangeConfiguration" />.
    /// </summary>
    /// <param name="exchangeName">The exchange name.</param>
    /// <param name="exchangeType">The exchange type.</param>
    /// <param name="durable">The durable.</param>
    /// <param name="autoDelete">The autodelete.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="passive">The passive.</param>
    /// <param name="noWait">The nowait.</param>
    /// <exception cref="ArgumentException"></exception>
    public ExchangeConfiguration(string exchangeName, ExchangeTypes exchangeType, bool durable = false,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null, bool passive = false,
        bool noWait = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(exchangeType));

        ExchangeName = exchangeName;
        ExchangeType = ExchangeTypeExtensions.ParseFromEnum(exchangeType);
        Durable = durable;
        AutoDelete = autoDelete;
        Arguments = arguments;
        Passive = passive;
        NoWait = noWait;
    }

    /// <summary>
    ///     Initiates a new instance of the <see cref="ExchangeConfiguration" />.
    /// </summary>
    public ExchangeConfiguration()
    {
    }

    /// <summary>
    ///     The exchange name.
    /// </summary>
    [Required]
    public string? ExchangeName { get; } = string.Empty;

    /// <summary>
    ///     The <see cref="string" /> representation of exchange type.
    /// </summary>
    [Required]
    public string? ExchangeType { get; } = string.Empty;

    /// <summary>
    ///     The durable.
    /// </summary>
    public bool Durable { get; }

    /// <summary>
    ///     The autodelete.
    /// </summary>
    public bool AutoDelete { get; }

    /// <summary>
    ///     The arguments.
    /// </summary>
    public IDictionary<string, object?>? Arguments { get; }

    /// <summary>
    ///     The passive.
    /// </summary>
    public bool Passive { get; }

    /// <summary>
    ///     The nowait.
    /// </summary>
    public bool NoWait { get; }
}