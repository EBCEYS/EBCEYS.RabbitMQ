using System.Collections.Concurrent;
using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

#pragma warning disable IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
// ReSharper disable once CheckNamespace
namespace EBCEYS.RabbitMQ.Client;
#pragma warning restore IDE0130 // Пространство имен (namespace) не соответствует структуре папок.

/// <summary>
///     The <see cref="RabbitMQClient" /> class.
/// </summary>
public class RabbitMQClient : IRabbitMQClient
{
    private const string ContentType = "application-json";

    private readonly bool _autoAck = true;
    private readonly RabbitMQConfiguration _configuration;
    private readonly Encoding _encoding;

    private readonly ILogger _logger;
    private readonly TimeSpan? _requestsTimeout;


    private readonly ConcurrentDictionary<string, TaskCompletionSource<RabbitMQClientResponse>> _responseDictionary =
        new();

    private readonly JsonSerializerSettings? _serializerOptions;
    private IChannel? _channel;

    private IConnection? _connection;
    private AsyncEventingBasicConsumer? _consumer;
    private BasicProperties? _nonRpcProps;


    private string? _replyQueueName;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQClient" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The rabbitmq configuration.</param>
    /// <param name="serializerOptions">The serializer options.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public RabbitMQClient(ILogger<RabbitMQClient> logger, RabbitMQConfiguration configuration,
        JsonSerializerSettings? serializerOptions = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _requestsTimeout = configuration.CallBackConfiguration?.RequestsTimeout;
        _serializerOptions = serializerOptions;
        _encoding = _configuration.Encoding;
    }

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQClient" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationFunction">The creation configuration function.</param>
    /// <param name="serializerOptions">The serializer options.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public RabbitMQClient(ILogger<RabbitMQClient> logger, Func<RabbitMQConfiguration> configurationFunction,
        JsonSerializerSettings? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configurationFunction);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configurationFunction.Invoke() ??
                         throw new ArgumentNullException(nameof(configurationFunction));
        _requestsTimeout = _configuration.CallBackConfiguration?.RequestsTimeout;
        _serializerOptions = serializerOptions;
        _encoding = _configuration.Encoding;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection =
            await _configuration.Factory.CreateConnectionAsync(_configuration.OnStartConfigs, null, cancellationToken);
        _channel = await _connection.CreateChannelAsync(_configuration.CreateChannelOptions, cancellationToken);
        _nonRpcProps = new BasicProperties
        {
            ContentEncoding = _encoding.EncodingName,
            ContentType = ContentType,
            Headers = new Dictionary<string, object?>()
        };

        await _channel.QueueDeclareAsync(_configuration.QueueConfiguration, cancellationToken);
        if (_configuration.ExchangeConfiguration is not null)
        {
            await _channel.ExchangeDeclareAsync(_configuration.ExchangeConfiguration, cancellationToken);
            await _channel.QueueBindAsync(_configuration.QueueConfiguration.QueueName,
                _configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                _configuration.QueueConfiguration.RoutingKey, cancellationToken: cancellationToken);
        }


        if (_configuration.CallBackConfiguration is not null)
        {
            await _channel.BasicQosAsync(_configuration.QoSConfiguration, cancellationToken);
            _consumer = new AsyncEventingBasicConsumer(_channel);

            _replyQueueName =
                (await _channel.QueueDeclareAsync(_configuration.CallBackConfiguration?.QueueConfiguration,
                    cancellationToken)).QueueName;
            if (_configuration.CallBackConfiguration?.ExchangeConfiguration is not null)
            {
                await _channel.ExchangeDeclareAsync(_configuration.CallBackConfiguration.ExchangeConfiguration,
                    cancellationToken);
                await _channel.QueueBindAsync(
                    _configuration.CallBackConfiguration.QueueConfiguration.QueueName,
                    _configuration.CallBackConfiguration.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                    _configuration.CallBackConfiguration.QueueConfiguration.RoutingKey,
                    cancellationToken: cancellationToken);
            }

            _consumer.ReceivedAsync += ReceiveAsync;

            await _channel.BasicConsumeAsync(_replyQueueName, _autoAck, _consumer, cancellationToken);
        }
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
    ///     Sends message to rabbitMQ.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="RabbitMQClientException"></exception>
    /// <returns>Task.</returns>
    public virtual async Task SendMessageAsync(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        if (!_connection!.IsOpen || !_channel!.IsOpen) throw new RabbitMQClientException("Connection is not opened!");
        var json = JsonConvert.SerializeObject(data, _serializerOptions);
        var msg = _encoding.GetBytes(json);

        _logger.LogDebug("Try to send message {msg}", json);

        var exchange = _configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty;
        var routingKey = _configuration.QueueConfiguration.RoutingKey;
        await PostMessageAsync(mandatory, msg, exchange, routingKey, data.GZip, _nonRpcProps!, token);
    }

    /// <summary>
    ///     Sends rabbitMQ request.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="data">The data to send.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Response data or default if timeout.</returns>
    /// <exception cref="RabbitMQClientException"></exception>
    /// <exception cref="RabbitMQRequestProcessingException"></exception>
    public virtual async Task<T?> SendRequestAsync<T>(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        if (!_connection!.IsOpen || !_channel!.IsOpen) throw new RabbitMQClientException("Connection is not opened!");
        if (_requestsTimeout is null) throw new RabbitMQClientException("Can not send request! Timeout is not exists!");
        var json = JsonConvert.SerializeObject(data, _serializerOptions);
        var msg = _encoding.GetBytes(json);
        _logger.LogDebug("Try to send request {msg}", json);

        var props = GetRpcProps();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(_requestsTimeout.Value);
        
        var tcs = new TaskCompletionSource<RabbitMQClientResponse>(cts.Token,
            TaskCreationOptions.RunContinuationsAsynchronously);
        _responseDictionary.TryAdd(props.CorrelationId!, tcs);

        var exchange = _configuration.ExchangeConfiguration?.ExchangeName ?? string.Empty;
        var routingKey = _configuration.QueueConfiguration.RoutingKey;

        await PostMessageAsync(mandatory, msg, exchange, routingKey, data.GZip, props, cts.Token);

        try
        {
            var response = await tcs.Task;

            if (response.RequestProcessingException != null)
                return _configuration.OnStartConfigs.ThrowServerExceptionsOnReceivingResponse
                    ? throw response.RequestProcessingException
                    : default;

            if (response.Response is null)
            {
                _logger.LogError("Response is null");
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T?>(response.Response, _serializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on converting data {@data} to type {typename}", response, typeof(T).Name);
                return default;
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _responseDictionary.TryRemove(props.CorrelationId!, out _);
        }

        _logger.LogError("Response on request {requestId} is timeout!", props.CorrelationId!);
        return default;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            _connection?.CloseAsync().Wait();
            _connection?.Dispose();
            _channel?.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_connection is not null) await _connection.CloseAsync();
            _connection?.Dispose();
            _channel?.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }

        GC.SuppressFinalize(this);
    }

    private async Task ReceiveAsync(object model, BasicDeliverEventArgs ea)
    {
        var exObject = GetExceptionHeader(ea);
        var gZipSettings = ea.BasicProperties.Headers?.GetHeaderBytes(RabbitMQServer.GZipSettingsResponseHeaderKey)
            ?.FirstOrDefault() == 1;
        var body = _encoding.GetString(GZipSettings.GZipDecompress(ea.Body.ToArray(), new GZipSettings
        {
            GZiped = gZipSettings
        }));
        _logger.LogTrace("Get rabbit response: {encodedMessage} {id}", body, ea.BasicProperties.CorrelationId);
        if (_responseDictionary.TryGetValue(ea.BasicProperties.CorrelationId ?? "", out var value))
            value.TrySetResult(new RabbitMQClientResponse
            {
                Response = body,
                RequestProcessingException = RabbitMQRequestProcessingException.CreateFromDto(exObject)
            });
        if (!_autoAck) await _channel!.BasicAckAsync(ea.DeliveryTag, false);
    }

    private RabbitMQRequestProcessingExceptionDto? GetExceptionHeader(BasicDeliverEventArgs ea)
    {
        return GetObjectFromHeaders<RabbitMQRequestProcessingExceptionDto>(ea,
            RabbitMQServer.ExceptionResponseHeaderKey);
    }

    private T? GetObjectFromHeaders<T>(BasicDeliverEventArgs ea, string headerKey) where T : class
    {
        T? result = null;
        try
        {
            var resultString = ea.BasicProperties.Headers?.GetHeaderString(headerKey, _encoding);
            if (resultString != null) result = JsonConvert.DeserializeObject<T?>(resultString, _serializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on deserializing header key {headerKey}", headerKey);
        }

        return result;
    }

    private async Task PostMessageAsync(bool mandatory, byte[] msg, string exchange, string routingKey,
        GZipSettings? gziped, BasicProperties props, CancellationToken token = default)
    {
        if (gziped?.GZiped ?? false)
            props.Headers?.Add(RabbitMQServer.GZipSettingsResponseHeaderKey, new byte[] { 1 });
        await _channel!.BasicPublishAsync(exchange, routingKey, mandatory, props, GZipSettings.GZipCompress(msg, gziped),
            token);
    }

    private BasicProperties GetRpcProps()
    {
        return new BasicProperties
        {
            ContentType = _nonRpcProps?.ContentType,
            ContentEncoding = _nonRpcProps?.ContentEncoding,
            CorrelationId = StringExtensions.ConcatStrings(Guid.NewGuid().ToString(), "_",
                _configuration.CallBackConfiguration?.QueueConfiguration.RoutingKey ?? string.Empty),
            ReplyToAddress = new PublicationAddress(
                _configuration.CallBackConfiguration?.ExchangeConfiguration?.ExchangeType ??
                ExchangeTypeExtensions.ParseFromEnum(ExchangeTypes.Direct),
                _configuration.CallBackConfiguration?.ExchangeConfiguration?.ExchangeName ?? string.Empty,
                _configuration.CallBackConfiguration?.QueueConfiguration.RoutingKey ?? _replyQueueName!),
            Headers = new Dictionary<string, object?>()
        };
    }
}