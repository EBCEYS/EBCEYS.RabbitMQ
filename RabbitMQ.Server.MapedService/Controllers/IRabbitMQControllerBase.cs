﻿using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text.Json;

namespace EBCEYS.RabbitMQ.Server.MappedService.Controllers
{
    public interface IRabbitMQControllerBase
    {
        IEnumerable<MethodInfo>? RabbitMQMethods { get; }

        MethodInfo? FindMethod(string? methodName);
        MethodInfo? GetMethodToExecute(BasicDeliverEventArgs eventArgs, JsonSerializerOptions? serializerOptions = null);
        Task ProcessRequestAsync(MethodInfo method);
        Task<object?> ProcessRequestWithResponseAsync(MethodInfo method);
    }
}