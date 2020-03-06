using CodingCat.RabbitMq.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace CodingCat.RabbitMq
{
    public static class ResolveExtensions
    {
        public static IEnumerable<IExchange> ResolveExchanges(
            this IServiceProvider provider
        )
        {
            return provider.GetService<IEnumerable<IExchange>>();
        }

        public static IExchange ResolveExchange<T>(
            this IServiceProvider provider
        ) where T : class, IExchange
        {
            return provider.GetService<T>();
        }

        public static IExchange RequireExchange<T>(
            this IServiceProvider provider
        ) where T : class, IExchange
        {
            return provider.GetRequiredService<T>();
        }

        public static IEnumerable<IQueue> ResolveQueues(
            this IServiceProvider provider
        )
        {
            return provider.GetService<IEnumerable<IQueue>>();
        }

        public static IQueue ResolveQueue<T>(
            this IServiceProvider provider
        ) where T : class, IQueue
        {
            return provider.GetService<T>();
        }

        public static IQueue RequireQueue<T>(
            this IServiceProvider provider
        ) where T : class, IQueue
        {
            return provider.GetRequiredService<T>();
        }

        public static IEnumerable<ISubscriberFactory> ResolveFactories(
            this IServiceProvider provider
        )
        {
            return provider.GetService<IEnumerable<ISubscriberFactory>>();
        }

        public static ISubscriberFactory ResolveFactory(
            this IServiceProvider provider
        )
        {
            return provider.GetService<ISubscriberFactory>();
        }

        public static ISubscriberFactory RequireFactory(
            this IServiceProvider provider
        )
        {
            return provider.GetRequiredService<ISubscriberFactory>();
        }

        public static IConnection ResolveRabbitMqConnection(
            this IServiceProvider provider
        )
        {
            return provider.GetService<IConnection>();
        }

        public static IConnection RequireRabbitMqConnection(
            this IServiceProvider provider
        )
        {
            return provider.GetRequiredService<IConnection>();
        }

        public static IModel ResolveRabbitMqChannel(
            this IServiceProvider provider
        )
        {
            return provider.GetService<IModel>();
        }

        public static IModel RequireRabbitMqChannel(
            this IServiceProvider provider
        )
        {
            return provider.GetRequiredService<IModel>();
        }
    }
}