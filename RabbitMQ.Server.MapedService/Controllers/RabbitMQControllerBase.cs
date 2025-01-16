using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client.Events;
using System.Reflection;

namespace EBCEYS.RabbitMQ.Server.MappedService.Controllers
{
    /// <summary>
    /// Base controller class. <br/>
    /// Methods should be asynchronous only!
    /// </summary>
    [Obsolete("It's better to use RabbitMQSmartControllerBase")]
    public abstract class RabbitMQControllerBase : IDisposable, IRabbitMQControllerBase
    {
        private BaseRabbitMQRequest? request;
        public IEnumerable<MethodInfo>? RabbitMQMethods { get; private set; }
        private JsonSerializerSettings? SerializerOptions { get; set; }
        public RabbitMQControllerBase()
        {
            SetControllerMethods();
        }

        private void SetControllerMethods()
        {
            Type controllerType = this.GetType();
            RabbitMQMethods = GetControllerMethods(controllerType);
        }

        private static IEnumerable<MethodInfo> GetControllerMethods(Type controllerType)
        {
            IEnumerable<MethodInfo> methods = controllerType.GetMethods().Where(m => m.Attributes.HasFlag(MethodAttributes.Public));
            return methods.Where(m => (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod) != null);
        }

        public MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(eventArgs);
            SerializerOptions = serializerOptions;
            request = new(eventArgs, SerializerOptions);

            if (RabbitMQMethods is null)
            {
                SetControllerMethods();
            }

            return FindMethod(request.RequestData.Method);
        }

        public MethodInfo? FindMethod(string? methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException($"\"{nameof(methodName)}\" не может быть пустым или содержать только пробел.", nameof(methodName));
            }
            MethodInfo? method = RabbitMQMethods!.FirstOrDefault(m => (m.GetCustomAttribute(typeof(RabbitMQMethod)) as RabbitMQMethod)?.Name == methodName);
            return method;
        }

        public async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
        {
            if (request is null)
            {
                throw new InvalidOperationException(nameof(request) + " should be set!");
            }
            ParameterInfo[] methodArgs = method.GetParameters();
            if (request!.RequestData.Params is null || request.RequestData.Params.Length == 0)
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

        public async Task ProcessRequestAsync(MethodInfo method)
        {
            if (request is null)
            {
                throw new InvalidOperationException(nameof(request) + " should be set!");
            }
            ParameterInfo[] methodArgs = method.GetParameters();
            if (request!.RequestData.Params is null || request.RequestData.Params.Length == 0)
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
            List<object> arguments = [];
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

            return [.. arguments];
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
