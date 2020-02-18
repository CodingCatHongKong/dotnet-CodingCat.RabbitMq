using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Impls;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntPublisher : BaseBasicPublisher<int, int>
    {
        public Exception LastException { get; private set; }

        #region Constructor(s)

        public IntPublisher(IQueue declaredQueue)
        {
            this.UsingQueue = declaredQueue;
            this.RoutingKey = this.UsingQueue.BindingKey;

            this.InputSerializer = new Int32Serializer();
            this.OutputSerializer = new Int32Serializer();
        }

        public IntPublisher(
            IExchangeProperty exchange,
            IQueue declaredQueue
        ) : this(declaredQueue)
        {
            this.ExchangeProperty = exchange;
        }

        #endregion Constructor(s)

        public override void OnReceiveError(Exception exception)
        {
            this.LastException = exception;
        }
    }
}