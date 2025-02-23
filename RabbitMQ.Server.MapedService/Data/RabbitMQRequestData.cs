using System.IO.Compression;

namespace EBCEYS.RabbitMQ.Server.MappedService.Data
{
    /// <summary>
    /// A <see cref="RabbitMQRequestData"/> class.
    /// </summary>
    public class RabbitMQRequestData
    {
        /// <summary>
        /// The params.
        /// </summary>
        public object[]? Params { get; set; }
        /// <summary>
        /// The method to execute name.
        /// </summary>
        public string? Method { get; set; }
        /// <summary>
        /// Is gziped message.
        /// </summary>
        public GZipSettings? GZip { get; set; } = null;
    }
    /// <summary>
    /// The gzip settings.
    /// </summary>
    public class GZipSettings
    {
        /// <summary>
        /// Is gziped.
        /// </summary>
        public bool GZiped { get; set; } = false;
        /// <summary>
        /// The compression level.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
        /// <summary>
        /// The default realization of <see cref="GZipSettings"/>.
        /// </summary>
        public static GZipSettings Default => new();
        /// <summary>
        /// Compress the input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="settings">The gzip settings.</param>
        /// <returns>Compresed message if <see cref="GZiped"/> set <c>true</c>; otherwise <paramref name="input"/>.</returns>
        public static byte[] GZipCompress(byte[] input, GZipSettings? settings)
        {
            if (settings is null || !settings.GZiped)
            {
                return input;
            }
            using MemoryStream stream = new();
            using (GZipStream gzip = new(stream, settings.CompressionLevel))
            {
                gzip.Write(input, 0, input.Length);
            }
            return stream.ToArray();
        }
        /// <summary>
        /// Decompress the input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="settings">The gzip settings.</param>
        /// <returns>Decompresed message if <see cref="GZiped"/> set <c>true</c>; otherwise <paramref name="input"/>.</returns>
        public static byte[] GZipDecompress(byte[] input, GZipSettings? settings)
        {
            if (settings is null || !settings.GZiped)
            {
                return input;
            }
            using MemoryStream gzipDeCompressedMemStream = new();
            using MemoryStream gzipCompressedMemStream = new(input);
            using (GZipStream gzipStream = new(gzipCompressedMemStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(gzipDeCompressedMemStream);
            }
            gzipCompressedMemStream.Close();
            return gzipCompressedMemStream.ToArray();
        }
    }
}
