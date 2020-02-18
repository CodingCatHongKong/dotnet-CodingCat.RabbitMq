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

        public override string QueueName => QUEUE_NAME;

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