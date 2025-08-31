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
namespace EBCEYS.RabbitMQ.Server.Service;
#pragma warning restore IDE0130 // Пространство имен (namespace) не соответствует структуре папок.

/// <summary>
///     A <see cref="RabbitMQServer" /> class.
/// </summary>
public class RabbitMQServer : IHostedService, IAsyncDisposable, IDisposable
{
    private const string ContentType = "application-json";

    /// <summary>
    ///     The exception response header key.
    /// </summary>
    public static readonly string ExceptionResponseHeaderKey = "RabbitMQRequestProcessingException";

    /// <summary>
    ///     The gzip settings header key.
    /// </summary>
    public static readonly string GZipSettingsResponseHeaderKey = "GZipCompressionSettings";

    private readonly bool _autoAck = true;
    private readonly RabbitMQConfiguration _configuration;
    private readonly Encoding _encoding;
    private readonly ILogger<RabbitMQServer> _logger;
    private IChannel? _channel;

    private IConnection? _connection;
    private AsyncEventHandler<BasicDeliverEventArgs>? _consumerAction;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQServer" />.
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
        this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this._consumerAction = consumerAction;
        SerializerOptions = serializerOptions;
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _encoding = this._configuration.Encoding;

        this._logger.LogDebug("Create rabbitMQ server service!");
    }

    /// <summary>
    ///     The consumer.
    /// </summary>
    public AsyncEventingBasicConsumer? Consumer { get; private set; }

    /// <summary>
    ///     The serialization options.
    /// </summary>
    public JsonSerializerSettings? SerializerOptions { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (_connection is not null)
            {
                _connection.CloseAsync().Wait();
                _connection.Dispose();
            }

            _channel?.Dispose();
        }
        catch(Exception)
        {
            // ignored
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_configuration.QueueConfiguration is null)
            throw new InvalidOperationException($"{nameof(_configuration.QueueConfiguration)} is null!");

        _connection =
            await _configuration.Factory.CreateConnectionAsync(_configuration.OnStartConfigs, null, cancellationToken);
        _channel = await _connection.CreateChannelAsync(_configuration.CreateChannelOptions, cancellationToken);
        Consumer = new AsyncEventingBasicConsumer(_channel);

        await _channel.QueueDeclareAsync(_configuration.QueueConfiguration, cancellationToken);
        if (_configuration.ExchangeConfiguration is not null)
        {
            await _channel.ExchangeDeclareAsync(_configuration.ExchangeConfiguration, cancellationToken);
            await _channel.QueueBindAsync(_configuration.QueueConfiguration.QueueName,
                _configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                _configuration.QueueConfiguration.RoutingKey, cancellationToken: cancellationToken);
        }

        await _channel.BasicQosAsync(_configuration.QoSConfiguration, cancellationToken);

        Consumer.ReceivedAsync += _consumerAction;
        await _channel.BasicConsumeAsync(_configuration.QueueConfiguration.QueueName, _autoAck, Consumer,
            cancellationToken);
        _logger.LogDebug("Consumer status on start: {status}", Consumer.IsRunning);
        _logger.LogDebug("Start rabbitmq server!");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection is null) return;
        try
        {
            await _connection.CloseAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on stoping service!");
        }
    }

    /// <summary>
    ///     Sets the consumer action.
    /// </summary>
    /// <param name="consumerAction"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetConsumerAction(AsyncEventHandler<BasicDeliverEventArgs> consumerAction)
    {
        ArgumentNullException.ThrowIfNull(consumerAction);
        this._consumerAction = consumerAction;
        _logger.LogDebug("Set consumer action!");
    }

    /// <summary>
    ///     Acks the message. Use it in your consumer action.
    /// </summary>
    /// <param name="ea">The event args.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task AckMessage(BasicDeliverEventArgs ea, CancellationToken token = default)
    {
        if (!_autoAck)
        {
            ArgumentNullException.ThrowIfNull(ea);

            _logger.LogTrace("Ack message: {tag}", ea.DeliveryTag);
            await _channel!.BasicAckAsync(ea.DeliveryTag, false, token);
        }
    }

    /// <summary>
    ///     Sends the response. Uses with RPC configuration.
    /// </summary>
    /// <param name="ea">The event arguments.</param>
    /// <param name="response">The response json data.</param>
    /// <param name="gzip">The gzip compression settings.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SendResponseAsync<T>(BasicDeliverEventArgs ea, T response, GZipSettings? gzip,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(ea);

        if (ea.BasicProperties.ReplyToAddress is null)
            throw new InvalidOperationException("Event args do not contains ReplyTo params!");
        try
        {
            var json = JsonConvert.SerializeObject(response, SerializerOptions);
            var resp = _encoding.GetBytes(json);

            Dictionary<string, object?> headers = [];
            if (gzip is { GZiped: true })
            {
                headers.TryAdd(GZipSettingsResponseHeaderKey, new byte[] { 1 });
                resp = GZipSettings.GZipCompress(resp, gzip);
            }

            BasicProperties replyProps = new()
            {
                ContentType = ContentType,
                ContentEncoding = _encoding.EncodingName,
                CorrelationId = ea.BasicProperties.CorrelationId,
                Headers = headers
            };

            _logger.LogTrace("On request {id} response is {resp}", replyProps.CorrelationId, json);

            await _channel!.BasicPublishAsync(ea.BasicProperties.ReplyToAddress.ExchangeName,
                ea.BasicProperties.ReplyToAddress.RoutingKey, false, replyProps, resp, token);
            await AckMessage(ea, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on responding!");
        }
    }

    /// <summary>
    ///     Sends an exception response.<br />
    ///     Throws <see cref="InvalidOperationException" /> if <paramref name="ea" /> doesn't contain ReplyToAddress.
    /// </summary>
    /// <param name="ea">The received message args.</param>
    /// <param name="processingException">The processing exception.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SendExceptionResponseAsync(BasicDeliverEventArgs ea,
        RabbitMQRequestProcessingException processingException, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(ea);

        ArgumentNullException.ThrowIfNull(processingException);

        if (ea.BasicProperties.ReplyToAddress is null)
            throw new InvalidOperationException("Event args do not contains ReplyTo params!");
        try
        {
            var obj = processingException.GetDto();
            var jsonException = JsonConvert.SerializeObject(obj, SerializerOptions);
            BasicProperties replyProps = new()
            {
                ContentType = ContentType,
                ContentEncoding = _encoding.EncodingName,
                CorrelationId = ea.BasicProperties.CorrelationId,
                Headers = new Dictionary<string, object?>
                {
                    { ExceptionResponseHeaderKey, _encoding.GetBytes(jsonException) }
                }
            };
            var body = _encoding.GetBytes("{}");
            _logger.LogTrace("On request {id} exception response is {ex}", replyProps.CorrelationId, jsonException);


            await _channel!.BasicPublishAsync(ea.BasicProperties.ReplyToAddress.ExchangeName,
                ea.BasicProperties.ReplyToAddress.RoutingKey, false, replyProps, body, token);
            await AckMessage(ea, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on responsing with error!");
        }
    }
}