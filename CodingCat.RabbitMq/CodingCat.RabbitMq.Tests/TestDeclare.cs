using System;
using System.Collections.Generic;
using CodingCat.RabbitMq.Impls;
using CodingCat.RabbitMq.Tests.Abstracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = CodingCat.RabbitMq.Enums.ExchangeType;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestDeclare : BaseTest, IDisposable
    {
        public const string QUEUE_NAME = nameof(TestDeclare);
        public static readonly string ExchangeName = $"{QUEUE_NAME}.direct";

        public new void Dispose()
        {
            var consumerChannel = this.Connection.CreateModel();
            var consumer = new EventingBasicConsumer(consumerChannel);
            consumerChannel.BasicConsume(QUEUE_NAME, false, consumer);
            consumerChannel.Close();
            consumerChannel.Dispose();

            base.Dispose();
        }

        [TestMethod]
        public void Test_QueueDeclare_Ok()
        {
            // Arrange
            var properties = new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            };

            // Act
            using (var queue = properties.Declare(this.Connection))
                queue.Channel.QueueDeclarePassive(QUEUE_NAME);

            // Assert
        }

        [TestMethod]
        public void Test_ExchangeDeclare_Ok()
        {
            // Arrange
            var queueProperties = new QueueProperty()
            {
                Name = QUEUE_NAME,
                BindingKey = Guid.NewGuid().ToString(),
                IsAutoDelete = true,
                IsDurable = false
            };
            var exchangeProperties = new ExchangeProperty()
            {
                Name = ExchangeName,
                IsAutoDelete = true,
                IsDurable = false,
                ExchangeType = ExchangeType.Direct
            };

            // Act
            using (var exchange = exchangeProperties.Declare(this.Connection))
            {
                using (var queue = queueProperties.Declare(this.Connection))
                {
                    queue.Bind(exchange);
                    exchange.Channel.ExchangeDeclarePassive(ExchangeName);
                }
            }

            // Assert
        }
    }
}
