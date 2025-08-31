using EBCEYS.RabbitMQ.Server.MappedService.Data;
using Microsoft.Extensions.Hosting;

#pragma warning disable IDE0130 // Пространство имен (namespace) не соответствует структуре папок.
namespace EBCEYS.RabbitMQ.Client;
#pragma warning restore IDE0130 // Пространство имен (namespace) не соответствует структуре папок.

/// <summary>
///     A <see cref="IRabbitMQClient" /> interface.
/// </summary>
public interface IRabbitMQClient : IHostedService, IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     Sends a message without response.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    Task SendMessageAsync(RabbitMQRequestData data, bool mandatory = false, CancellationToken token = default);

    /// <summary>
    ///     Sends request with response.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="data">The data to send.</param>
    /// <param name="mandatory">The mandatory.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns><typeparamref name="T" /> object or <c>null</c> if there was no response.</returns>
    Task<T?> SendRequestAsync<T>(RabbitMQRequestData data, bool mandatory = false, CancellationToken token = default);
}