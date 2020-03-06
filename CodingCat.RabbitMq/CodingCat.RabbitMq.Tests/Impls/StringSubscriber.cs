using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions;
using CodingCat.Serializers.Impls;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringSubscriber : BaseSubscriber<string>
    {
        public ISerializer<string> Serializer { get; }

        #region Constructor(s)

        public StringSubscriber(
            IModel channel,
            string queueName,
            IProcessor<string> processor
        ) : base(channel, queueName, processor)
        {
            this.IsAutoAck = true;

            this.Serializer = new StringSerializer();
        }

        #endregion Constructor(s)

        protected override string FromBytes(byte[] bytes)
        {
            return this.Serializer.FromBytes(bytes);
        }
    }
}