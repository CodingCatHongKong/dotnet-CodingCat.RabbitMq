using CodingCat.Mq.Abstractions.Interfaces;
using System;

namespace CodingCat.RabbitMq.Tests
{
    public static class Constants
    {
        public const string CONNECTION_STRING = "amqp://localhost/test";

        public static readonly IConnectConfiguration ConnectConfiguration = new ConnectConfiguration()
        {
            TimeoutPerTry = TimeSpan.FromSeconds(10),
            RetryInterval = TimeSpan.FromSeconds(3),
            RetryUpTo = 5
        };
    }
}