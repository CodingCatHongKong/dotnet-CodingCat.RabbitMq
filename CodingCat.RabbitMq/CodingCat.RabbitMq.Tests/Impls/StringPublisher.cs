﻿using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Impls;
using CodingCat.Serializers.Impls;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringPublisher : BasicPublisher<string>
    {
        #region Constructor(s)

        public StringPublisher(IQueue declaredQueue)
        {
            this.UsingQueue = declaredQueue;
            this.RoutingKey = this.UsingQueue.BindingKey;
            this.InputSerializer = new StringSerializer();
        }

        public StringPublisher(
            IExchangeProperty exchange,
            IQueue declaredQueue
        ) : this(declaredQueue)
        {
            this.ExchangeProperty = exchange;
        }

        #endregion Constructor(s)
    }
}