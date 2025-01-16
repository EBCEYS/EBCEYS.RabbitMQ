using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    public static class StringExtensions
    {
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
