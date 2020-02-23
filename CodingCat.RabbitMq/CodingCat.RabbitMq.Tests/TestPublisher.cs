using CodingCat.RabbitMq.Tests.Abstracts;
using CodingCat.RabbitMq.Tests.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestPublisher : BaseTest
    {
        public const string QUEUE_NAME = nameof(TestPublisher);

        public override string QueueName => QUEUE_NAME;

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
                DefaultOutput = expected
            };
            MockDispose(publisher);

            // Act
            var actual = publisher.Process(input);

            // Assert
            Assert.AreEqual(expected, actual);
            publisher.Dispose();
        }

        [TestMethod]
        public void Test_BasicPublisher_WithResponse_ChannelNotDisposedByReplySubscriber()
        {
            // Arrange
            var input = new Random().Next(0, 1000) - 1;
            var expected = input + 1;

            var publisher = new IntPublisher(this.GetDeclaredQueue());
            var subscriber = new IntSubscriber(this.GetDeclaredQueue())
                .Subscribe();

            // Act
            publisher.Process(int.MinValue);
            var actual = publisher.Process(input);

            // Assert
            Assert.AreEqual(expected, actual);

            subscriber.Dispose();
            publisher.Dispose();
        }
    }
}