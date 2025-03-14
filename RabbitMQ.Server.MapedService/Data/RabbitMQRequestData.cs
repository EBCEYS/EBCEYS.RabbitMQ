﻿using System.IO.Compression;
using System.Text.Json.Serialization;

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
        [Newtonsoft.Json.JsonIgnore()]
        [System.Text.Json.Serialization.JsonIgnore()]
        public GZipSettings? GZip { get; set; } = null;
    }
    /// <summary>
    /// The gzip settings.
    /// </summary>
    /// <remarks>
    /// Initiates a new instance of the <see cref="GZipSettings"/>/
    /// </remarks>
    /// <param name="gZiped">The gziped.</param>
    /// <param name="compLevel">The compression level.</param>
    public struct GZipSettings(bool gZiped = false, CompressionLevel compLevel = CompressionLevel.Optimal)
    {
        /// <summary>
        /// Is gziped.
        /// </summary>
        public bool GZiped { get; set; } = gZiped;
        /// <summary>
        /// The compression level.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = compLevel;

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
            if (settings is null || !settings.Value.GZiped)
            {
                return input;
            }
            using MemoryStream result = new();
            byte[] lengthBytes = BitConverter.GetBytes(input.Length);
            result.Write(lengthBytes, 0, 4);
            using (GZipStream compressionStream = new(result,
                settings.Value.CompressionLevel))
            {
                compressionStream.Write(input, 0, input.Length);
                compressionStream.Flush();
            }
            return result.ToArray();
        }
        /// <summary>
        /// Decompress the input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="settings">The gzip settings.</param>
        /// <returns>Decompresed message if <see cref="GZiped"/> set <c>true</c>; otherwise <paramref name="input"/>.</returns>
        public static byte[] GZipDecompress(byte[] input, GZipSettings? settings)
        {
            if (settings is null || !settings.Value.GZiped)
            {
                return input;
            }
            using MemoryStream source = new(input);
            byte[] lengthBytes = new byte[4];
            source.Read(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            using GZipStream decompressionStream = new(source,
                CompressionMode.Decompress);
            byte[] result = new byte[length];
            decompressionStream.Read(result, 0, length);
            return result;
        }
    }
}
