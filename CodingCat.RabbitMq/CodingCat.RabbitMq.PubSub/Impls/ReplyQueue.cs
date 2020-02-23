using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.PubSub.Impls
{
    public class ReplyQueue : IQueue
    {
        public IModel Channel { get; }

        public string Name { get; }
        public string BindingKey => null;

        public bool IsDurable => false;
        public bool IsExclusive => false;
        public bool IsAutoDelete => true;
        public IDictionary<string, object> Arguments { get; }

        #region Constructor(s)

        public ReplyQueue(
            IModel channel,
            IDictionary<string, object> arguments
        )
        {
            this.Channel = channel;

            this.Name = this.GetReplyQueueName();
            this.Arguments = arguments;
        }

        #endregion Constructor(s)

        public IQueue Bind(
            IExchangeProperty exchange,
            IDictionary<string, object> arguments = null
        )
        {
            throw new NotSupportedException();
        }

        public virtual void Dispose() { }

        private string GetReplyQueueName()
        {
            return this.Channel.QueueDeclare(
                queue: string.Empty,
                durable: this.IsDurable,
                exclusive: this.IsExclusive,
                autoDelete: this.IsAutoDelete,
                arguments: this.Arguments
            ).QueueName;
        }
    }
}