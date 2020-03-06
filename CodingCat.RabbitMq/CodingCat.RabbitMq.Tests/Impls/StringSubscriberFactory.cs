using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringSubscriberFactory : DelegatedSubscriberFactory
    {
        #region Constructor(s)

        public StringSubscriberFactory(
            IQueue queue,
            IProcessor<string> processor
        ) : base()
        {
            this.DelegatedCreateSubscriber = channel =>
                new StringSubscriber(channel, queue.Name, processor)
                {
                    IsAutoAck = true
                };
        }

        #endregion Constructor(s)
    }
}