using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions;
using CodingCat.Serializers.Impls;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntSubscriber : BaseSubscriber<int, int>
    {
        public ISerializer<int> Serializer { get; }

        #region Constructor(s)

        public IntSubscriber(
            IModel channel,
            string queueName,
            IProcessor<int, int> processor
        ) : base(channel, queueName, processor)
        {
            this.IsAutoAck = true;

            this.Serializer = new Int32Serializer();
        }

        #endregion Constructor(s)

        protected override int FromBytes(byte[] bytes)
        {
            return this.Serializer.FromBytes(bytes);
        }

        protected override byte[] ToBytes(int output)
        {
            return this.Serializer.ToBytes(output);
        }
    }
}