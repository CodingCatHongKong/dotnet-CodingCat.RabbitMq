using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntSubscriberFactory : DelegatedSubscriberFactory
    {
        #region Constructor(s)

        public IntSubscriberFactory(
            IQueue queue,
            IProcessor<int, int> processor
        ) : base()
        {
            this.DelegatedCreateSubscriber = channel =>
                new IntSubscriber(channel, queue.Name, processor)
                {
                    IsAutoAck = true
                };
        }

        #endregion Constructor(s)
    }
}