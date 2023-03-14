using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using System.Text.Json.Serialization;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;

namespace EBCEYS.RabbitMQ.Server.Service
{
    public class RabbitMQServer : IHostedService, IAsyncDisposable, IDisposable
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly ILogger<RabbitMQServer> logger;
        public AsyncEventingBasicConsumer Consumer { get; private set; }
        private readonly RabbitMQConfiguration configuration;
        private AsyncEventHandler<BasicDeliverEventArgs>? consumerAction;

        public JsonSerializerOptions? SerializerOptions { get; private set; }

        public RabbitMQServer(ILogger<RabbitMQServer> logger, 
            RabbitMQConfiguration configuration, 
            AsyncEventHandler<BasicDeliverEventArgs>? consumerAction = null, 
            JsonSerializerOptions? serializerOptions = null)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.consumerAction = consumerAction;
            this.SerializerOptions = serializerOptions ?? new()
            {
                Converters = { new JsonStringEnumConverter(), new JsonStringConverter() },
                WriteIndented = false,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            connection = this.configuration.Factory!.CreateConnection();
            channel = connection.CreateModel();
            this.configuration = configuration;
            Consumer = new(channel);

            this.logger.LogDebug("Create rabbitMQ server service!");
        }

        public void SetConsumerAction(AsyncEventHandler<BasicDeliverEventArgs> consumerAction)
        {
            if (this.consumerAction is not null)
            {
                throw new InvalidOperationException("Consumer action is already set!");
            }
            if (consumerAction is null)
            {
                throw new ArgumentNullException(nameof(consumerAction));
            }
            this.consumerAction = consumerAction;
            logger.LogDebug("Set consumer action!");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (configuration.QueueConfiguration is null)
            {
                throw new Exception($"{nameof(configuration.QueueConfiguration)} is null!");
            }
            await Task.Run(() => 
            { 
                if (configuration.ExchangeConfiguration is not null)
                {
                    channel.ExchangeDeclare(configuration.ExchangeConfiguration.ExchangeName, 
                        configuration.ExchangeConfiguration.ExchangeType, 
                        configuration.ExchangeConfiguration.Durable,
                        configuration.ExchangeConfiguration.AutoDelete,
                        configuration.ExchangeConfiguration.Arguments);
                }

                channel.BasicQos(0, 1, false);
                channel.QueueDeclare(configuration.QueueConfiguration.QueueName, 
                    configuration.QueueConfiguration.Durable, 
                    configuration.QueueConfiguration.Exclusive, 
                    configuration.QueueConfiguration.AutoDelete, 
                    configuration.QueueConfiguration.Arguments);

                Consumer.Received += consumerAction;
                channel.BasicConsume(configuration.QueueConfiguration.QueueName, false, Consumer);

                logger.LogDebug("Consumer status on start: {status}", Consumer.IsRunning);
            }, cancellationToken);
            logger.LogDebug("Start rabbitmq server!");
        }
        /// <summary>
        /// Acks the message. Use it in your consumer action.
        /// </summary>
        /// <param name="ea">The event args.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AckMessage(BasicDeliverEventArgs ea)
        {
            if (ea is null)
            {
                throw new ArgumentNullException(nameof(ea));
            }

            logger.LogDebug("Ack message: {tag}", ea.DeliveryTag);
            channel.BasicAck(ea.DeliveryTag, false);
        }
        /// <summary>
        /// Sends the response. Uses with RPC configuration.
        /// </summary>
        /// <param name="ea">The event arguments.</param>
        /// <param name="response">The response json data.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task SendResponseAsync(BasicDeliverEventArgs ea, object response)
        {
            if (ea is null)
            {
                throw new ArgumentNullException(nameof(ea));
            }

            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (SerializerOptions is null)
            {
                throw new Exception($"{nameof(SerializerOptions)} is null! Can not serialize response!");
            }
            await Task.Run(() =>
            {
                try
                {
                    IBasicProperties replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                    byte[] resp = JsonSerializer.SerializeToUtf8Bytes(response, SerializerOptions);

                    logger.LogInformation("On request {id} response is {resp}", replyProps.CorrelationId, Encoding.UTF8.GetString(resp));

                    channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: replyProps, body: resp);
                    AckMessage(ea);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on responsing!");
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
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on stoping service!");
                }
            }, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                connection.Close();
                connection.Dispose();
                channel.Dispose();
            }
            catch { }
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                connection.Close();
                connection.Dispose();
                channel.Dispose();
            }
            catch { }
            GC.SuppressFinalize(this);
        }
    }
}