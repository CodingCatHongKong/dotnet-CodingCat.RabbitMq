using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Impls;
using CodingCat.Serializers.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion
    }
}
