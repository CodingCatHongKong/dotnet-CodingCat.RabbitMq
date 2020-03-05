using CodingCat.RabbitMq.Abstractions;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System;

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
            return new BasicQueue()
            {
                Name = this.QueueName,
                BindingKey = bindingKey,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection.CreateModel());
        }

        public IExchange GetDeclaredExchange(ExchangeTypes exchangeType)
        {
            return new BasicExchange()
            {
                Name = $"{this.QueueName}.{exchangeType.ToString().ToLower()}",
                ExchangeType = exchangeType,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection.CreateModel());
        }

        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }
    }
}