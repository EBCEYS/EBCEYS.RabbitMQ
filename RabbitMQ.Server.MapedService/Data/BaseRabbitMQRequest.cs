using System.Text;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using EBCEYS.RabbitMQ.Server.Service;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data;

/// <summary>
///     A <see cref="BaseRabbitMQRequest" /> class.
/// </summary>
public sealed class BaseRabbitMQRequest
{
    /// <summary>
    ///     Initiates a new instance of the <see cref="BaseRabbitMQRequest" />.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    /// <param name="serializerOptions">The serializer options.</param>
    /// <param name="encoding">The encoding.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseRabbitMQRequest(BasicDeliverEventArgs eventArgs, JsonSerializerSettings? serializerOptions = null,
        Encoding? encoding = default)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        GZipSettings gZipSettings = new(false);
        if (eventArgs.BasicProperties.Headers?.GetHeaderBytes(RabbitMQServer.GZipSettingsResponseHeaderKey)
                ?.FirstOrDefault() == 1)
        {
            gZipSettings = new GZipSettings(true);
            MessageCompression = ReceivedMessageCompression.GZiped;
        }

        var request = GZipSettings.GZipDecompress(eventArgs.Body.ToArray(), gZipSettings);
        var json = (encoding ?? Encoding.UTF8).GetString(request);
        var data = JsonConvert.DeserializeObject<RabbitMQRequestData?>(json, serializerOptions);
        ArgumentNullException.ThrowIfNull(data);
        RequestData = data;
    }

    /// <summary>
    ///     The request data.
    /// </summary>
    public RabbitMQRequestData RequestData { get; }

    /// <summary>
    ///     Indicates the message compression.
    /// </summary>
    public ReceivedMessageCompression MessageCompression { get; } = ReceivedMessageCompression.NoCompression;
}

/// <summary>
///     The <see cref="ReceivedMessageCompression" /> enum.
/// </summary>
public enum ReceivedMessageCompression
{
    /// <summary>
    ///     No compression.
    /// </summary>
    NoCompression,

    /// <summary>
    ///     GZiped.
    /// </summary>
    GZiped
}