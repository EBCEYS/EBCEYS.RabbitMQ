using RabbitMQ.Client.Events;
using RabbitMQ.Server.MapedService.Attributes;
using RabbitMQ.Server.MapedService.Data;
using RabbitMQ.Server.MapedService.Exceptions;
using System.Reflection;
using System.Text.Json;

namespace RabbitMQ.Server.MapedService.Controllers
{
    /// <summary>
    /// Base controller class. <br/>
    /// Method should be asynchronous only!
    /// </summary>
    public class RabbitMQControllerBase : IDisposable
    {
        private BaseRabbitMQRequest? request;
        public IEnumerable<MethodInfo>? RabbitMQMethods { get; private set; }
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
            return methods.Where(m => (m.GetCustomAttribute(typeof(RabbitMQMethodName)) as RabbitMQMethodName) != null);
        }

        public MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }
            request = new(eventArgs);

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
            MethodInfo? method = RabbitMQMethods!.FirstOrDefault(m => (m.GetCustomAttribute(typeof(RabbitMQMethodName)) as RabbitMQMethodName)?.Name == methodName);
            return method;
        }

        public async Task<object?> ProcessRequestWithResponseAsync(MethodInfo method)
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

        public async Task ProcessRequestAsync(MethodInfo method)
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
                object argument = JsonSerializer.Deserialize(request!.RequestData!.Params[i]!.ToString()!, methodArgs[i].ParameterType)!;
                arguments.Add(argument);
            }

            return arguments.ToArray();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
