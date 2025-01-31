﻿using System.Collections.Concurrent;
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

namespace EBCEYS.RabbitMQ.Client
{
    public class RabbitMQClient : IHostedService, IDisposable, IAsyncDisposable
    {
        private const string contentType = "application-json";

        private readonly bool autoAck = true;

        private readonly ILogger logger;
        private readonly RabbitMQConfiguration configuration;
        private readonly TimeSpan? requestsTimeout;
        private readonly JsonSerializerSettings? serializerOptions;
        private readonly Encoding encoding;

        private readonly ConnectionFactory factory;
        private IConnection? connection;
        private IChannel? channel;
        private BasicProperties? nonRPCProps;


        private string? replyQueueName;
        private AsyncEventingBasicConsumer? consumer;


        private readonly ConcurrentDictionary<string, RabbitMQClientResponse> ResponseDictionary = new();

        public RabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration configuration, TimeSpan? requestsTimeout = null, JsonSerializerSettings? serializerOptions = null)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.requestsTimeout = requestsTimeout;
            this.serializerOptions = serializerOptions;
            encoding = this.configuration.Encoding;
            factory = configuration.Factory ?? throw new ArgumentException(nameof(configuration.Factory));
        }

        public RabbitMQClient(ILogger<RabbitMQClient> logger, Func<RabbitMQConfiguration> configurationFunction, TimeSpan? requestsTimeout = null, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(configurationFunction);

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configurationFunction!.Invoke() ?? throw new ArgumentNullException(nameof(configurationFunction));
            this.requestsTimeout = requestsTimeout;
            this.serializerOptions = serializerOptions;
            encoding = this.configuration.Encoding;
            factory = configuration.Factory ?? throw new ArgumentException(nameof(configuration.Factory));
        }

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


            if (requestsTimeout is not null)
            {
                await channel.BasicQosAsync(configuration.QoSConfiguration, cancellationToken);
                consumer = new(channel);

                replyQueueName = (await channel.QueueDeclareAsync(configuration.CallBackConfiguration?.QueueConfiguration, cancellationToken)).QueueName;
                if (configuration.CallBackConfiguration?.ExchangeConfiguration is not null)
                {
                    await channel.ExchangeDeclareAsync(configuration.CallBackConfiguration.ExchangeConfiguration, cancellationToken);
                    await channel.QueueBindAsync(
                        configuration.CallBackConfiguration?.QueueConfiguration.QueueName ?? replyQueueName,
                        configuration.CallBackConfiguration?.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                        configuration.CallBackConfiguration?.QueueConfiguration.RoutingKey ?? replyQueueName,
                        cancellationToken: cancellationToken);
                }

                consumer.ReceivedAsync += ReceiveAsync;

                await channel.BasicConsumeAsync(replyQueueName, autoAck, consumer, cancellationToken);
            }
        }

        private async Task ReceiveAsync(object model, BasicDeliverEventArgs ea)
        {
            string body = encoding.GetString(ea.Body.ToArray());
            logger.LogTrace("Get rabbit response: {encodedMessage} {id}", body, ea.BasicProperties.CorrelationId);
            RabbitMQRequestProcessingExceptionDTO? exObject = null;
            try
            {
                string? exception = ea.BasicProperties.Headers?.GetHeaderString(RabbitMQServer.ExceptionResponseHeaderKey, Encoding.UTF8);
                if (exception != null)
                {
                    exObject = JsonConvert.DeserializeObject<RabbitMQRequestProcessingExceptionDTO?>(exception);
                }
            }
            catch (Exception deserializeException)
            {
                logger.LogError(deserializeException, "Error on deserializing exception!");
            }
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
        /// Sends message to rabbitMQ queue async.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mandatory">The mandatory.</param>
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

            await channel.BasicPublishAsync(exchange, routingKey, mandatory, nonRPCProps!, msg, token);
        }

        /// <summary>
        /// Sends rabbitMQ request async.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="data">The data to send.</param>
        /// <param name="mandatory">The mandatory.</param>
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

            await channel.BasicPublishAsync(exchange, routingKey, mandatory, props, msg, token);

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
