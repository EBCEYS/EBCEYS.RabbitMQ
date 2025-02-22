using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    /// <summary>
    /// A <see cref="ExchangeTypeExtensions"/> class.
    /// </summary>
    public static class ExchangeTypeExtensions
    {
        /// <summary>
        /// Gets <see cref="string"/> representation of <see cref="ExchangeTypes"/> value.
        /// </summary>
        /// <param name="type">The supported exchange type.</param>
        /// <returns>A <see cref="string"/> representation of <see cref="ExchangeTypes"/>.</returns>
        public static string ParseFromEnum(ExchangeTypes type)
        {
            return type switch
            {
                ExchangeTypes.Fanout => ExchangeType.Fanout,
                ExchangeTypes.Direct => ExchangeType.Direct,
                ExchangeTypes.Topic => ExchangeType.Topic,
                _ => ExchangeType.Direct,
            };
        }
        /// <summary>
        /// Gets <see cref="ExchangeTypes"/> from <see cref="string"/>.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <returns>A <see cref="ExchangeTypes"/> parsed from <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static ExchangeTypes GetExchangeType(this string str, bool ignoreCase = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(str));

            return Enum.Parse<ExchangeTypes>(str, ignoreCase);
        }
    }
}
