using CodingCat.RabbitMq.Impls;
using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Abstracts;
using RabbitMQ.Client;
using System;
using ExchangeType = CodingCat.RabbitMq.Enums.ExchangeType;

namespace CodingCat.RabbitMq.Tests.Abstracts
{
    public abstract class BaseTest : IDisposable
    {
        protected IConnection Connection { get; }

        public abstract string QueueName { get; }

        #region Constructor(s)

        public BaseTest()
        {
            this.Connection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.CONNECTION_STRING)
            }.CreateConnection();
        }

        #endregion Constructor(s)

        public IQueue GetDeclaredQueue(string bindingKey = null)
        {
            return new QueueProperty()
            {
                Name = this.QueueName,
                BindingKey = bindingKey,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);
        }

        public IExchange GetDeclaredExchange(ExchangeType exchangeType)
        {
            return new ExchangeProperty()
            {
                Name = $"{this.QueueName}.{exchangeType.ToString().ToLower()}",
                IsAutoDelete = true,
                IsDurable = false,
                ExchangeType = exchangeType
            }.Declare(this.Connection);
        }

        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }

        public static BaseBasicPublisher MockDispose(
            BaseBasicPublisher publisher
        )
        {
            publisher.Disposing += (sender, eventArgs) =>
            {
                var queue = publisher.UsingQueue;
                queue.Channel.QueueDelete(queue.Name, false, false);
                queue.Dispose();
            };
            return publisher;
        }

        public static BaseBasicSubscriber MockDispose(
            BaseBasicSubscriber subscriber
        )
        {
            subscriber.Disposing += (sender, eventArgs) =>
            {
                subscriber.UsingQueue.Dispose();
            };
            return subscriber;
        }
    }
}