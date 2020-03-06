using CodingCat.RabbitMq.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CodingCat.RabbitMq
{
    public static class ResolveExtensions
    {
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
    }
}