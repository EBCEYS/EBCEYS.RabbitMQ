﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using RabbitMQ.Configuration;

namespace RabbitMQServerService
{
    public class RabbitMQServer : IHostedService, IAsyncDisposable
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly ILogger logger;
        private readonly AsyncEventingBasicConsumer consumer;
        private readonly RabbitMQConfiguration configuration;
        private AsyncEventHandler<BasicDeliverEventArgs>? consumerAction;
        private readonly JsonSerializerOptions? serializerOptions;

        public RabbitMQServer(ILogger logger, 
            RabbitMQConfiguration configuration, 
            AsyncEventHandler<BasicDeliverEventArgs>? consumerAction = null, 
            JsonSerializerOptions? serializerOptions = null)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.consumerAction = consumerAction;
            this.serializerOptions = serializerOptions;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            connection = this.configuration.Factory!.CreateConnection();
            channel = connection.CreateModel();
            this.configuration = configuration;
            consumer = new(channel);

            this.logger.LogInformation("Create rabbitMQ server service!");
        }

        public void SetConsumerAction(AsyncEventHandler<BasicDeliverEventArgs> consumerAction)
        {
            if (this.consumerAction != null)
            {
                throw new InvalidOperationException("Consumer action is already set!");
            }
            if (consumerAction is null)
            {
                throw new ArgumentNullException(nameof(consumerAction));
            }
            this.consumerAction = consumerAction;
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
                channel.BasicConsume(configuration.QueueConfiguration.QueueName, false, consumer);

                consumer.Received += consumerAction;
            }, cancellationToken);
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

            if (serializerOptions is null)
            {
                throw new Exception($"{nameof(serializerOptions)} is null! Can not serialize response!");
            }
            await Task.Run(() =>
            {
                try
                {
                    IBasicProperties replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                    byte[] resp = JsonSerializer.SerializeToUtf8Bytes(response, serializerOptions);

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
            await DisposeAsync();
        }

        public ValueTask DisposeAsync()
        {
            connection.Dispose();
            channel.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}