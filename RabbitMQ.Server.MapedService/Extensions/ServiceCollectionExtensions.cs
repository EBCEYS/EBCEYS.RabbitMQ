using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQMappedServer(this IServiceCollection services, RabbitMQConfiguration config, JsonSerializerOptions? serializerOptions = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.AddSingleton<RabbitMQMappedServer>(sr =>
            {
                return new RabbitMQMappedServer(sr.GetService<ILogger<RabbitMQMappedServer>>()!, config, sr, sr.GetService<IEnumerable<RabbitMQControllerBase>>()!, serializerOptions);
            });
            return services.AddHostedService<RabbitMQMappedServer>(sr =>
            {
                return sr.GetService<RabbitMQMappedServer>()!;
            });
        }
        public static IServiceCollection AddRabbitMQServer(this IServiceCollection services, RabbitMQConfiguration config, AsyncEventHandler<BasicDeliverEventArgs> receiverAction, JsonSerializerOptions? serializerOptions = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (receiverAction is null)
            {
                throw new ArgumentNullException(nameof(receiverAction));
            }

            services.AddSingleton<RabbitMQServer>(sr =>
            {
                return new RabbitMQServer(sr.GetService<ILogger<RabbitMQServer>>()!, config, receiverAction, serializerOptions);
            });
            return services.AddHostedService<RabbitMQServer>(sr =>
            {
                return sr.GetService<RabbitMQServer>()!;
            });
        }
        public static IServiceCollection AddRabbitMQControllers(this IServiceCollection services, IEnumerable<RabbitMQControllerBase> controllers)
        {
            if (controllers is null)
            {
                throw new ArgumentNullException(nameof(controllers));
            }
            services.AddSingleton<IEnumerable<RabbitMQControllerBase>>(sp =>
            {
                return controllers;
            });
            return services;
        }
        public static IServiceCollection AddRabbitMQController<T>(this IServiceCollection services) where T: RabbitMQControllerBase
        {
            return services.AddSingleton<IRabbitMQControllerBase, T>();
        }
        public static IServiceCollection FixRabbitMQControllers(this IServiceCollection services)
        {
            return services.AddSingleton<IEnumerable<IRabbitMQControllerBase>>(sr =>
            {
                return sr.GetServices<IRabbitMQControllerBase>()!;
            });
        }
    }
}
