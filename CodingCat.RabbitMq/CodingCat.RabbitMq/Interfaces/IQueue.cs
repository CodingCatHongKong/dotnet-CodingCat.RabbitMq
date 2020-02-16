using System.Collections.Generic;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IQueue
    {
        IQueue Bind(IExchangeProperty exchange, IDictionary<string, object> arguments);
    }
}