using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        [Obsolete("It's better to use RabbitMQSmartController")]
        public static IServiceCollection AddRabbitMQMappedServer(this IServiceCollection services, RabbitMQConfiguration config, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(config);

            services.AddSingleton<RabbitMQMappedServer>(sr =>
            {
                return new RabbitMQMappedServer(sr.GetService<ILogger<RabbitMQMappedServer>>()!, config, sr, serializerOptions);
            });
            return services.AddHostedService<RabbitMQMappedServer>(sr =>
            {
                return sr.GetService<RabbitMQMappedServer>()!;
            });
        }
        public static IServiceCollection AddRabbitMQServer(this IServiceCollection services, RabbitMQConfiguration config, AsyncEventHandler<BasicDeliverEventArgs> receiverAction, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(config);

            ArgumentNullException.ThrowIfNull(receiverAction);

            services.AddSingleton<RabbitMQServer>(sr =>
            {
                return new RabbitMQServer(sr.GetService<ILogger<RabbitMQServer>>()!, config, receiverAction, serializerOptions);
            });
            return services.AddHostedService<RabbitMQServer>(sr =>
            {
                return sr.GetService<RabbitMQServer>()!;
            });
        }

        public static IServiceCollection AddRabbitMQClient(this IServiceCollection services, RabbitMQConfiguration config, TimeSpan? requestTimeout = null, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(config);

            services.AddSingleton<RabbitMQClient>(sr =>
            {
                return new RabbitMQClient(sr.GetService<ILogger<RabbitMQClient>>()!, config, requestTimeout, serializerOptions);
            });
            return services.AddHostedService<RabbitMQClient>(sr =>
            {
                return sr.GetService<RabbitMQClient>()!;
            });
        }

        [Obsolete("It's better to use RabbitMQSmartController")]
        public static IServiceCollection AddRabbitMQControllers(this IServiceCollection services, IEnumerable<RabbitMQControllerBase> controllers)
        {
            ArgumentNullException.ThrowIfNull(controllers);
            controllers.ToList().ForEach(l =>
            {
                services.AddScoped<RabbitMQControllerBase>(sp =>
                {
                    return l;
                });
            });
            return services;
        }

        [Obsolete("It's better to use RabbitMQSmartController")]
        public static IServiceCollection AddRabbitMQController<T>(this IServiceCollection services) where T: RabbitMQControllerBase
        {
            return services.AddScoped<RabbitMQControllerBase, T>();
        }

        public static IServiceCollection AddSmartRabbitMQController<T>(this IServiceCollection services, RabbitMQConfiguration configuration, JsonSerializerSettings? serializerOptions = null) where T : RabbitMQSmartControllerBase
        {
            return services.AddHostedService(sr =>
            {
                return RabbitMQSmartControllerBase.InitializeNewController<T>(configuration, sr, serializerOptions);
            });
        }


    }
}
