﻿using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Configuration
{
    public sealed class RabbitMQConfigurationBuilder
    {
        private ConnectionFactory? factory;
        private QueueConfiguration? queueConfiguration;
        private ExchangeConfiguration? exchangeConfiguration;
        private CallbackRabbitMQConfiguration? callbackConfig;
        private CreateChannelOptions? createChannelOptions;
        public RabbitMQConfigurationBuilder AddConnectionFactory(ConnectionFactory factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            this.factory = factory;
            return this;
        }
        public RabbitMQConfigurationBuilder AddQueueConfiguration(QueueConfiguration queueConfiguration)
        {
            ArgumentNullException.ThrowIfNull(queueConfiguration);

            this.queueConfiguration = queueConfiguration;
            return this;
        }
        public RabbitMQConfigurationBuilder AddExchangeConfiguration(ExchangeConfiguration exchangeConfiguration)
        {
            ArgumentNullException.ThrowIfNull(exchangeConfiguration);

            this.exchangeConfiguration = exchangeConfiguration;
            return this;
        }
        public RabbitMQConfigurationBuilder AddCallbackConfiguration(CallbackRabbitMQConfiguration callbackRabbitMQConfiguration)
        {
            ArgumentNullException.ThrowIfNull(callbackRabbitMQConfiguration);
            this.callbackConfig = callbackRabbitMQConfiguration;
            return this;
        }
        public RabbitMQConfigurationBuilder AddCreateChannelOptions(CreateChannelOptions createChannelOptions)
        {
            ArgumentNullException.ThrowIfNull(createChannelOptions);
            this.createChannelOptions = createChannelOptions;
            return this;
        }
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
            return new(factory, queueConfiguration, exchangeConfiguration ?? null, callbackConfig ?? null, createChannelOptions ?? null);
        }

    }
}