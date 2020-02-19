using System;

namespace CodingCat.RabbitMq.PubSub.Interfaces
{
    public interface ITimeoutPubSub
    {
        TimeSpan Timeout { get; }
    }
}