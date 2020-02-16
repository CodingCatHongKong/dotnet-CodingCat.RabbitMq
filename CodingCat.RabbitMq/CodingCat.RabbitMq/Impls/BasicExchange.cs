using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Impls
{
    public class BasicExchange : IExchange, IExchangeProperty
    {
        public IModel Channel { get; private set; }

        public string Name { get; }
        public string Type { get; }

        public bool IsDurable { get; }
        public bool IsAutoDelete { get; }
        public IDictionary<string, object> Arguments { get; }

        #region Constructor(s)

        public BasicExchange(IExchangeProperty property)
        {
            this.Name = property.Name;
            this.Type = property.Type;

            this.IsDurable = property.IsDurable;
            this.IsAutoDelete = property.IsAutoDelete;
            this.Arguments = property.Arguments;
        }

        #endregion Constructor(s)

        internal IExchange Declare(IConnection connection)
        {
            this.Channel = connection.CreateModel();
            this.Channel.ExchangeDeclare(
                this.Name,
                this.Type,
                this.IsDurable,
                this.IsAutoDelete,
                this.Arguments
            );
            return this;
        }
    }
}