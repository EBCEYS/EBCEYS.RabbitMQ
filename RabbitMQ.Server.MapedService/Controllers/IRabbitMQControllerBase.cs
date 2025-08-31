using System.Reflection;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Controllers;

/// <summary>
///     A <see cref="IRabbitMQControllerBase" /> interface.
/// </summary>
[Obsolete($"It's better to use {nameof(RabbitMQSmartControllerBase)}. Will be removed in future versions.")]
public interface IRabbitMQControllerBase
{
    /// <summary>
    ///     The rabbitmq methods.
    /// </summary>
    IEnumerable<MethodInfo>? RabbitMQMethods { get; }

    /// <summary>
    ///     Finds a method.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <returns>A rabbitmq method as <see cref="MethodInfo" /> if exists; otherwise <c>null</c>.</returns>
    MethodInfo? FindMethod(string? methodName);

    /// <summary>
    ///     Gets the method to execute.
    /// </summary>
    /// <param name="eventArgs">The received message.</param>
    /// <param name="serializerOptions">The serrializer options.</param>
    /// <returns>A rabbitmq method as <see cref="MethodInfo" /> if exists; otherwise <c>null</c>.</returns>
    MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs, JsonSerializerSettings? serializerOptions = null);

    /// <summary>
    ///     Process the request.
    /// </summary>
    /// <param name="method">The rabbitmq method.</param>
    /// <returns></returns>
    Task ProcessRequestAsync(MethodInfo method);

    /// <summary>
    ///     Process request with response.
    /// </summary>
    /// <param name="method">The rabbitmq method.</param>
    /// <returns>Response object.</returns>
    Task<object?> ProcessRequestWithResponseAsync(MethodInfo method);
}