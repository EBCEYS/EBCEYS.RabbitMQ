using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBCEYS.RabbitMQ.Client
{
    public class RabbitMQClient : IHostedService, IDisposable, IAsyncDisposable
    {
        private readonly ILogger logger;
        private readonly RabbitMQConfiguration configuration;
        private readonly TimeSpan? requestsTimeout;
        private readonly JsonSerializerOptions? serializerOptions;
        private readonly ConnectionFactory factory;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly IBasicProperties nonRPCProps;


        private string? replyQueueName;
        private AsyncEventingBasicConsumer? consumer;


        private readonly ConcurrentDictionary<string, RabbitMQClientResponse> ResponseDictionary = new();

        public RabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration configuration, TimeSpan? requestsTimeout = null, JsonSerializerOptions? serializerOptions = null)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.requestsTimeout = requestsTimeout;
            this.serializerOptions = serializerOptions ?? new()
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            factory = configuration.Factory ?? throw new ArgumentException(nameof(configuration.Factory));

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            nonRPCProps = channel.CreateBasicProperties();
        }

        public RabbitMQClient(ILogger logger, Func<RabbitMQConfiguration> configurationFunction, TimeSpan? requestsTimeout = null, JsonSerializerOptions? serializerOptions = null)
        {
            if (configurationFunction is null)
            {
                throw new ArgumentNullException(nameof(configurationFunction));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configurationFunction!.Invoke() ?? throw new ArgumentNullException(nameof(configurationFunction));
            this.requestsTimeout = requestsTimeout;
            this.serializerOptions = serializerOptions ?? new()
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            factory = configuration.Factory ?? throw new ArgumentException(nameof(configuration.Factory));

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            nonRPCProps = channel.CreateBasicProperties();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => 
            { 
                if(configuration.ExchangeConfiguration is not null)
                {
                    channel.ExchangeDeclare(configuration.ExchangeConfiguration.ExchangeName,
                            configuration.ExchangeConfiguration.ExchangeType,
                            configuration.ExchangeConfiguration.Durable,
                            configuration.ExchangeConfiguration.AutoDelete,
                            configuration.ExchangeConfiguration.Arguments);
                }                
                
                channel.QueueDeclare(
                    queue: configuration.QueueConfiguration.QueueName, 
                    durable: configuration.QueueConfiguration.Durable, 
                    exclusive: configuration.QueueConfiguration.Exclusive, 
                    autoDelete: configuration.QueueConfiguration.AutoDelete, 
                    arguments: configuration.QueueConfiguration.Arguments);

                if (requestsTimeout is not null)
                {
                    consumer = new(channel);

                    replyQueueName = channel.QueueDeclare().QueueName;

                    consumer.Received += ReceiveAsync;

                    channel.BasicConsume(
                        consumer: consumer,
                        queue: replyQueueName,
                        autoAck: false);
                }
            }, cancellationToken);
        }

        private async Task ReceiveAsync(object model, BasicDeliverEventArgs ea)
        {
            await Task.Run(() =>
            {
                string body = Encoding.UTF8.GetString(ea.Body.ToArray());
                logger.LogInformation("Get rabbit response: {encodedMessage} {id}", body, ea.BasicProperties.CorrelationId);
                if (body is not null)
                {
                    if (ResponseDictionary.TryGetValue(ea.BasicProperties.CorrelationId, out RabbitMQClientResponse? value) && value is not null)
                    {
                        value.Response = body;
                        value.Event!.Set();
                        logger.LogDebug("Set event!");
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => 
            {
                try
                {
                    connection.Close();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error on stoping service!");
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Sends message to rabbitMQ queue async.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>Task.</returns>
        public async Task SendMessageAsync(RabbitMQRequestData data)
        {
            if (!connection.IsOpen || !channel.IsOpen)
            {
                throw new RabbitMQClientException("Connection is not opened!");
            }
            await Task.Run(() =>
            {

                byte[] msg = JsonSerializer.SerializeToUtf8Bytes(data, serializerOptions);

                logger.LogDebug("Try to send message {msg}", Encoding.UTF8.GetString(msg));

                string exchange = configuration.ExchangeConfiguration?.ExchangeName ?? "";
                string queue = configuration.QueueConfiguration.QueueName;

                channel.BasicPublish(
                exchange: exchange,
                routingKey: queue,
                basicProperties: nonRPCProps,
                body: msg);
            });
        }

        /// <summary>
        /// Sends rabbitMQ request async.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="data">The data to send.</param>
        /// <returns>Response data or default if timeout.</returns>
        /// <exception cref="RabbitMQClientException"></exception>
        public async Task<T?> SendRequestAsync<T>(RabbitMQRequestData data)
        {
            if (!connection.IsOpen || !channel.IsOpen)
            {
                throw new RabbitMQClientException("Connection is not opened!");
            }
            if (requestsTimeout is null)
            {
                throw new RabbitMQClientException("Can not send request! Timeout is not exists!");
            }
            return await Task.Run(() =>
            {
                byte[] msg = JsonSerializer.SerializeToUtf8Bytes(data, serializerOptions);
                logger.LogDebug("Try to send request {msg}", Encoding.UTF8.GetString(msg));

                GetRPCProps(out IBasicProperties props, out string correlationId);

                ManualResetEvent @event = new(false);
                ResponseDictionary.TryAdd(correlationId, new()
                {
                    Event = @event
                });

                string exchange = configuration.ExchangeConfiguration?.ExchangeName ?? "";
                string queue = configuration.QueueConfiguration.QueueName;

                channel.BasicPublish(
                    exchange: exchange,
                    routingKey: queue,
                    basicProperties: props,
                    body: msg);

                @event.WaitOne(requestsTimeout.Value);
                if (ResponseDictionary.TryRemove(correlationId, out RabbitMQClientResponse? response) && response != null && response.Response != null)
                {
                    try
                    {
                        return JsonSerializer.Deserialize<T?>(response.Response!, serializerOptions);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error on converting data {@data} to type {typename}", response, typeof(T).Name);
                    }
                }
                logger.LogError("Response on request {requestId} is timeouted!", correlationId);
                return default;
            });
        }

        private void GetRPCProps(out IBasicProperties props, out string correlationId)
        {
            props = channel.CreateBasicProperties();
            correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
            channel.Dispose();
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            connection.Close();
            connection.Dispose();
            channel.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}
