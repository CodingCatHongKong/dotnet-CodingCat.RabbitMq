using CodingCat.RabbitMq.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CodingCat.RabbitMq
{
    public static class InjectExtensions
    {
        public static IServiceCollection AddExchange<T>(
            this IServiceCollection services,
            T exchange
        ) where T : class, IExchange
        {
            return services.AddSingleton(exchange);
        }

        public static IServiceCollection AddQueue<T>(
            this IServiceCollection services,
            T queue
        ) where T : class, IQueue
        {
            return services.AddSingleton(queue);
        }

        public static IServiceCollection AddFactory<T>(
            this IServiceCollection services
        ) where T : class, ISubscriberFactory
        {
            return services.AddTransient<ISubscriberFactory, T>();
        }
    }
}