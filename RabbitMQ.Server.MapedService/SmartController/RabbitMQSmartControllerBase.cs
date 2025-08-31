using System.Reflection;
using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.SmartController;

/// <summary>
///     A <see cref="RabbitMQSmartControllerBase" /> class.
/// </summary>
public abstract class RabbitMQSmartControllerBase : IHostedService, IDisposable, IAsyncDisposable
{
    private Encoding _encoding = Encoding.UTF8;
    private GZipSettings? _gzipSettings;
    private ILogger<RabbitMQSmartControllerBase>? _logger;
    private JsonSerializerSettings? _serializerSettings;
    private RabbitMQServer? _server;
    private IServiceProvider? _serviceProvider;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQSmartControllerBase" />.
    /// </summary>
    public RabbitMQSmartControllerBase()
    {
    }

    /// <summary>
    ///     The received request.
    /// </summary>
    public BaseRabbitMQRequest? Request { get; private set; }

    private IEnumerable<MethodInfo>? RabbitMQMethods => GetControllerMethods(GetType());

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_server is not null) await _server.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _server?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _server!.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _server!.StopAsync(cancellationToken);
    }

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQSmartControllerBase" />.
    /// </summary>
    /// <typeparam name="T">The <see cref="RabbitMQSmartControllerBase" /> generic.</typeparam>
    /// <param name="config">The rabbitmq configuration.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="gzipSettings">The gzip settings.</param>
    /// <param name="serializerSettings">The serializer settings.</param>
    /// <returns>A new instance of the <see cref="RabbitMQSmartControllerBase" />.</returns>
    public static T InitializeNewController<T>(RabbitMQConfiguration config, IServiceProvider serviceProvider,
        GZipSettings? gzipSettings = null, JsonSerializerSettings? serializerSettings = null)
        where T : RabbitMQSmartControllerBase
    {
        foreach (var constructor in typeof(T).GetConstructors())
            try
            {
                var parameters = constructor.GetParameters();
                var inputParameters = parameters.Select(p => serviceProvider!.GetService(p.ParameterType));
                var controller =
                    (T)Activator.CreateInstance(typeof(T), inputParameters.Any() ? inputParameters.ToArray() : null)!;
                controller.SetParams(config, serviceProvider, gzipSettings, serializerSettings);
                return controller;
            }
            catch (Exception)
            {
                // ignored
            }

        var emptyController = Activator.CreateInstance<T>();
        emptyController.SetParams(config, serviceProvider, gzipSettings, serializerSettings);
        return emptyController;
    }

    private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
    {
        var methods = controllerType.GetMethods().Where(m => m.Attributes.HasFlag(MethodAttributes.Public));
        return methods.Where(m => m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod != null);
    }

    private void SetParams(RabbitMQConfiguration config, IServiceProvider? serviceProvider,
        GZipSettings? gzipSettings = null, JsonSerializerSettings? serializerSettings = null)
    {
        this._serviceProvider = serviceProvider;
        this._serializerSettings = serializerSettings;
        _logger =
            (ILogger<RabbitMQSmartControllerBase>?)serviceProvider?.GetService(
                typeof(ILogger<RabbitMQSmartControllerBase>)) ??
            NullLoggerFactory.Instance.CreateLogger<RabbitMQSmartControllerBase>();
        _server = new RabbitMQServer(
            (ILogger<RabbitMQServer>?)serviceProvider?.GetService(typeof(ILogger<RabbitMQServer>)) ??
            NullLoggerFactory.Instance.CreateLogger<RabbitMQServer>(), config, ConsumerAction, serializerSettings);
        _encoding = config.Encoding;
        this._gzipSettings = gzipSettings;
    }

    private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var method = GetMethodToExecute(args);
            if (method is null) return;
            var returnParam = method.ReturnParameter;
            if (returnParam.ParameterType == typeof(Task) || returnParam.ParameterType == typeof(void))
            {
                await ProcessRequestAsync(method);
                return;
            }

            var result = await ProcessRequestWithResponseAsync(method);
            if (result is not null)
            {
                await _server!.SendResponseAsync(args, result, _gzipSettings);
                return;
            }
        }
        catch (TargetInvocationException processingException)
        {
            try
            {
                if (processingException.InnerException is RabbitMQRequestProcessingException rabbitEx)
                    await _server!.SendExceptionResponseAsync(args, rabbitEx);
                else
                    await _server!.SendExceptionResponseAsync(args,
                        new RabbitMQRequestProcessingException("Unexpected exception", processingException));
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "Error on sending error response!: {@msg}", args);
            }
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "Error on processing message!: {@msg}", args);
            try
            {
                await _server!.SendExceptionResponseAsync(args,
                    new RabbitMQRequestProcessingException("Unexpected exception", ex));
            }
            catch (Exception responseEx)
            {
                _logger!.LogError(responseEx, "Error on sending error response!: {@msg}", args);
            }
        }

        await _server!.AckMessage(args);
    }

    private MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        Request = new BaseRabbitMQRequest(eventArgs, _serializerSettings, _encoding);

        return FindMethod(Request.RequestData.Method);
    }

    private MethodInfo? FindMethod(string? methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));
        var method = RabbitMQMethods!.FirstOrDefault(m =>
            (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
        return method;
    }

    private async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
    {
        if (Request is null) throw new InvalidOperationException(nameof(Request) + " should be set!");
        var methodArgs = method.GetParameters();
        if (Request!.RequestData.Params is null || Request.RequestData.Params.Length == 0)
        {
            using var t = method.Invoke(this, null) as Task;
            if (t == null) return null;
            await t;
            return ((dynamic)t).Result;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs);
            using var t = method.Invoke(this, arguments) as Task;
            if (t == null) return null;
            await t;
            return ((dynamic)t).Result;
        }
    }

    private async Task ProcessRequestAsync(MethodInfo method)
    {
        if (Request is null) throw new InvalidOperationException(nameof(Request) + " should be set!");
        var methodArgs = method.GetParameters();
        if (Request!.RequestData.Params is null || Request.RequestData.Params.Length == 0)
        {
            if (methodArgs.Length > 0)
                throw new RabbitMQControllerException("Method has arguments but request does not contains it!");
            await (Task)method.Invoke(this, null)!;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs);
            await (Task)method.Invoke(this, arguments)!;
        }
    }

    private object[] GetArgumentsForMethod(ParameterInfo[] methodArgs)
    {
        if (methodArgs.Length != Request!.RequestData.Params!.Length)
            throw new RabbitMQControllerException("Request and method arguments are not equal!");
        List<object> arguments = [];
        for (var i = 0; i < methodArgs.Length; i++)
        {
            if (Request.RequestData.Params[i] is JObject)
                Request.RequestData.Params[i] = JsonConvert.DeserializeObject(Request.RequestData.Params[i].ToString()!,
                    methodArgs[i].ParameterType)!;
            if (Request.RequestData.Params[i] is JArray)
                Request.RequestData.Params[i] =
                    JArray.Parse(Request.RequestData.Params[i].ToString()!)!.ToObject(methodArgs[i].ParameterType)!;
            arguments.Add(Request.RequestData.Params[i]);
        }

        return [.. arguments];
    }
}