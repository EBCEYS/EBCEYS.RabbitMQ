using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EBCEYS.RabbitMQ.Server.Service
{
    public class RabbitMQServer : IHostedService, IAsyncDisposable, IDisposable
    {
        private readonly bool autoAck = true;

        private IConnection? connection;
        private IChannel? channel;
        private readonly ILogger<RabbitMQServer> logger;
        public AsyncEventingBasicConsumer? Consumer { get; private set; }
        private readonly RabbitMQConfiguration configuration;
        private AsyncEventHandler<BasicDeliverEventArgs>? consumerAction;

        public JsonSerializerSettings? SerializerOptions { get; private set; }

        public RabbitMQServer(ILogger<RabbitMQServer> logger, 
            RabbitMQConfiguration configuration, 
            AsyncEventHandler<BasicDeliverEventArgs>? consumerAction = null,
            JsonSerializerSettings? serializerOptions = null)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.consumerAction = consumerAction;
            this.SerializerOptions = serializerOptions;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.logger.LogDebug("Create rabbitMQ server service!");
        }

        public void SetConsumerAction(AsyncEventHandler<BasicDeliverEventArgs> consumerAction)
        {
            if (this.consumerAction is not null)
            {
                throw new InvalidOperationException("Consumer action is already set!");
            }
            ArgumentNullException.ThrowIfNull(consumerAction);
            this.consumerAction = consumerAction;
            logger.LogDebug("Set consumer action!");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (configuration.QueueConfiguration is null)
            {
                throw new Exception($"{nameof(configuration.QueueConfiguration)} is null!");
            }

            connection = await configuration.Factory!.CreateConnectionAsync(cancellationToken);
            channel = await connection.CreateChannelAsync(configuration.CreateChannelOptions, cancellationToken);
            Consumer = new(channel);

            await channel.QueueDeclareAsync(configuration.QueueConfiguration!, cancellationToken);
            if (configuration.ExchangeConfiguration is not null)
            {
                await channel.ExchangeDeclareAsync(configuration.ExchangeConfiguration, cancellationToken);
                await channel.QueueBindAsync(configuration.QueueConfiguration!.QueueName!, configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty, configuration.QueueConfiguration!.RoutingKey!, cancellationToken: cancellationToken);
            }

            await channel.BasicQosAsync(0, 1, false, cancellationToken);

            Consumer.ReceivedAsync += consumerAction;
            await channel.BasicConsumeAsync(configuration.QueueConfiguration.QueueName, autoAck, Consumer, cancellationToken: cancellationToken);

            logger.LogDebug("Consumer status on start: {status}", Consumer.IsRunning);
            logger.LogDebug("Start rabbitmq server!");
        }
        /// <summary>
        /// Acks the message. Use it in your consumer action.
        /// </summary>
        /// <param name="ea">The event args.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task AckMessage(BasicDeliverEventArgs ea, CancellationToken token = default)
        {
            if (!autoAck)
            {
                ArgumentNullException.ThrowIfNull(ea);

                logger.LogDebug("Ack message: {tag}", ea.DeliveryTag);
                await channel!.BasicAckAsync(ea.DeliveryTag, false, token);
            }
        }
        /// <summary>
        /// Sends the response. Uses with RPC configuration.
        /// </summary>
        /// <param name="ea">The event arguments.</param>
        /// <param name="response">The response json data.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendResponseAsync<T>(BasicDeliverEventArgs ea, T response)
        {
            ArgumentNullException.ThrowIfNull(ea);

            if (ea.BasicProperties.ReplyToAddress is null)
            {
                throw new InvalidOperationException("Event args do not contains ReplyTo params!");
            }
            try
            {
                BasicProperties replyProps = new()
                {
                    ContentType = "application/json",
                    ContentEncoding = "UTF-8",
                    CorrelationId = ea.BasicProperties.CorrelationId,
                };
                string json = response != null ? JsonConvert.SerializeObject(response, SerializerOptions) : "error_null";
                byte[] resp = Encoding.UTF8.GetBytes(json);

                logger.LogInformation("On request {id} response is {resp}", replyProps.CorrelationId, json);

                await channel!.BasicPublishAsync(ea.BasicProperties.ReplyToAddress.ExchangeName, ea.BasicProperties.ReplyToAddress.RoutingKey, false, replyProps, resp);
                await AckMessage(ea);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on responsing!");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (connection is null)
            {
                return;
            }
            try
            {
                await connection.CloseAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on stoping service!");
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (connection is not null)
                {
                    await connection.CloseAsync();
                    connection.Dispose();
                }
                channel?.Dispose();
            }
            catch { }
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            try
            {
                if (connection is not null)
                {
                    connection.CloseAsync().Wait();
                    connection.Dispose();
                }
                channel?.Dispose();
            }
            catch { }
            GC.SuppressFinalize(this);
        }
    }
}