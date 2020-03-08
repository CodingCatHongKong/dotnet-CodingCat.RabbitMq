using CodingCat.RabbitMq.Abstractions;
using CodingCat.Serializers.Impls;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntPublisher : BasePublisher<int, int>
    {
        public Exception LastException { get; private set; }

        public ISerializer<int> Serializer { get; set; }

        #region Constructor(s)

        public IntPublisher(IConnection connection, string routingKey)
            : base(connection)
        {
            this.RoutingKey = routingKey;

            this.Serializer = new Int32Serializer();
        }

        public IntPublisher(
            IConnection connection,
            string routingKey,
            string exchangeName
        ) : this(connection, routingKey)
        {
            this.ExchangeName = exchangeName;
        }

        #endregion Constructor(s)

        protected override int FromBytes(byte[] bytes)
        {
            return this.Serializer.FromBytes(bytes);
        }

        protected override byte[] ToBytes(int input)
        {
            return this.Serializer.ToBytes(input);
        }
    }
}