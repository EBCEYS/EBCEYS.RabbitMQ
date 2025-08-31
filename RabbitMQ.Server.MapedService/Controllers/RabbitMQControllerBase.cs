using System.Reflection;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Controllers;

/// <summary>
///     A <see cref="RabbitMQControllerBase" /> class.
/// </summary>
[Obsolete($"It's better to use {nameof(RabbitMQSmartControllerBase)}")]
public abstract class RabbitMQControllerBase : IDisposable, IRabbitMQControllerBase
{
    private BaseRabbitMQRequest? _request;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQControllerBase" />.
    /// </summary>
    public RabbitMQControllerBase()
    {
        SetControllerMethods();
    }

    private JsonSerializerSettings? SerializerOptions { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IEnumerable<MethodInfo>? RabbitMQMethods { get; private set; }

    /// <inheritdoc />
    public MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs,
        JsonSerializerSettings? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        SerializerOptions = serializerOptions;
        _request = new BaseRabbitMQRequest(eventArgs, SerializerOptions);

        if (RabbitMQMethods is null) SetControllerMethods();

        return FindMethod(_request.RequestData.Method);
    }

    /// <inheritdoc />
    public MethodInfo? FindMethod(string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
            throw new ArgumentException($"\"{nameof(methodName)}\" не может быть пустым или содержать только пробел.",
                nameof(methodName));
        var method = RabbitMQMethods!.FirstOrDefault(m =>
            (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
        return method;
    }

    /// <inheritdoc />
    public async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
    {
        if (_request is null) throw new InvalidOperationException(nameof(_request) + " should be set!");
        var methodArgs = method.GetParameters();
        if (_request!.RequestData.Params is null || _request.RequestData.Params.Length == 0)
        {
            if (methodArgs.Length > 0) throw new Exception("Method has no arguments but request contains it!");
            var t = (Task)method.Invoke(this, null)!;
            await t.ConfigureAwait(false);
            return ((dynamic)t).Result;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs);
            var t = (Task)method.Invoke(this, arguments)!;
            await t.ConfigureAwait(false);
            return ((dynamic)t).Result;
        }
    }

    /// <inheritdoc />
    public async Task ProcessRequestAsync(MethodInfo method)
    {
        if (_request is null) throw new InvalidOperationException(nameof(_request) + " should be set!");
        var methodArgs = method.GetParameters();
        if (_request!.RequestData.Params is null || _request.RequestData.Params.Length == 0)
        {
            if (methodArgs.Length > 0) throw new Exception("Method has no arguments but request contains it!");
            await (Task)method.Invoke(this, null)!;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs);
            await (Task)method.Invoke(this, arguments)!;
        }
    }

    private void SetControllerMethods()
    {
        var controllerType = GetType();
        RabbitMQMethods = GetControllerMethods(controllerType);
    }

    private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
    {
        var methods = controllerType.GetMethods().Where(m => m.Attributes.HasFlag(MethodAttributes.Public));
        return methods.Where(m => m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod != null);
    }

    private object[] GetArgumentsForMethod(ParameterInfo[] methodArgs)
    {
        if (methodArgs.Length != _request!.RequestData.Params!.Length)
            throw new RabbitMQControllerException("Request and method arguments are not equeal!");
        List<object> arguments = [];
        for (var i = 0; i < methodArgs.Length; i++)
        {
            if (_request.RequestData.Params[i] is JObject)
                _request.RequestData.Params[i] = JsonConvert.DeserializeObject(_request.RequestData.Params[i].ToString()!,
                    methodArgs[i].ParameterType)!;
            if (_request.RequestData.Params[i] is JArray)
                _request.RequestData.Params[i] =
                    JArray.Parse(_request.RequestData.Params[i].ToString()!)!.ToObject(methodArgs[i].ParameterType)!;
            arguments.Add(_request.RequestData.Params[i]);
        }

        return [.. arguments];
    }
}