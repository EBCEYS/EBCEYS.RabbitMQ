using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

#pragma warning disable IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
namespace EBCEYS.RabbitMQ.Server.Service
#pragma warning restore IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
{
    /// <summary>
    /// A <see cref="RabbitMQServer"/> class.
    /// </summary>
    public class RabbitMQServer : IHostedService, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// The exception response header key.
        /// </summary>
        public static readonly string ExceptionResponseHeaderKey = "RabbitMQRequestProcessingException";
        /// <summary>
        /// The gzip settings header key.
        /// </summary>
        public static readonly string GZipSettingsResponseHeaderKey = "GZipCompressionSettings";

        private readonly bool autoAck = true;
        private const string contentType = "application-json";

        private IConnection? connection;
        private IChannel? channel;
        private readonly ILogger<RabbitMQServer> logger;
        /// <summary>
        /// The consumer.
        /// </summary>
        public AsyncEventingBasicConsumer? Consumer { get; private set; }
        private readonly RabbitMQConfiguration configuration;
        private AsyncEventHandler<BasicDeliverEventArgs>? consumerAction;
        /// <summary>
        /// The serialization options.
        /// </summary>
        public JsonSerializerSettings? SerializerOptions { get; private set; }
        private readonly Encoding encoding;
        /// <summary>
        /// Initiates a new instance of the <see cref="RabbitMQServer"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="consumerAction"></param>
        /// <param name="serializerOptions"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQServer(ILogger<RabbitMQServer> logger,
            RabbitMQConfiguration configuration,
            AsyncEventHandler<BasicDeliverEventArgs>? consumerAction = null,
            JsonSerializerSettings? serializerOptions = null)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.consumerAction = consumerAction;
            this.SerializerOptions = serializerOptions;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.encoding = this.configuration.Encoding;

            this.logger.LogDebug("Create rabbitMQ server service!");
        }
        /// <summary>
        /// Sets the consumer action.
        /// </summary>
        /// <param name="consumerAction"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetConsumerAction(AsyncEventHandler<BasicDeliverEventArgs> consumerAction)
        {
            ArgumentNullException.ThrowIfNull(consumerAction);
            this.consumerAction = consumerAction;
            logger.LogDebug("Set consumer action!");
        }
        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (configuration.QueueConfiguration is null)
            {
                throw new InvalidOperationException($"{nameof(configuration.QueueConfiguration)} is null!");
            }

            connection = await configuration.Factory.CreateConnectionAsync(configuration.OnStartConfigs, null, cancellationToken);
            channel = await connection.CreateChannelAsync(configuration.CreateChannelOptions, cancellationToken);
            Consumer = new(channel);

            await channel.QueueDeclareAsync(configuration.QueueConfiguration!, cancellationToken);
            if (configuration.ExchangeConfiguration is not null)
            {
                await channel.ExchangeDeclareAsync(configuration.ExchangeConfiguration, cancellationToken);
                await channel.QueueBindAsync(configuration.QueueConfiguration!.QueueName!, configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty, configuration.QueueConfiguration!.RoutingKey!, cancellationToken: cancellationToken);
            }

            await channel.BasicQosAsync(configuration.QoSConfiguration, cancellationToken);

            Consumer.ReceivedAsync += consumerAction;
            await channel.BasicConsumeAsync(configuration.QueueConfiguration.QueueName, autoAck, Consumer, cancellationToken: cancellationToken);
            logger.LogDebug("Consumer status on start: {status}", Consumer.IsRunning);
            logger.LogDebug("Start rabbitmq server!");
        }

        /// <summary>
        /// Acks the message. Use it in your consumer action.
        /// </summary>
        /// <param name="ea">The event args.</param>
        /// <param name="token">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task AckMessage(BasicDeliverEventArgs ea, CancellationToken token = default)
        {
            if (!autoAck)
            {
                ArgumentNullException.ThrowIfNull(ea);

                logger.LogTrace("Ack message: {tag}", ea.DeliveryTag);
                await channel!.BasicAckAsync(ea.DeliveryTag, false, token);
            }
        }
        /// <summary>
        /// Sends the response. Uses with RPC configuration.
        /// </summary>
        /// <param name="ea">The event arguments.</param>
        /// <param name="response">The response json data.</param>
        /// <param name="gzip">The gzip compression settings.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendResponseAsync<T>(BasicDeliverEventArgs ea, T response, GZipSettings? gzip, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(ea);

            if (ea.BasicProperties.ReplyToAddress is null)
            {
                throw new InvalidOperationException("Event args do not contains ReplyTo params!");
            }
            try
            {
                string json = JsonConvert.SerializeObject(response, SerializerOptions);
                byte[] resp = encoding.GetBytes(json);

                Dictionary<string, object?> headers = [];
                if (gzip is not null && gzip.GZiped)
                {
                    headers.TryAdd(GZipSettingsResponseHeaderKey, new byte[1] { 1 });
                    resp = GZipSettings.GZipCompress(resp, gzip);
                }
                BasicProperties replyProps = new()
                {
                    ContentType = contentType,
                    ContentEncoding = encoding.EncodingName,
                    CorrelationId = ea.BasicProperties.CorrelationId,
                    Headers = headers,
                };

                logger.LogTrace("On request {id} response is {resp}", replyProps.CorrelationId, json);

                await channel!.BasicPublishAsync(ea.BasicProperties.ReplyToAddress.ExchangeName, ea.BasicProperties.ReplyToAddress.RoutingKey, false, replyProps, resp, token);
                await AckMessage(ea, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on responsing!");
            }
        }
        /// <summary>
        /// Sends an exception response.<br/>
        /// Throws <see cref="InvalidOperationException"/> if <paramref name="ea"/> doesn't contain ReplyToAddress.
        /// </summary>
        /// <param name="ea">The received message args.</param>
        /// <param name="processingException">The processing exception.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendExceptionResponseAsync(BasicDeliverEventArgs ea, RabbitMQRequestProcessingException processingException, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(ea);

            ArgumentNullException.ThrowIfNull(processingException);

            if (ea.BasicProperties.ReplyToAddress is null)
            {
                throw new InvalidOperationException("Event args do not contains ReplyTo params!");
            }
            try
            {
                RabbitMQRequestProcessingExceptionDTO obj = processingException.GetDTO();
                string jsonException = JsonConvert.SerializeObject(obj, SerializerOptions);
                BasicProperties replyProps = new()
                {
                    ContentType = contentType,
                    ContentEncoding = encoding.EncodingName,
                    CorrelationId = ea.BasicProperties.CorrelationId,
                    Headers = new Dictionary<string, object?>()
                    {
                        { ExceptionResponseHeaderKey, encoding.GetBytes(jsonException) }
                    }
                };
                byte[] body = encoding.GetBytes("{}");
                logger.LogTrace("On request {id} exception response is {ex}", replyProps.CorrelationId, jsonException);


                await channel!.BasicPublishAsync(ea.BasicProperties.ReplyToAddress.ExchangeName, ea.BasicProperties.ReplyToAddress.RoutingKey, false, replyProps, body, token);
                await AckMessage(ea, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on responsing with error!");
            }
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (connection is not null)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }
            catch { }
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
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