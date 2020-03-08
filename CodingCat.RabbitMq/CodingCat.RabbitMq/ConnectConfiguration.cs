using CodingCat.Mq.Abstractions.Interfaces;
using System;

namespace CodingCat.RabbitMq
{
    public class ConnectConfiguration : IConnectConfiguration
    {
        public TimeSpan TimeoutPerTry { get; set; }
        public TimeSpan RetryInterval { get; set; }
        public uint RetryUpTo { get; set; }
    }
}