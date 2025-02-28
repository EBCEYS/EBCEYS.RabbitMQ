using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    /// <summary>
    /// A <see cref="StringExtensions"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Concatinates strings.
        /// </summary>
        /// <param name="strings">The strings array.</param>
        /// <returns>Concatinated string.</returns>
        public static string ConcatStrings(params string[] strings)
        {
            StringBuilder sb = new();
            foreach (string str in strings)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }
    }
}
