using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Configuration
{
    public sealed class RabbitMQConfigurationBuilder
    {
        private ConnectionFactory? factory;
        private QueueConfiguration? queueConfiguration;
        private ExchangeConfiguration? exchangeConfiguration;
        public RabbitMQConfigurationBuilder AddConnectionFactory(ConnectionFactory factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            this.factory = factory;
            return this;
        }
        public RabbitMQConfigurationBuilder AddQueueConfiguration(QueueConfiguration queueConfiguration)
        {
            if (queueConfiguration is null)
            {
                throw new ArgumentNullException(nameof(queueConfiguration));
            }

            this.queueConfiguration = queueConfiguration;
            return this;
        }
        public RabbitMQConfigurationBuilder AddExchangeConfiguration(ExchangeConfiguration exchangeConfiguration)
        {
            if (exchangeConfiguration is null)
            {
                throw new ArgumentNullException(nameof(exchangeConfiguration));
            }

            this.exchangeConfiguration = exchangeConfiguration;
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
            return new(factory, queueConfiguration, exchangeConfiguration ?? null);
        }

    }
}