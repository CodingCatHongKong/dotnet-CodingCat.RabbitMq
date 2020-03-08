using CodingCat.RabbitMq.Abstractions;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq
{
    public class DelegatedInitializer : BaseInitializer
    {
        public delegate void ConfigureRabbitMq(IModel channel);

        public ConfigureRabbitMq DelegatedConfigure { get; }

        #region Constructor(s)

        public DelegatedInitializer(
            IModel channel,
            IEnumerable<ISubscriberFactory> factories,
            IEnumerable<IExchange> exchanges,
            IEnumerable<IQueue> queues,
            ConfigureRabbitMq delegatedConfigure
        ) : base(channel, factories, exchanges, queues)
        {
            this.DelegatedConfigure = delegatedConfigure;
        }

        #endregion Constructor(s)

        public override IInitializer Configure(IModel channel)
        {
            this.DelegatedConfigure(channel);
            return this;
        }
    }
}