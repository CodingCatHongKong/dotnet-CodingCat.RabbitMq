using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Impls;
using CodingCat.Serializers.Impls;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntPublisher : BasicPublisher<int, int>
    {
        public Exception LastException { get; private set; }

        #region Constructor(s)

        public IntPublisher(IQueue declaredQueue)
        {
            this.UsingQueue = declaredQueue;

            this.InputSerializer = new Int32Serializer();
            this.OutputSerializer = new Int32Serializer();
        }

        #endregion Constructor(s)

        public override void OnReceiveError(Exception exception)
        {
            base.OnReceiveError(exception);
            this.LastException = exception;
        }
    }
}