using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Impls
{
    public class QueueProperty
        : IQueueProperty, IDeclarable<BasicQueue>
    {
        public string Name { get; set; }
        public string BindingKey { get; set; }

        public bool IsDurable { get; set; } = true;
        public bool IsExclusive { get; set; } = false;
        public bool IsAutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }

        public BasicQueue Declare(IConnection connection)
        {
            return new BasicQueue(this).Declare(connection);
        }
    }
}