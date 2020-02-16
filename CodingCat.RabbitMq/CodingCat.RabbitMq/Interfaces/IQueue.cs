using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IQueue
    {
        IModel Channel { get; }

        IQueue Bind(IExchangeProperty exchange, IDictionary<string, object> arguments);
    }
}