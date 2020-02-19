using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestCreateConnectionExtensions
    {
        public const int RETRY_UP_TO = 2;
        public const int TIMEOUT_IN_SECONDS = 1;
        public const int RETRY_INTERVAL_IN_SECONDS = 1;

        public TimeSpan TimeoutPerTry => TimeSpan.FromSeconds(TIMEOUT_IN_SECONDS);
        public TimeSpan RetryInterval => TimeSpan.FromSeconds(RETRY_INTERVAL_IN_SECONDS);

        [TestMethod]
        public void Test_GetInvalidConnection_IsNull()
        {
            // Arrange
            var usingUri = new Uri("amqp://127.0.0.1:5673");

            // Act
            var connection = new ConnectionFactory()
            {
                Uri = usingUri
            }.CreateConnection(
                this.TimeoutPerTry,
                this.RetryInterval,
                RETRY_UP_TO
            );

            // Assert
            Assert.IsNull(connection);
        }

        [TestMethod]
        public void Test_GetValidConnection_IsOpen()
        {
            // Arrange
            var usingUri = new Uri(Constants.CONNECTION_STRING);

            // Act
            var connection = new ConnectionFactory()
            {
                Uri = usingUri
            }.CreateConnection(
                this.TimeoutPerTry,
                this.RetryInterval,
                RETRY_UP_TO
            );

            // Assert
            Assert.IsNotNull(connection);
            Assert.IsTrue(connection.IsOpen);

            connection.Close();
            connection.Dispose();
        }
    }
}