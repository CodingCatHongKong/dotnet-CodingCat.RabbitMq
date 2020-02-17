using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Impls;
using CodingCat.Serializers.Impls;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringPublisher : BasicPublisher<string>
    {
        #region Constructor(s)

        public StringPublisher(IQueue declaredQueue)
        {
            this.UsingQueue = declaredQueue;
            this.InputSerializer = new StringSerializer();
        }

        #endregion Constructor(s)
    }
}