using System.ComponentModel.DataAnnotations;
using System.Text;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     A <see cref="RabbitMQConfiguration" /> class.
/// </summary>
public class RabbitMQConfiguration
{
    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQConfiguration" />.
    /// </summary>
    /// <param name="factory">The RabbitMQ connection factory.</param>
    /// <param name="queueConfiguration">The queue configuration.</param>
    /// <param name="exchangeConfiguration">The exchange configuration. [optional]</param>
    /// <param name="callBackConfig">
    ///     The callback exchange and queue configurations. [optional for
    ///     <see cref="Client.RabbitMQClient" /> only]
    /// </param>
    /// <param name="createChannelOptions">The create channel options. [optinal]</param>
    /// <param name="qoSConfiguration">The QoS configuration. [optional]</param>
    /// <param name="encoding">The encoding. Default is <see cref="System.Text.Encoding.UTF8" />. [optional]</param>
    /// <param name="onStartConfigs">The on start config. [optional]</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RabbitMQConfiguration(ConnectionFactory factory, QueueConfiguration queueConfiguration,
        ExchangeConfiguration? exchangeConfiguration = null, CallbackRabbitMQConfiguration? callBackConfig = null,
        CreateChannelOptions? createChannelOptions = null, QoSConfiguration? qoSConfiguration = null,
        Encoding? encoding = null, RabbitMQOnStartConfigs? onStartConfigs = null)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        QueueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
        ExchangeConfiguration = exchangeConfiguration;
        CreateChannelOptions = createChannelOptions;
        QoSConfiguration = qoSConfiguration ?? new QoSConfiguration(0, 1, false);
        CallBackConfiguration = callBackConfig;
        Encoding = encoding ?? Encoding.UTF8;
        OnStartConfigs = onStartConfigs ?? new RabbitMQOnStartConfigs();
    }

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQConfiguration" />.
    /// </summary>
    public RabbitMQConfiguration()
    {
    }

    /// <summary>
    ///     The connection factory.
    /// </summary>
    [Required]
    public ConnectionFactory Factory { get; set; } = null!;

    /// <summary>
    ///     The queue configuration.
    /// </summary>
    [Required]
    public QueueConfiguration QueueConfiguration { get; set; } = null!;

    /// <summary>
    ///     The exchange configuration [optional].
    /// </summary>
    public ExchangeConfiguration? ExchangeConfiguration { get; set; }

    /// <summary>
    ///     The channel creation option [optional].
    /// </summary>
    public CreateChannelOptions? CreateChannelOptions { get; set; }

    /// <summary>
    ///     The QoS configuration.
    /// </summary>
    public QoSConfiguration QoSConfiguration { get; set; } = new(0, 1, false);

    /// <summary>
    ///     The callback configuration [optional].
    /// </summary>
    public CallbackRabbitMQConfiguration? CallBackConfiguration { get; set; }

    /// <summary>
    ///     The on start configuration.
    /// </summary>
    public RabbitMQOnStartConfigs OnStartConfigs { get; set; } = new();

    /// <summary>
    ///     The encoding to post and receive messages.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}