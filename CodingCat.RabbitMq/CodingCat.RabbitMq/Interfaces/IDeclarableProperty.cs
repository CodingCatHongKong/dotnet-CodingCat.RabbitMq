using System.Collections.Generic;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IDeclarableProperty
    {
        string Name { get; }
        bool IsDurable { get; }
        bool IsAutoDelete { get; }
        IDictionary<string, object> Arguments { get; }
    }
}