using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntTimeoutSubscriber : IntSubscriber
    {
        #region Constructor(s)

        public IntTimeoutSubscriber(IQueue queue) : base(queue)
        {
            this.Timeout = TimeSpan.FromMilliseconds(0);
        }

        #endregion Constructor(s)

        protected override int Process(
            int input,
            BasicDeliverEventArgs eventArgs
        )
        {
            Thread.Sleep(this.Timeout.Add(TimeSpan.FromSeconds(5)));
            return base.Process(input, eventArgs);
        }
    }
}