using System.Text;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     A <see cref="RabbitMQConfigurationBuilder" /> class.
/// </summary>
public sealed class RabbitMQConfigurationBuilder
{
    private CallbackRabbitMQConfiguration? _callbackConfig;
    private CreateChannelOptions? _createChannelOptions;
    private Encoding? _encoding;
    private ExchangeConfiguration? _exchangeConfiguration;
    private ConnectionFactory? _factory;
    private RabbitMQOnStartConfigs? _onStartConfigs;
    private QoSConfiguration? _qoSConfiguration;
    private QueueConfiguration? _queueConfiguration;

    /// <summary>
    ///     Adds a <see cref="ConnectionFactory" /> to builder instance.
    /// </summary>
    /// <param name="factory">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddConnectionFactory(ConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        this._factory = factory;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="QueueConfiguration" /> to builder instance.
    /// </summary>
    /// <param name="queueConfiguration">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddQueueConfiguration(QueueConfiguration queueConfiguration)
    {
        ArgumentNullException.ThrowIfNull(queueConfiguration);

        this._queueConfiguration = queueConfiguration;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="ExchangeConfiguration" /> to builder instance. [optional]
    /// </summary>
    /// <param name="exchangeConfiguration">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddExchangeConfiguration(ExchangeConfiguration exchangeConfiguration)
    {
        ArgumentNullException.ThrowIfNull(exchangeConfiguration);

        this._exchangeConfiguration = exchangeConfiguration;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="CallbackRabbitMQConfiguration" /> to builder instance. [optional]
    /// </summary>
    /// <param name="callbackRabbitMQConfiguration">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddCallbackConfiguration(
        CallbackRabbitMQConfiguration callbackRabbitMQConfiguration)
    {
        ArgumentNullException.ThrowIfNull(callbackRabbitMQConfiguration);
        _callbackConfig = callbackRabbitMQConfiguration;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="CreateChannelOptions" /> to builder instance [optional]
    /// </summary>
    /// <param name="createChannelOptions">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddCreateChannelOptions(CreateChannelOptions createChannelOptions)
    {
        ArgumentNullException.ThrowIfNull(createChannelOptions);
        this._createChannelOptions = createChannelOptions;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="QoSConfiguration" /> to builder instance. [optional]
    /// </summary>
    /// <param name="qoSConfiguration">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddQoSConfiguration(QoSConfiguration qoSConfiguration)
    {
        ArgumentNullException.ThrowIfNull(qoSConfiguration);
        this._qoSConfiguration = qoSConfiguration;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="Encoding" /> to builder instance. [optional default is <see cref="Encoding.UTF8" />]
    /// </summary>
    /// <param name="encoding">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddEncoding(Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        this._encoding = encoding;
        return this;
    }

    /// <summary>
    ///     Adds a <see cref="RabbitMQOnStartConfigs" /> to builder instance. [optional]
    /// </summary>
    /// <param name="onStartConfigs">The connection factory.</param>
    /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder" />.</returns>
    public RabbitMQConfigurationBuilder AddOnStartConfiguration(RabbitMQOnStartConfigs onStartConfigs)
    {
        ArgumentNullException.ThrowIfNull(onStartConfigs);
        this._onStartConfigs = onStartConfigs;
        return this;
    }

    /// <summary>
    ///     Builds a new instance of <see cref="RabbitMQConfiguration" />.<br />
    ///     <see cref="ConnectionFactory" /> is mandatory.<br />
    ///     <see cref="QueueConfiguration" /> is mandatory.
    /// </summary>
    /// <returns>A new instance of the <see cref="RabbitMQConfiguration" />.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public RabbitMQConfiguration Build()
    {
        if (_factory is null) throw new ArgumentNullException(nameof(_factory));
        if (_queueConfiguration is null) throw new ArgumentNullException(nameof(_queueConfiguration));
        return new RabbitMQConfiguration(
            _factory,
            _queueConfiguration,
            _exchangeConfiguration,
            _callbackConfig,
            _createChannelOptions,
            _qoSConfiguration,
            _encoding,
            _onStartConfigs);
    }
}