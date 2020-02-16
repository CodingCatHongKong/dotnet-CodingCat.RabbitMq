using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Impls;
using CodingCat.Serializers.Impls;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntRequester : BasicPublisher<int, int>
    {
        #region Constructor(s)
        public IntRequester(IQueue declaredQueue)
        {
            this.UsingQueue = declaredQueue;

            this.InputSerializer = new Int32Serializer();
            this.OutputSerializer = new Int32Serializer();
        }
        #endregion
    }
}
