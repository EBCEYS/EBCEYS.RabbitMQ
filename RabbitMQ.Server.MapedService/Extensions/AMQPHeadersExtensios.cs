using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    internal static class AMQPHeadersExtensios
    {
        /// <summary>
        /// Gets the value from AMQP message headers by key.
        /// </summary>
        /// <param name="headers">The AMQP message headers.</param>
        /// <param name="key">The key.</param>
        /// <returns>Returns the <see cref="byte"/> array from AMQP headers by key if exists; otherwise null.</returns>
        internal static byte[]? GetHeaderBytes(this IDictionary<string, object?> headers, string key)
        {
            try
            {
                if (headers.TryGetValue(key, out object? value) && value != null)
                {
                    return (byte[]?)Convert.ChangeType(value, typeof(byte[])) ?? [];
                }
            }
            catch (InvalidCastException)
            {
                return null;
            }
            return null;
        }
        /// <summary>
        /// Gets the value from AMQP message headers by key.
        /// </summary>
        /// <param name="headers">The AMQP message headers.</param>
        /// <param name="key">The key.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>Returns the <see cref="string"/> from AMQP headers decoded by selected <see cref="Encoding"/> if exists; otherwise null.</returns>
        internal static string? GetHeaderString(this IDictionary<string, object?> headers, string key, Encoding encoding)
        {
            byte[]? value = headers.GetHeaderBytes(key);
            if (value is null)
            {
                return null;
            }
            return encoding.GetString(value);
        }
    }
}
