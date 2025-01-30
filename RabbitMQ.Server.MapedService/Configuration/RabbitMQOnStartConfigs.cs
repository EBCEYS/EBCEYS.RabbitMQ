namespace EBCEYS.RabbitMQ.Configuration
{
    public class RabbitMQOnStartConfigs
    {
        private byte connectionRetries = 1;
        private TimeSpan delayBeforeRetries = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// Number of connection retries on starting service.<br/>
        /// Min - 1. Max - 100.<br/>
        /// Default is 1.
        /// </summary>
        public byte ConnectionReties
        {
            get { return connectionRetries; }
            set { if (value > 100) connectionRetries = 100; else if (value == 0) connectionRetries = 1;  else connectionRetries = value; }
        }
        /// <summary>
        /// Delay before connection retries on starting service.<br/>
        /// Min - 0.1 sec. Max - <see cref="TimeSpan.MaxValue"/>.<br/>
        /// Default is 1 sec.
        /// </summary>
        public TimeSpan DelayBeforeRetries
        {
            get { return delayBeforeRetries; }
            set { if (value < TimeSpan.FromSeconds(0.1)) delayBeforeRetries = TimeSpan.FromSeconds(1.0); else delayBeforeRetries = value; }
        }
        /// <summary>
        /// Set <c>true</c> if you want to client throws server exceptions as <see cref="Server.MappedService.Exceptions.RabbitMQRequestProcessingException"/> on receiving response.<br/>
        /// Default is <c>false</c>. <br/>
        /// Works only for <see cref="Client.RabbitMQClient"/>.
        /// </summary>
        public bool ThrowServerExceptionsOnReceivingResponse { get; set; } = false;
    }
}