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
    public class RabbitMQSmartControllerBase : IHostedService
    {
        public BaseRabbitMQRequest? request;
        private RabbitMQServer? server;
        private IServiceProvider? serviceProvider;
        private JsonSerializerSettings? serializerSettings;
        private ILogger<RabbitMQSmartControllerBase>? logger;

        private IEnumerable<MethodInfo>? RabbitMQMethods => GetControllerMethods(this.GetType());

        public static T InitializeNewController<T>(RabbitMQConfiguration config, IServiceProvider serviceProvider, JsonSerializerSettings? serializerSettings = null) where T : RabbitMQSmartControllerBase
        {
            ConstructorInfo constructor = typeof(T).GetConstructors().FirstOrDefault() ?? throw new Exception($"Service {typeof(T).Name} has no constructor!");
            ParameterInfo[] parameters = constructor.GetParameters();
            List<object> inputParameters = new();
            inputParameters.AddRange(parameters.Select(p => serviceProvider!.GetService(p.ParameterType)!));
            T controller = (T)Activator.CreateInstance(typeof(T), inputParameters.Any() ? inputParameters.ToArray() : null)!;
            controller.SetParams(config, serviceProvider, serializerSettings);
            return controller;
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
            catch (Exception ex)
            {
                logger!.LogError(ex, "Error on processing message!: {@msg}", args);
            }
            server!.AckMessage(args);
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
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }
            request = new(eventArgs, serializerSettings);

            return FindMethod(request.RequestData.Method);
        }

        private MethodInfo? FindMethod(string? methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException($"\"{nameof(methodName)}\" не может быть пустым или содержать только пробел.", nameof(methodName));
            }
            MethodInfo? method = RabbitMQMethods!.FirstOrDefault(m => (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
            return method;
        }

        private async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
        {
            if (request is null)
            {
                throw new InvalidOperationException(nameof(request) + " should be set!");
            }
            ParameterInfo[] methodArgs = method.GetParameters();
            if (request!.RequestData.Params is null || !request.RequestData.Params.Any())
            {
                if (methodArgs.Length > 0)
                {
                    throw new Exception("Method has no arguments but request contains it!");
                }
                Task t = (Task)method.Invoke(this, null)!;
                await t.ConfigureAwait(false);
                return ((dynamic)t).Result;
            }
            else
            {
                object[] arguments = GetArgumentsForMethod(methodArgs);
                Task t = (Task)method.Invoke(this, arguments)!;
                await t.ConfigureAwait(false);
                return ((dynamic)t).Result;
            }
        }

        private async Task ProcessRequestAsync(MethodInfo method)
        {
            if (request is null)
            {
                throw new InvalidOperationException(nameof(request) + " should be set!");
            }
            ParameterInfo[] methodArgs = method.GetParameters();
            if (request!.RequestData.Params is null || !request.RequestData.Params.Any())
            {
                if (methodArgs.Length > 0)
                {
                    throw new Exception("Method has no arguments but request contains it!");
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
            if (methodArgs.Length != request!.RequestData.Params!.Length)
            {
                throw new RabbitMQControllerException("Request and method arguments are not equeal!");
            }
            List<object> arguments = new();
            for (int i = 0; i < methodArgs.Length; i++)
            {
                if (request.RequestData.Params[i] is JObject)
                {
                    request.RequestData.Params[i] = JsonConvert.DeserializeObject(request.RequestData.Params[i].ToString()!, methodArgs[i].ParameterType)!;
                }
                if (request.RequestData.Params[i] is JArray)
                {
                    request.RequestData.Params[i] = JArray.Parse(request.RequestData.Params[i].ToString()!)!.ToObject(methodArgs[i].ParameterType)!;
                }
                arguments.Add(request.RequestData.Params[i]);
            }

            return arguments.ToArray();
        }
    }
}
