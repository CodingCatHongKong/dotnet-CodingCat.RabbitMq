using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client.Events;
using System;

namespace CodingCat.RabbitMq.PubSub.Impls
{
    public class ReplySubscriber<T> : BaseBasicSubscriber<T>
    {
        public Exception Exception { get; private set; }

        public T Replied { get; internal set; }

        #region Constructor(s)

        public ReplySubscriber(
            ReplyQueue replyQueue,
            ISerializer<T> serializer
        )
        {
            this.IsAutoAck = true;
            this.UsingQueue = replyQueue;
            this.InputSerializer = serializer;
        }

        #endregion Constructor(s)

        public new ReplySubscriber<T> Subscribe()
        {
            base.Subscribe();
            return this;
        }

        protected override void Process(
            T input,
            BasicDeliverEventArgs eventArgs
        )
        {
            this.Replied = input;
        }

        protected override void OnError(Exception exception)
        {
            this.Exception = exception;
        }
    }
}