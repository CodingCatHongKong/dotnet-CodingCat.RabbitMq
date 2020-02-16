using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Impls
{
    public class QueueProperty
        : IQueueProperty, IDeclarable<IQueue>
    {
        public string Name { get; set; }
        public string BindingKey { get; set; }

        public bool IsDurable { get; set; }
        public bool IsExclusive { get; set; }
        public bool IsAutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        public IQueue Declare(IConnection connection)
        {
            return new BasicQueue(this).Declare(connection);
        }
    }
}