using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

#pragma warning disable IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
namespace EBCEYS.RabbitMQ.Client
#pragma warning restore IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
{
    /// <summary>
    /// The <see cref="RabbitMQClient"/> class.
    /// </summary>
    public class RabbitMQClient : IRabbitMQClient
    {
        private const string contentType = "application-json";

        private readonly bool autoAck = true;

        private readonly ILogger logger;
        private readonly RabbitMQConfiguration configuration;
        private readonly TimeSpan? requestsTimeout;
        private readonly JsonSerializerSettings? serializerOptions;
        private readonly Encoding encoding;

        private IConnection? connection;
        private IChannel? channel;
        private BasicProperties? nonRPCProps;


        private string? replyQueueName;
        private AsyncEventingBasicConsumer? consumer;


        private readonly ConcurrentDictionary<string, RabbitMQClientResponse> ResponseDictionary = new();
        /// <summary>
        /// Initiates a new instance of the <see cref="RabbitMQClient"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The rabbitmq configuration.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration configuration, JsonSerializerSettings? serializerOptions = null)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.requestsTimeout = configuration.CallBackConfiguration?.RequestsTimeout;
            this.serializerOptions = serializerOptions;
            encoding = this.configuration.Encoding;
        }
        /// <summary>
        /// Initiates a new instance of the <see cref="RabbitMQClient"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationFunction">The creation configuration function.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public RabbitMQClient(ILogger<RabbitMQClient> logger, Func<RabbitMQConfiguration> configurationFunction, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(configurationFunction);

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configurationFunction!.Invoke() ?? throw new ArgumentNullException(nameof(configurationFunction));
            this.requestsTimeout = configuration.CallBackConfiguration?.RequestsTimeout;
            this.serializerOptions = serializerOptions;
            encoding = this.configuration.Encoding;
        }
        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            connection = await configuration.Factory.CreateConnectionAsync(configuration.OnStartConfigs, null, cancellationToken);
            channel = await connection.CreateChannelAsync(configuration.CreateChannelOptions, cancellationToken);
            nonRPCProps = new BasicProperties()
            {
                ContentEncoding = encoding.EncodingName,
                ContentType = contentType
            };

            await channel.QueueDeclareAsync(configuration.QueueConfiguration!, cancellationToken);
            if (configuration.ExchangeConfiguration is not null)
            {
                await channel.ExchangeDeclareAsync(configuration.ExchangeConfiguration, cancellationToken);
                await channel.QueueBindAsync(configuration.QueueConfiguration!.QueueName, configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty, configuration.QueueConfiguration!.RoutingKey, cancellationToken: cancellationToken);
            }


            if (configuration.CallBackConfiguration is not null)
            {
                await channel.BasicQosAsync(configuration.QoSConfiguration, cancellationToken);
                consumer = new(channel);

                replyQueueName = (await channel.QueueDeclareAsync(configuration.CallBackConfiguration?.QueueConfiguration, cancellationToken)).QueueName;
                if (configuration.CallBackConfiguration?.ExchangeConfiguration is not null)
                {
                    await channel.ExchangeDeclareAsync(configuration.CallBackConfiguration.ExchangeConfiguration, cancellationToken);
                    await channel.QueueBindAsync(
                        configuration.CallBackConfiguration.QueueConfiguration.QueueName ?? replyQueueName,
                        configuration.CallBackConfiguration.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                        configuration.CallBackConfiguration.QueueConfiguration.RoutingKey ?? replyQueueName,
                        cancellationToken: cancellationToken);
                }

                consumer.ReceivedAsync += ReceiveAsync;

                await channel.BasicConsumeAsync(replyQueueName, autoAck, consumer, cancellationToken);
            }
        }

        private async Task ReceiveAsync(object model, BasicDeliverEventArgs ea)
        {
            RabbitMQRequestProcessingExceptionDTO? exObject = GetExceptionHeader(ea);
            bool gZipSettings = ea.BasicProperties.Headers?.GetHeaderBytes(RabbitMQServer.GZipSettingsResponseHeaderKey)?.FirstOrDefault() == 1;
            string body = encoding.GetString(GZipSettings.GZipDecompress(ea.Body.ToArray(), new()
            {
                GZiped = gZipSettings
            }));
            logger.LogTrace("Get rabbit response: {encodedMessage} {id}", body, ea.BasicProperties.CorrelationId);
            if (ResponseDictionary.TryGetValue(ea.BasicProperties.CorrelationId ?? "", out RabbitMQClientResponse? value) && value is not null)
            {
                value.Response = body;
                value.RequestProcessingException = RabbitMQRequestProcessingException.CreateFromDTO(exObject);
                value.Event!.Set();
            }
            if (!autoAck)
            {
                await channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
        }

        private RabbitMQRequestProcessingExceptionDTO? GetExceptionHeader(BasicDeliverEventArgs ea)
        {
            return GetObjectFromHeaders<RabbitMQRequestProcessingExceptionDTO>(ea, RabbitMQServer.ExceptionResponseHeaderKey);
        }
        private T? GetObjectFromHeaders<T>(BasicDeliverEventArgs ea, string headerKey) where T : class
        {
            T? result = null;
            try
            {
                string? resultString = ea.BasicProperties.Headers?.GetHeaderString(headerKey, encoding);
                if (resultString != null)
                {
                    result = JsonConvert.DeserializeObject<T?>(resultString, serializerOptions);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on deserializing header key {headerKey}", headerKey);
            }
            return result;
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
                await connection.CloseAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on stoping service!");
            }
        }

        /// <summary>
        /// Sends message to rabbitMQ.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mandatory">The mandatory.</param>
        /// <param name="token">The cancellation token.</param>
        /// <exception cref="RabbitMQClientException"></exception>
        /// <returns>Task.</returns>
        public virtual async Task SendMessageAsync(RabbitMQRequestData data, bool mandatory = false, CancellationToken token = default)
        {
            if (!connection!.IsOpen || !channel!.IsOpen)
            {
                throw new Server.MappedService.Exceptions.RabbitMQClientException("Connection is not opened!");
            }
            string json = JsonConvert.SerializeObject(data, serializerOptions);
            byte[] msg = encoding.GetBytes(json);

            logger.LogDebug("Try to send message {msg}", json);

            string exchange = configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty;
            string routingKey = configuration.QueueConfiguration!.RoutingKey;
            await PostMessageAsync(mandatory, msg, exchange, routingKey, data.GZip, nonRPCProps!, token);
        }

        private async Task PostMessageAsync(bool mandatory, byte[] msg, string exchange, string routingKey, GZipSettings? gziped, BasicProperties props, CancellationToken token = default)
        {
            await channel!.BasicPublishAsync(exchange, routingKey, mandatory, props, GZipSettings.GZipCompress(msg, gziped), token);
        }

        /// <summary>
        /// Sends rabbitMQ request.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="data">The data to send.</param>
        /// <param name="mandatory">The mandatory.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Response data or default if timeout.</returns>
        /// <exception cref="RabbitMQClientException"></exception>
        /// <exception cref="RabbitMQRequestProcessingException"></exception>
        public virtual async Task<T?> SendRequestAsync<T>(RabbitMQRequestData data, bool mandatory = false, CancellationToken token = default)
        {
            if (!connection!.IsOpen || !channel!.IsOpen)
            {
                throw new Server.MappedService.Exceptions.RabbitMQClientException("Connection is not opened!");
            }
            if (requestsTimeout is null)
            {
                throw new Server.MappedService.Exceptions.RabbitMQClientException("Can not send request! Timeout is not exists!");
            }
            string json = JsonConvert.SerializeObject(data, serializerOptions);
            byte[] msg = encoding.GetBytes(json);
            logger.LogDebug("Try to send request {msg}", json);

            BasicProperties props = GetRPCProps();

            using ManualResetEvent @event = new(false);
            ResponseDictionary.TryAdd(props.CorrelationId!, new()
            {
                Event = @event
            });

            string exchange = configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty;
            string routingKey = configuration.QueueConfiguration!.RoutingKey;

            await PostMessageAsync(mandatory, msg, exchange, routingKey, data.GZip, props, token);

            @event.WaitOne(requestsTimeout.Value);
            if (ResponseDictionary.TryRemove(props.CorrelationId!, out RabbitMQClientResponse? response) && response != null && response.Response != null)
            {
                if (response.RequestProcessingException != null)
                {
                    if (configuration.OnStartConfigs.ThrowServerExceptionsOnReceivingResponse)
                    {
                        throw response.RequestProcessingException;
                    }
                    return default;
                }
                try
                {
                    return JsonConvert.DeserializeObject<T?>(response.Response, serializerOptions);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on converting data {@data} to type {typename}", response, typeof(T).Name);
                    return default;
                }
            }
            logger.LogError("Response on request {requestId} is timeouted!", props.CorrelationId!);
            return default;
        }

        private BasicProperties GetRPCProps()
        {
            return new()
            {
                ContentType = nonRPCProps?.ContentType,
                ContentEncoding = nonRPCProps?.ContentEncoding,
                CorrelationId = StringExtensions.ConcatStrings(Guid.NewGuid().ToString(), "_", configuration.CallBackConfiguration?.QueueConfiguration.RoutingKey ?? string.Empty),
                ReplyToAddress = new(
                    configuration.CallBackConfiguration?.ExchangeConfiguration?.ExchangeType ?? ExchangeTypeExtensions.ParseFromEnum(ExchangeTypes.Direct),
                    configuration.CallBackConfiguration?.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                    configuration.CallBackConfiguration?.QueueConfiguration.RoutingKey ?? replyQueueName!)
            };
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                connection?.CloseAsync().Wait();
                connection?.Dispose();
                channel?.Dispose();
            }
            catch (Exception)
            {

            }
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (connection is not null)
                {
                    await connection.CloseAsync();
                }
                connection?.Dispose();
                channel?.Dispose();
            }
            catch (Exception)
            {

            }
            GC.SuppressFinalize(this);
        }
    }
}
