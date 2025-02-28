using System.Text;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Configuration
{
    /// <summary>
    /// A <see cref="RabbitMQConfigurationBuilder"/> class.
    /// </summary>
    public sealed class RabbitMQConfigurationBuilder
    {
        private ConnectionFactory? factory;
        private QueueConfiguration? queueConfiguration;
        private ExchangeConfiguration? exchangeConfiguration;
        private CallbackRabbitMQConfiguration? callbackConfig;
        private CreateChannelOptions? createChannelOptions;
        private QoSConfiguration? qoSConfiguration;
        private Encoding? encoding;
        private RabbitMQOnStartConfigs? onStartConfigs;
        /// <summary>
        /// Adds a <see cref="ConnectionFactory"/> to builder instance.
        /// </summary>
        /// <param name="factory">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddConnectionFactory(ConnectionFactory factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            this.factory = factory;
            return this;
        }
        /// <summary>
        /// Adds a <see cref="QueueConfiguration"/> to builder instance.
        /// </summary>
        /// <param name="queueConfiguration">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddQueueConfiguration(QueueConfiguration queueConfiguration)
        {
            ArgumentNullException.ThrowIfNull(queueConfiguration);

            this.queueConfiguration = queueConfiguration;
            return this;
        }
        /// <summary>
        /// Adds a <see cref="ExchangeConfiguration"/> to builder instance. [optional]
        /// </summary>
        /// <param name="exchangeConfiguration">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddExchangeConfiguration(ExchangeConfiguration exchangeConfiguration)
        {
            ArgumentNullException.ThrowIfNull(exchangeConfiguration);

            this.exchangeConfiguration = exchangeConfiguration;
            return this;
        }
        /// <summary>
        /// Adds a <see cref="CallbackRabbitMQConfiguration"/> to builder instance. [optional]
        /// </summary>
        /// <param name="callbackRabbitMQConfiguration">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddCallbackConfiguration(CallbackRabbitMQConfiguration callbackRabbitMQConfiguration)
        {
            ArgumentNullException.ThrowIfNull(callbackRabbitMQConfiguration);
            this.callbackConfig = callbackRabbitMQConfiguration;
            return this;
        }
        /// <summary>
        /// Adds a <see cref="CreateChannelOptions"/> to builder instance [optional]
        /// </summary>
        /// <param name="createChannelOptions">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddCreateChannelOptions(CreateChannelOptions createChannelOptions)
        {
            ArgumentNullException.ThrowIfNull(createChannelOptions);
            this.createChannelOptions = createChannelOptions;
            return this;
        }
        /// <summary>
        /// Adds a <see cref="QoSConfiguration"/> to builder instance. [optional]
        /// </summary>
        /// <param name="qoSConfiguration">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddQoSConfiguration(QoSConfiguration qoSConfiguration)
        {
            ArgumentNullException.ThrowIfNull(qoSConfiguration);
            this.qoSConfiguration = qoSConfiguration;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="Encoding"/> to builder instance. [optional default is <see cref="Encoding.UTF8"/>]
        /// </summary>
        /// <param name="encoding">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddEncoding(Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(encoding);
            this.encoding = encoding;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="RabbitMQOnStartConfigs"/> to builder instance. [optional]
        /// </summary>
        /// <param name="onStartConfigs">The connection factory.</param>
        /// <returns>Current instance of <see cref="RabbitMQConfigurationBuilder"/>.</returns>
        public RabbitMQConfigurationBuilder AddOnStartConfiguration(RabbitMQOnStartConfigs onStartConfigs)
        {
            ArgumentNullException.ThrowIfNull(onStartConfigs);
            this.onStartConfigs = onStartConfigs;
            return this;
        }
        /// <summary>
        /// Builds a new instance of <see cref="RabbitMQConfiguration"/>.<br/>
        /// <see cref="ConnectionFactory"/> is mandatory.<br/>
        /// <see cref="QueueConfiguration"/> is mandatory.
        /// </summary>
        /// <returns>A new instance of the <see cref="RabbitMQConfiguration"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConfiguration Build()
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (queueConfiguration is null)
            {
                throw new ArgumentNullException(nameof(queueConfiguration));
            }
            return new(
                factory, 
                queueConfiguration, 
                exchangeConfiguration, 
                callbackConfig, 
                createChannelOptions, 
                qoSConfiguration,
                encoding,
                onStartConfigs);
        }

    }
}