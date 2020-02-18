using CodingCat.RabbitMq.Impls;
using CodingCat.RabbitMq.Tests.Abstracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using ExchangeType = CodingCat.RabbitMq.Enums.ExchangeType;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestBasicQueue : BaseTest
    {
        public const string QUEUE_NAME = nameof(TestBasicQueue);
        public static readonly string ExchangeName = $"{QUEUE_NAME}.direct";
        
        public override string QueueName => QUEUE_NAME;

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
            {
                queue.Channel.QueueDeclarePassive(QUEUE_NAME);
                queue.Channel.QueueDelete(QUEUE_NAME, true, true);
            }

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

                    queue.Channel.QueueDelete(QUEUE_NAME, true, true);
                }
                exchange.Channel.ExchangeDelete(ExchangeName, true);
            }

            // Assert
        }

        [TestMethod]
        public void Test_Publish_Receive_Ok()
        {
            // Arrange
            var queue = this.GetDeclaredQueue();
            var expected = Guid.NewGuid().ToString();

            queue.Channel.BasicPublish(
                exchange: "",
                routingKey: queue.Name,
                body: Encoding.UTF8.GetBytes(expected)
            );

            // Act
            var message = queue.Channel.BasicGet(queue.Name, true);
            var actual = null as string;
            try
            {
                actual = Encoding.UTF8.GetString(message.Body);
            }
            catch { }

            // Assert
            Assert.IsNotNull(message);
            Assert.AreEqual(expected, actual);

            queue.Channel.QueueDelete(queue.Name, false, false);
            queue.Dispose();
        }

        [TestMethod]
        public void Test_Subscription_Ok()
        {
            // Arrange
            var queue = this.GetDeclaredQueue();
            var expected = Guid.NewGuid().ToString();

            // Act
            var actual = null as string;

            var waiter = new AutoResetEvent(false);
            var consumer = new EventingBasicConsumer(queue.Channel);

            consumer.Received += (sender, @event) =>
            {
                actual = Encoding.UTF8.GetString(@event.Body);
                waiter.Set();
            };

            queue.Channel.BasicConsume(queue.Name, true, consumer);
            queue.Channel.BasicPublish(
                exchange: "",
                routingKey: queue.Name,
                body: Encoding.UTF8.GetBytes(expected)
            );

            // Assert
            waiter.WaitOne();
            Assert.AreEqual(expected, actual);

            queue.Channel.BasicCancel(consumer.ConsumerTag);
            queue.Channel.QueueDelete(queue.Name, false, false);
            queue.Dispose();
        }
    }
}