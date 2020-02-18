using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Impls;
using RabbitMQ.Client.Events;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringSubscriber : BaseBasicSubscriber<string>
    {
        public string LastInput { get; private set; }
        public Exception LastException { get; private set; }

        #region Constructor(s)

        public StringSubscriber(IQueue queue)
        {
            this.UsingQueue = queue;
            this.IsAutoAck = true;

            this.DefaultInput = null;
            this.InputSerializer = new StringSerializer();
        }

        #endregion Constructor(s)

        public override void OnSubscribeException(Exception exception)
        {
            this.LastException = exception;
        }

        protected override void Process(
            string input,
            BasicDeliverEventArgs eventArgs
        )
        {
            this.LastInput = input;
        }
    }
}