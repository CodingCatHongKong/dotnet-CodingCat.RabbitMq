using System;
using System.Collections.Generic;
using System.Text;

namespace CodingCat.RabbitMq.PubSub.Interfaces
{
    public interface ITimeoutPubSub
    {
        TimeSpan Timeout { get; }
    }
}
