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
using System.Reflection;

namespace EBCEYS.RabbitMQ.Server.MappedService.SmartController
{
    public abstract class RabbitMQSmartControllerBase : IHostedService
    {
        public BaseRabbitMQRequest? Request { get; private set; }
        private RabbitMQServer? server;
        private IServiceProvider? serviceProvider;
        private JsonSerializerSettings? serializerSettings;
        private ILogger<RabbitMQSmartControllerBase>? logger;

        private IEnumerable<MethodInfo>? RabbitMQMethods => GetControllerMethods(this.GetType());

        public static T InitializeNewController<T>(RabbitMQConfiguration config, IServiceProvider serviceProvider, JsonSerializerSettings? serializerSettings = null) where T : RabbitMQSmartControllerBase
        {
            foreach (ConstructorInfo constructor in typeof(T).GetConstructors())
            {
                try
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    IEnumerable<object?> inputParameters = parameters.Select(p => serviceProvider!.GetService(p.ParameterType));
                    T controller = (T)Activator.CreateInstance(typeof(T), inputParameters.Any() ? inputParameters.ToArray() : null)!;
                    controller.SetParams(config, serviceProvider, serializerSettings);
                    return controller;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            T emptyController = (T)Activator.CreateInstance(typeof(T))!;
            emptyController.SetParams(config, serviceProvider, serializerSettings);
            return emptyController;
        }

        private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
        {
            IEnumerable<MethodInfo> methods = controllerType.GetMethods().Where(m => m.Attributes.HasFlag(MethodAttributes.Public));
            return methods.Where(m => (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod) != null);
        }

        private void SetParams(RabbitMQConfiguration config, IServiceProvider? serviceProvider, JsonSerializerSettings? serializerSettings = null)
        {
            this.serviceProvider = serviceProvider;
            this.serializerSettings = serializerSettings;
            this.logger = (ILogger<RabbitMQSmartControllerBase>?)serviceProvider?.GetService(typeof(ILogger<RabbitMQSmartControllerBase>)) ?? NullLoggerFactory.Instance.CreateLogger<RabbitMQSmartControllerBase>();
            this.server = new((ILogger<RabbitMQServer>?)serviceProvider?.GetService(typeof(ILogger<RabbitMQServer>)) ?? NullLoggerFactory.Instance.CreateLogger<RabbitMQServer>(), config, ConsumerAction, serializerSettings);
        }

        public RabbitMQSmartControllerBase()
        {

        }

        private async Task ConsumerAction(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                MethodInfo? method = GetMethodToExecute(args);
                if (method is null)
                {
                    return;
                }
                ParameterInfo returnParam = method.ReturnParameter;
                if (returnParam.ParameterType == typeof(Task) || returnParam.ParameterType == typeof(void))
                {
                    await ProcessRequestAsync(method);
                    return;
                }

                object? result = await ProcessRequestWithResponseAsync(method);
                if (result is not null)
                {
                    await server!.SendResponseAsync(args, result);
                    return;
                }

            }
            catch (TargetInvocationException processingException)
            {
                try
                {
                    if (processingException.InnerException is RabbitMQRequestProcessingException rabbitEx)
                    {
                        await server!.SendExceptionResponseAsync(args, rabbitEx);
                    }
                    else
                    {
                        await server!.SendExceptionResponseAsync(args, new("Unexpected exception", processingException));
                    }
                }
                catch (Exception ex)
                {
                    logger!.LogError(ex, "Error on sending error response!: {@msg}", args);
                }
            }
            catch (Exception ex)
            {
                logger!.LogError(ex, "Error on processing message!: {@msg}", args);
                try
                {
                    await server!.SendExceptionResponseAsync(args, new("Unexpected exception", ex));
                }
                catch (Exception responseEx)
                {
                    logger!.LogError(responseEx, "Error on sending error response!: {@msg}", args);
                }
            }
            await server!.AckMessage(args);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await server!.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await server!.StopAsync(cancellationToken);
        }

        private MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs)
        {
            ArgumentNullException.ThrowIfNull(eventArgs);
            Request = new(eventArgs, serializerSettings);

            return FindMethod(Request.RequestData.Method);
        }

        private MethodInfo? FindMethod(string? methodName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));
            MethodInfo? method = RabbitMQMethods!.FirstOrDefault(m => (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
            return method;
        }

        private async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
        {
            if (Request is null)
            {
                throw new InvalidOperationException(nameof(Request) + " should be set!");
            }
            ParameterInfo[] methodArgs = method.GetParameters();
            if (Request!.RequestData.Params is null || Request.RequestData.Params.Length == 0)
            {
                using Task? t = method.Invoke(this, null) as Task;
                if (t == null)
                {
                    return null;
                }
                await t;
                return ((dynamic)t).Result;
            }
            else
            {
                object[] arguments = GetArgumentsForMethod(methodArgs);
                using Task? t = method.Invoke(this, arguments) as Task;
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
            ParameterInfo[] methodArgs = method.GetParameters();
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
                object[] arguments = GetArgumentsForMethod(methodArgs);
                await (Task)method.Invoke(this, arguments)!;
            }
        }

        private object[] GetArgumentsForMethod(ParameterInfo[] methodArgs)
        {
            if (methodArgs.Length != Request!.RequestData.Params!.Length)
            {
                throw new RabbitMQControllerException("Request and method arguments are not equal!");
            }
            List<object> arguments = [];
            for (int i = 0; i < methodArgs.Length; i++)
            {
                if (Request.RequestData.Params[i] is JObject)
                {
                    Request.RequestData.Params[i] = JsonConvert.DeserializeObject(Request.RequestData.Params[i].ToString()!, methodArgs[i].ParameterType)!;
                }
                if (Request.RequestData.Params[i] is JArray)
                {
                    Request.RequestData.Params[i] = JArray.Parse(Request.RequestData.Params[i].ToString()!)!.ToObject(methodArgs[i].ParameterType)!;
                }
                arguments.Add(Request.RequestData.Params[i]);
            }

            return [.. arguments];
        }
    }
}
