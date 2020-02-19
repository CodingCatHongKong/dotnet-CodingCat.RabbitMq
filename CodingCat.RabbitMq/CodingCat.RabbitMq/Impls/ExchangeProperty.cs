using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;
using ExchangeType = CodingCat.RabbitMq.Enums.ExchangeType;

namespace CodingCat.RabbitMq.Impls
{
    public class ExchangeProperty
        : IExchangeProperty, IDeclarable<BasicExchange>
    {
        public string Name { get; set; }
        public ExchangeType ExchangeType { get; set; } = ExchangeType.Unknown;
        public string Type => this.ExchangeType.ToString().ToLower();

        public bool IsDurable { get; set; } = true;
        public bool IsAutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }

        public BasicExchange Declare(IConnection connection)
        {
            return new BasicExchange(this).Declare(connection);
        }
    }
}