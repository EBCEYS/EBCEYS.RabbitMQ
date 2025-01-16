using EBCEYS.RabbitMQ.Configuration;
using RabbitMQ.Client;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    public static class ExchangeTypeExtensions
    {
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
        public static ExchangeTypes GetExchangeType(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException($"\"{nameof(str)}\" не может быть пустым или содержать только пробел.", nameof(str));
            }

            return Enum.Parse<ExchangeTypes>(str, true);
        }
    }
}
