using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq
{
    public static class InjectExtensions
    {
        public static IServiceCollection AddExchange<T>(
            this IServiceCollection services,
            T exchange
        ) where T : class, IExchange
        {
            return services
                .AddSingleton(exchange)
                .AddSingleton<IExchange>(exchange);
        }

        public static IServiceCollection AddQueue<T>(
            this IServiceCollection services,
            T queue
        ) where T : class, IQueue
        {
            return services
                .AddSingleton(queue)
                .AddSingleton<IQueue>(queue);
        }

        public static IServiceCollection AddFactory<T>(
            this IServiceCollection services
        ) where T : class, ISubscriberFactory
        {
            return services.AddSingleton<ISubscriberFactory, T>();
        }

        public static IServiceCollection AddFactory<T>(
            this IServiceCollection services,
            T factory
        ) where T : class, ISubscriberFactory
        {
            return services.AddSingleton<ISubscriberFactory>(factory);
        }

        public static IServiceCollection AddFactory<T>(
            this IServiceCollection services,
            Func<IServiceProvider, T> callback
        ) where T : class, ISubscriberFactory
        {
            return services.AddSingleton<ISubscriberFactory>(
                provider => callback(provider)
            );
        }

        public static IServiceCollection AddSingleConnection(
            this IServiceCollection services,
            IConnectionFactory connectionFactory
        )
        {
            return services
                .AddSingleton(provider =>
                {
                    var configuration = provider
                        .GetRequiredService<IConnectConfiguration>();
                    return connectionFactory
                        .CreateConnection(configuration);
                })
                .AddSingleton(provider =>
                    provider.ResolveRabbitMqConnection()
                        ?.CreateModel()
                );
        }
    }
}