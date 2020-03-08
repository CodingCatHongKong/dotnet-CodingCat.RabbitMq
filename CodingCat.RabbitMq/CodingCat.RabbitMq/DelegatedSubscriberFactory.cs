using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using ISubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq
{
    public class DelegatedSubscriberFactory : ISubscriberFactory
    {
        public delegate ISubscriber CreateSubscriber(IModel channel);

        public CreateSubscriber DelegatedCreateSubscriber { get; set; }

        #region Constructor(s)

        public DelegatedSubscriberFactory()
        {
        }

        public DelegatedSubscriberFactory(
            CreateSubscriber delegatedCreateSubscriber
        ) : this()
        {
            this.DelegatedCreateSubscriber = delegatedCreateSubscriber;
        }

        #endregion Constructor(s)

        public ISubscriber GetSubscribed(IModel channel)
        {
            return this.DelegatedCreateSubscriber(channel)
                .Subscribe();
        }
    }
}