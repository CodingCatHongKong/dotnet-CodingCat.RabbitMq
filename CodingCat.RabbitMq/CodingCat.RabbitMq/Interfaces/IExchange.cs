using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IExchange : IExchangeProperty, IDisposable
    {
        IModel Channel { get; }
    }
}