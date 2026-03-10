using System.Reflection;
using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.Middlewares;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
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
    private ILogger<RabbitMQSmartControllerBase> _logger = null!;
    private JsonSerializerSettings? _serializerSettings;
    private RabbitMQServer? _server;
    private IServiceProvider _serviceProvider = null!;

    /// <summary>
    ///     Initiates a new instance of the <see cref="RabbitMQSmartControllerBase" />.
    /// </summary>
    public RabbitMQSmartControllerBase()
    {
    }

    /// <summary>
    ///     The requested methods params processing delegates. <br />
    ///     <see cref="CustomParameterProcessingOptions.ParameterSelection" /> uses to select parameter
    ///     that will be placed to <see cref="CustomParameterProcessingOptions.ParameterProcessing" /> as second argument.
    ///     <br />
    ///     <see cref="CustomParameterProcessingOptions.ParameterProcessing" /> arguments: <br />
    ///     0) the parameter index in method; <br />
    ///     1) the called method; <br />
    ///     2) the parameter; <br />
    ///     3) the received message data. <br />
    ///     As the delegate result should be an argument to pass it in method call.
    /// </summary>
    protected virtual IEnumerable<CustomParameterProcessingOptions> CustomParametersProcessing { get; set; } = [];

    private RabbitMqControllerMiddlewaresCollection Middlewares { get; set; } = null!;

    private AsyncLocal<BaseRabbitMQRequest> RequestLocal { get; } = new();

    /// <summary>
    ///     The received request.
    /// </summary>
    public BaseRabbitMQRequest Request => RequestLocal.Value
                                          ?? throw new InvalidOperationException("The incoming request not found!");

    private IEnumerable<MethodInfo> RabbitMQMethods => GetControllerMethods(GetType());

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_server is not null)
        {
            await _server.DisposeAsync();
        }

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
    ///     The middlewares. Controller calls them before starting message processing!
    /// </summary>
    protected virtual void SetMiddlewares(RabbitMqControllerMiddlewaresCollection middlewares)
    {
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
        var emptyController = ActivatorUtilities.CreateInstance<T>(serviceProvider);
        emptyController.SetParams(config, serviceProvider, gzipSettings, serializerSettings);
        return emptyController;
    }

    private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
    {
        var methods = controllerType.GetMethods().Where(m => m.Attributes.HasFlag(MethodAttributes.Public));
        return methods.Where(m => m.GetCustomAttribute(typeof(RabbitMQMethod)) is RabbitMQMethod);
    }

    private void SetParams(RabbitMQConfiguration config, IServiceProvider serviceProvider,
        GZipSettings? gzipSettings = null, JsonSerializerSettings? serializerSettings = null)
    {
        Middlewares = new RabbitMqControllerMiddlewaresCollection(serviceProvider);
        _serviceProvider = serviceProvider;
        _serializerSettings = serializerSettings;
        _logger =
            (ILogger<RabbitMQSmartControllerBase>?)serviceProvider?.GetService(
                typeof(ILogger<RabbitMQSmartControllerBase>)) ??
            NullLoggerFactory.Instance.CreateLogger<RabbitMQSmartControllerBase>();
        _server = new RabbitMQServer(
            (ILogger<RabbitMQServer>?)serviceProvider?.GetService(typeof(ILogger<RabbitMQServer>)) ??
            NullLoggerFactory.Instance.CreateLogger<RabbitMQServer>(), config, ConsumerAction, serializerSettings);
        _encoding = config.Encoding;
        _gzipSettings = gzipSettings;
        SetMiddlewares(Middlewares);
    }

    private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
    {
        using var loggerScope =
            _logger!.BeginScope("CorrelationId: {CorrelationId}", args.BasicProperties.CorrelationId);
        try
        {
            if (Middlewares.Any())
            {
                foreach (var middleware in Middlewares)
                {
                    await middleware.InvokeAsync(args, args.CancellationToken);
                }
            }

            var method = GetMethodToExecute(args);
            if (method is null)
            {
                return;
            }

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
                {
                    await _server!.SendExceptionResponseAsync(args, rabbitEx);
                }
                else
                {
                    await _server!.SendExceptionResponseAsync(args,
                        new RabbitMQRequestProcessingException("Unexpected exception", processingException));
                }
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
        RequestLocal.Value =
            new BaseRabbitMQRequest(eventArgs, _serializerSettings, _encoding, eventArgs.CancellationToken);

        return FindMethod(Request.RequestData.Method);
    }

    private MethodInfo? FindMethod(string? methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
        var method = RabbitMQMethods!.FirstOrDefault(m =>
            (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
        return method;
    }

    private async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
    {
        var methodArgs = method.GetParameters();
        if (Request.RequestData.Params is null || Request.RequestData.Params.Length == 0)
        {
            using var t = method.Invoke(this, null) as Task;
            if (t == null)
            {
                return null;
            }

            await t;
            return ((dynamic)t).Result;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs, method);
            using var t = method.Invoke(this, arguments) as Task;
            if (t == null)
            {
                return null;
            }

            await t;
            return ((dynamic)t).Result;
        }
    }

    private async Task ProcessRequestAsync(MethodInfo method)
    {
        if (Request is null)
        {
            throw new InvalidOperationException(nameof(Request) + " should be set!");
        }

        var methodArgs = method.GetParameters();
        if (Request!.RequestData.Params is null || Request.RequestData.Params.Length == 0)
        {
            if (methodArgs.Length > 0)
            {
                throw new RabbitMQControllerException("Method has arguments but request does not contains it!");
            }

            await (Task)method.Invoke(this, null)!;
        }
        else
        {
            var arguments = GetArgumentsForMethod(methodArgs, method);
            await (Task)method.Invoke(this, arguments)!;
        }
    }

    private object[] GetArgumentsForMethod(ParameterInfo[] methodArgs, MethodInfo method)
    {
        List<object> arguments = [];
        for (var i = 0; i < methodArgs.Length; i++)
        {
            var customProcessor = CustomParametersProcessing
                .FirstOrDefault(x => x.ParameterSelection(methodArgs[i]));
            if (customProcessor is not null)
            {
                var argument = customProcessor.ParameterProcessing(i, method, methodArgs[i], Request.RequestData);
                arguments.Add(argument);
                continue;
            }

            if (methodArgs[i].GetCustomAttributes(typeof(RabbitMqFromKeyedServiceAttribute))
                    .OfType<RabbitMqFromKeyedServiceAttribute>().FirstOrDefault()
                is { } keyServiceAttribute)
            {
                arguments.Add(
                    _serviceProvider.GetRequiredKeyedService(methodArgs[i].ParameterType, keyServiceAttribute.Key));
                continue;
            }

            if (methodArgs[i].GetCustomAttributes(typeof(RabbitMqFromServiceAttribute)).Any())
            {
                arguments.Add(_serviceProvider.GetRequiredService(methodArgs[i].ParameterType));
                continue;
            }

            if (methodArgs[i].ParameterType == typeof(CancellationToken))
            {
                arguments.Add(Request.CancellationToken);
                continue;
            }

            if (Request.RequestData.Params is null || i >= Request.RequestData.Params.Length)
            {
                continue;
            }

            if (Request.RequestData.Params[i] is JObject)
            {
                Request.RequestData.Params[i] = JsonConvert.DeserializeObject(Request.RequestData.Params[i].ToString()!,
                    methodArgs[i].ParameterType)!;
            }

            if (Request.RequestData.Params[i] is JArray)
            {
                Request.RequestData.Params[i] =
                    JArray.Parse(Request.RequestData.Params[i].ToString()!)!.ToObject(methodArgs[i].ParameterType)!;
            }

            arguments.Add(Request.RequestData.Params[i]);
        }

        if (methodArgs.Length != arguments.Count)
        {
            throw new RabbitMQControllerException("Request and method arguments are not equal!");
        }

        return arguments.ToArray();
    }
}