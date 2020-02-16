using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.PubSub.Abstracts
{
    public abstract class BaseBasicPublisher
    {
        public IExchangeProperty ExchangeProperty { get; set; }
        public IQueue UsingQueue { get; set; }

        public string RoutingKey { get; set; }
        public bool IsMandatory { get; set; } = false;

        protected void Publish(byte[] body, IBasicProperties properties)
        {
            if (this.UsingQueue.Channel == null)
                throw new InvalidOperationException(
                    "the queue is not yet declared"
                );

            var exchangeName = this.ExchangeProperty?.Name ?? "";
            properties = this.GetOrCreateProperties(properties);

            this.UsingQueue.Channel
                .BasicPublish(
                    exchange: exchangeName,
                    routingKey: this.RoutingKey,
                    mandatory: this.IsMandatory,
                    basicProperties: properties,
                    body: body
                );
        }

        protected IBasicProperties GetOrCreateProperties(
            IBasicProperties properties
        ) => properties ?? this.UsingQueue.Channel.CreateBasicProperties();
    }
}