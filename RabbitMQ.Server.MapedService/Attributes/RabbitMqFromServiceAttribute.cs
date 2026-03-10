namespace EBCEYS.RabbitMQ.Server.MappedService.Attributes;

/// <summary>
///     Indicates that parameter should be got from service provider. <br />
///     <b>Attributed parameter should be placed at the end of parameters line!</b>
/// </summary>
/// <example>
///     <code>
/// [RabbitMqMethod("MyMethod")]
/// public Task MyMethod([RabbitMqFromService] IMyService service, CancellationToken token)
/// {
///     return service.ExecuteAsync(token);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public class RabbitMqFromServiceAttribute : Attribute;

/// <summary>
///     Indicates that parameter should be got from service provider by key.<br />
///     <b>Attributed parameter should be placed at the end of parameters line!</b>
/// </summary>
/// <example>
///     <code>
/// [RabbitMqMethod("MyMethod")]
/// public Task MyMethod([RabbitMqFromKeyedService("SomeKey")] IMyService service, CancellationToken token)
/// {
///     return service.ExecuteAsync(token);
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public class RabbitMqFromKeyedServiceAttribute(object key) : Attribute
{
    /// <summary>
    ///     The key.
    /// </summary>
    public object Key { get; } = key;
}