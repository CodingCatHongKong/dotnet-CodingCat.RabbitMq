using CodingCat.RabbitMq.Abstractions;
using CodingCat.Serializers.Impls;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringPublisher : BasePublisher<string>
    {
        public ISerializer<string> Serializer { get; set; }

        #region Constructor(s)

        public StringPublisher(IModel channel, string routingKey)
            : base(channel)
        {
            this.RoutingKey = routingKey;

            this.Serializer = new StringSerializer();
        }

        public StringPublisher(
            IModel channel,
            string routingKey,
            string exchangeName
        ) : this(channel, routingKey)
        {
            this.ExchangeName = ExchangeName;
        }

        #endregion Constructor(s)

        protected override byte[] ToBytes(string input)
        {
            return this.Serializer.ToBytes(input);
        }
    }
}