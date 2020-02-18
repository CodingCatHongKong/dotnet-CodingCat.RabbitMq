using CodingCat.RabbitMq.Impls;
using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Abstracts;
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

        public IQueue GetDeclaredQueue()
        {
            return new QueueProperty()
            {
                Name = QUEUE_NAME,
                IsAutoDelete = true,
                IsDurable = false
            }.Declare(this.Connection);
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

        [TestMethod]
        public void Test_BasicPublisher_Receive_Ok()
        {
            // Arrange
            var queue = this.GetDeclaredQueue();
            var expected = Guid.NewGuid().ToString();
            var publisher = new StringPublisher(queue);
            MockDispose(publisher);

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
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicPublisher_WithResponse_Ok()
        {
            // Arrange
            var input = new Random().Next(0, 1000) - 1;
            var expected = input + 1;

            var responseEvent = new AutoResetEvent(false);
            var queue = this.GetDeclaredQueue();
            var publisher = new IntPublisher(queue);
            MockDispose(publisher);

            // Act
            var actual = -1;
            Task.Run(() =>
            {
                actual = publisher.Process(input);
                responseEvent.Set();
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
                responseEvent.Set();
            }

            // Assert
            responseEvent.WaitOne();
            Assert.AreEqual(expected, actual);
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicPublisher_WithResponse_Exception_Ok()
        {
            // Arrange
            var input = new Random().Next(0, 1000) - 1;
            var expected = input + 1;

            var responseEvent = new AutoResetEvent(false);
            var queue = this.GetDeclaredQueue();
            var publisher = new IntPublisher(queue)
            {
                OutputSerializer = new DeserializeNotImplementedSerializer<int>()
            };
            MockDispose(publisher);

            // Act
            var actual = -1;
            Task.Run(() =>
            {
                actual = publisher.Process(input);
                responseEvent.Set();
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
                responseEvent.Set();
            }

            // Assert
            responseEvent.WaitOne();
            Assert.IsNotNull(publisher.LastException);
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicPublisher_WithTimeout_Ok()
        {
            // Arrange
            var input = new Random().Next(0, 1000);
            var expected = input + 1;

            var queue = this.GetDeclaredQueue();
            var publisher = new IntPublisher(queue)
            {
                Timeout = TimeSpan.FromSeconds(2),
                DefaultValue = expected
            };
            MockDispose(publisher);

            // Act
            var actual = publisher.Process(input);

            // Assert
            Assert.AreEqual(expected, actual);
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicSubscriber_Ok()
        {
            // Arrange
            var responseEvent = new AutoResetEvent(false);
            var expected = Guid.NewGuid().ToString();

            var publisher = new StringPublisher(GetDeclaredQueue());
            var subscriber = new StringSubscriber(GetDeclaredQueue());

            MockDispose(publisher);
            MockDispose(subscriber);
            subscriber.MessageCompleted += (sender, args) => responseEvent.Set();

            // Act
            publisher.Send(expected);
            subscriber.Subscribe();

            responseEvent.WaitOne();
            var actual = subscriber.LastInput;

            // Assert
            Assert.AreEqual(expected, actual);
            subscriber.Dispose();
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicSubscriber_WithException_Ok()
        {
            // Arrange
            var responseEvent = new AutoResetEvent(false);

            var publisher = new StringPublisher(GetDeclaredQueue());
            var subscriber = new StringSubscriber(GetDeclaredQueue())
            {
                InputSerializer = new DeserializeNotImplementedSerializer()
            };

            MockDispose(publisher);
            MockDispose(subscriber);
            subscriber.MessageCompleted += (sender, args) => responseEvent.Set();

            // Act
            publisher.Send("");
            subscriber.Subscribe();

            responseEvent.WaitOne();
            var actual = subscriber.LastInput;

            // Assert
            Assert.IsNotNull(subscriber.LastException);
            subscriber.Dispose();
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicSubscriber_WithResponse_Ok()
        {
            // Arrange
            var input = new Random().Next(0, 1000);
            var expected = input + 1;
            var responseEvent = new AutoResetEvent(false);

            var publisherQueue = GetDeclaredQueue();
            var subscriberQueue = GetDeclaredQueue();

            var publisher = new IntPublisher(publisherQueue);
            var subscriber = new IntSubscriber(subscriberQueue);

            MockDispose(publisher);
            MockDispose(subscriber);

            // Act
            var actual = -1;

            Task.Run(() =>
            {
                actual = publisher.Process(input);
                responseEvent.Set();
            });
            subscriber.Subscribe();

            // Assert
            responseEvent.WaitOne();
            Assert.AreEqual(expected, actual);

            subscriber.Dispose();
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicSubscriber_Timeout_Ok()
        {
            // Arrange
            var publisherQueue = GetDeclaredQueue();
            var subscriberQueue = GetDeclaredQueue();

            var publisher = new IntPublisher(publisherQueue);
            var subscriber = new IntTimeoutSubscriber(subscriberQueue)
            {
                Timeout = TimeSpan.FromMilliseconds(0)
            };

            var responseEvent = new AutoResetEvent(false);
            var expected = subscriber.DefaultOutput;

            MockDispose(publisher);
            MockDispose(subscriber);

            // Act
            var actual = 999;

            Task.Run(() =>
            {
                actual = publisher.Process(actual);
                responseEvent.Set();
            });
            subscriber.Subscribe();

            // Assert
            responseEvent.WaitOne();
            Assert.AreEqual(expected, actual);

            subscriber.Dispose();
            publisher.Dispose();
        }
    }
}