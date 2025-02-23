using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using EBCEYS.RabbitMQ.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace EBCEYS.RabbitMQ.Server.MappedService.Extensions
{
    /// <summary>
    /// A <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string obsoleDesc = $"It's better to use {nameof(RabbitMQSmartControllerBase)}. Method will be removed in future versions.";
        /// <summary>
        /// Adds <see cref="RabbitMQMappedServer"/> to service collection as singleton and hosted service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The rabbitmq configuration.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
        [Obsolete(obsoleDesc)]
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
        /// <summary>
        /// Adds <see cref="RabbitMQServer"/> to service collection as singleton and hosted service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The rabbitmq configuration.</param>
        /// <param name="receiverAction">The received action.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Adds <see cref="RabbitMQClient"/> to service collection as signleton and hosted service.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The rabbitmq configuration.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQClient(this IServiceCollection services, RabbitMQConfiguration config, JsonSerializerSettings? serializerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(config);

            services.AddSingleton<RabbitMQClient>(sr =>
            {
                return new RabbitMQClient(sr.GetService<ILogger<RabbitMQClient>>()!, config, serializerOptions);
            });
            return services.AddHostedService<RabbitMQClient>(sr =>
            {
                return sr.GetService<RabbitMQClient>()!;
            });
        }
        /// <summary>
        /// Adds a collection of the <see cref="RabbitMQControllerBase"/> instances to service collection as scoped.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="controllers">The controllers.</param>
        /// <returns></returns>
        [Obsolete(obsoleDesc)]
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
        /// <summary>
        /// Adds <see cref="RabbitMQControllerBase"/> to service collection as scoped.
        /// </summary>
        /// <typeparam name="T">The <see cref="RabbitMQControllerBase"/> representation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns></returns>
        [Obsolete(obsoleDesc)]
        public static IServiceCollection AddRabbitMQController<T>(this IServiceCollection services) where T: RabbitMQControllerBase
        {
            return services.AddScoped<RabbitMQControllerBase, T>();
        }
        /// <summary>
        /// Adds <see cref="RabbitMQSmartControllerBase"/> to service collection as hosted service.
        /// </summary>
        /// <typeparam name="T">The <see cref="RabbitMQSmartControllerBase"/> representation type.</typeparam>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="gZipSettings">The gzip settings. Compress the response message.</param>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
        public static IServiceCollection AddSmartRabbitMQController<T>(this IServiceCollection services, RabbitMQConfiguration configuration, GZipSettings? gZipSettings = null, JsonSerializerSettings? serializerOptions = null) where T : RabbitMQSmartControllerBase
        {
            return services.AddHostedService(sr =>
            {
                return RabbitMQSmartControllerBase.InitializeNewController<T>(configuration, sr, gZipSettings, serializerOptions);
            });
        }


    }
}
