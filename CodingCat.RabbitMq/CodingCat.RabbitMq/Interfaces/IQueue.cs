using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IQueue : IQueueProperty, IDisposable
    {
        IModel Channel { get; }

        IQueue Bind(IExchangeProperty exchange, IDictionary<string, object> arguments = null);
    }
}