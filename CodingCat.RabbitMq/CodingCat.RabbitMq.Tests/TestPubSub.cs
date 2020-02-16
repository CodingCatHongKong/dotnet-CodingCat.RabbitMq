using CodingCat.RabbitMq.Impls;
using CodingCat.RabbitMq.Tests.Abstracts;
using CodingCat.RabbitMq.Tests.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestPubSub : BaseTest
    {
        public const string QUEUE_NAME = nameof(TestPubSub);

        [TestMethod]
        public void Test_Publish_Receive_Ok()
        {
            // Arrange
            var queue = new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);

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

            queue.Channel.QueueDelete(queue.Name, true, true);
            queue.Dispose();
        }

        [TestMethod]
        public void Test_Subscription_Ok()
        {
            // Arrange
            var queue = new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);

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
            queue.Channel.QueueDelete(queue.Name, true, true);
            queue.Dispose();
        }
        
        [TestMethod]
        public void Test_BasicPublisher_Receive_Ok()
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();

            var queue = new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);
            var publisher = new StringPublisher(queue);

            // Act
            publisher.Send(expected);

            var message = queue.Channel.BasicGet(queue.Name, true);
            var actual = null as string;
            try
            {
                actual = publisher.InputSerializer.FromBytes(message.Body);
            }
            catch { }

            // Assert
            Assert.IsNotNull(message);
            Assert.AreEqual(expected, actual);

            queue.Channel.QueueDelete(queue.Name, true, true);
            queue.Dispose();
        }

        [TestMethod]
        public void Test_BasicPublisher_WithResponse_Ok()
        {
            // Arrange
            var original = new Random().Next(0, 1000) - 1;
            var expected = original + 1;

            var resetEvent = new AutoResetEvent(false);
            var queue = new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);
            var publisher = new IntRequester(queue);

            // Act
            var actual = -1;
            Task.Run(() =>
            {
                actual = publisher.Send(original);
                resetEvent.Set();
            });

            try
            {
                var message = null as BasicGetResult;
                while ((message = queue.Channel.BasicGet(queue.Name, true)) == null)
                    Thread.Sleep(100);

                var source = publisher.InputSerializer
                    .FromBytes(message.Body);
                queue.Channel.BasicPublish(
                    exchange: "",
                    routingKey: message.BasicProperties.ReplyTo,
                    body: publisher.OutputSerializer.ToBytes(source + 1)
                );
            }
            catch
            {
                resetEvent.Set();
            }

            // Assert
            resetEvent.WaitOne();
            Assert.AreEqual(expected, actual);

            queue.Channel.QueueDelete(queue.Name, true, true);
            queue.Dispose();
        }
    }
}