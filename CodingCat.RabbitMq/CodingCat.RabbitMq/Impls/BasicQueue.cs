using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Impls
{
    public class BasicQueue : IQueue
    {
        public IModel Channel { get; private set; }

        public string Name { get; }
        public string BindingKey { get; }

        public bool IsDurable { get; }
        public bool IsExclusive { get; }
        public bool IsAutoDelete { get; }
        public IDictionary<string, object> Arguments { get; }

        #region Constructor(s)

        public BasicQueue(IQueueProperty properties)
        {
            this.Name = properties.Name;
            this.BindingKey = properties.BindingKey;

            this.IsDurable = properties.IsDurable;
            this.IsExclusive = properties.IsExclusive;
            this.IsAutoDelete = properties.IsAutoDelete;
            this.Arguments = properties.Arguments;
        }

        #endregion Constructor(s)

        internal BasicQueue Declare(IConnection connection)
        {
            this.Channel = connection.CreateModel();
            this.Channel.QueueDeclare(
                this.Name,
                this.IsDurable,
                this.IsExclusive,
                this.IsAutoDelete,
                this.Arguments
            );
            return this;
        }

        public IQueue Bind(
            IExchangeProperty properties,
            IDictionary<string, object> arguments = null
        )
        {
            if (this.Channel == null)
                throw new InvalidOperationException(
                    "the queue is not yet declared"
                );

            this.Channel.QueueBind(
                this.Name,
                exchange: properties.Name,
                routingKey: this.BindingKey,
                arguments: arguments
            );
            return this;
        }
    }
}