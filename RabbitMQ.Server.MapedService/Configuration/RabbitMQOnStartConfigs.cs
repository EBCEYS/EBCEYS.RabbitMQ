namespace EBCEYS.RabbitMQ.Configuration;

/// <summary>
///     A <see cref="RabbitMQOnStartConfigs" /> class.
/// </summary>
public class RabbitMQOnStartConfigs
{
    private byte _connectionRetries = 1;
    private TimeSpan _delayBeforeRetries = TimeSpan.FromSeconds(1.0);

    /// <summary>
    ///     Number of connection retries on starting service.<br />
    ///     Min - 1. Max - 100.<br />
    ///     Default is 1.
    /// </summary>
    public byte ConnectionReties
    {
        get => _connectionRetries;
        set
        {
            if (value > 100) _connectionRetries = 100;
            else if (value == 0) _connectionRetries = 1;
            else _connectionRetries = value;
        }
    }

    /// <summary>
    ///     Delay before connection retries on starting service.<br />
    ///     Min - 0.1 sec. Max - <see cref="TimeSpan.MaxValue" />.<br />
    ///     Default is 1 sec.
    /// </summary>
    public TimeSpan DelayBeforeRetries
    {
        get => _delayBeforeRetries;
        set
        {
            if (value < TimeSpan.FromSeconds(0.1))
                _delayBeforeRetries = TimeSpan.FromSeconds(1.0);
            else
                _delayBeforeRetries = value;
        }
    }

    /// <summary>
    ///     Set <c>true</c> if you want to client throws server exceptions as
    ///     <see cref="Server.MappedService.Exceptions.RabbitMQRequestProcessingException" /> on receiving response.<br />
    ///     Default is <c>false</c>. <br />
    ///     Works only for <see cref="Client.RabbitMQClient" />.
    /// </summary>
    public bool ThrowServerExceptionsOnReceivingResponse { get; set; } = false;
}