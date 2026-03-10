using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Middlewares;

/// <summary>
///     The <see cref="IRabbitMqSmartControllerMiddleware" /> interface.
/// </summary>
public interface IRabbitMqSmartControllerMiddleware
{
    /// <summary>
    ///     Invokes the middleware.
    /// </summary>
    /// <param name="arguments">The received arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task InvokeAsync(BasicDeliverEventArgs arguments, CancellationToken cancellationToken);
}

/// <summary>
///     The <see cref="RabbitMqControllerMiddlewaresCollection" /> class.
/// </summary>
/// <param name="serviceProvider"></param>
public sealed class RabbitMqControllerMiddlewaresCollection(IServiceProvider serviceProvider)
    : IEnumerable<IRabbitMqSmartControllerMiddleware>
{
    private readonly List<IRabbitMqSmartControllerMiddleware> _instances = [];

    /// <inheritdoc />
    public IEnumerator<IRabbitMqSmartControllerMiddleware> GetEnumerator()
    {
        return _instances.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Adds the middleware
    /// </summary>
    /// <param name="type">
    ///     The middleware type. <b>Should be a child of <see cref="IRabbitMqSmartControllerMiddleware" />!</b>
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Throw if <paramref name="type" /> is not
    ///     <see cref="IRabbitMqSmartControllerMiddleware" />.
    /// </exception>
    public void Add(Type type)
    {
        if (type.GetInterfaces().Any(x => x == typeof(IRabbitMqSmartControllerMiddleware)))
        {
            var instance =
                ActivatorUtilities.CreateInstance(serviceProvider, type) as IRabbitMqSmartControllerMiddleware;
            if (instance is null)
            {
                throw new InvalidOperationException($"Cannot create instance of {type.FullName}!");
            }

            _instances.Add(instance);
        }

        throw new InvalidOperationException(
            $"{type.FullName} can not be added because it's not {typeof(IRabbitMqSmartControllerMiddleware).FullName}");
    }

    /// <summary>
    ///     Adds the middleware.
    /// </summary>
    /// <typeparam name="T">The middleware type.</typeparam>
    public void Add<T>() where T : IRabbitMqSmartControllerMiddleware
    {
        var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider);
        _instances.Add(instance);
    }
}