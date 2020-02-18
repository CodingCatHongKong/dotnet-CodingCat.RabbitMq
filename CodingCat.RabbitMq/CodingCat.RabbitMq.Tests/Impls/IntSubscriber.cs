using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Impls;
using RabbitMQ.Client.Events;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntSubscriber : BaseBasicSubscriber<int, int>
    {
        public Exception LastException { get; private set; }

        #region Constructor(s)

        public IntSubscriber(IQueue queue)
        {
            this.UsingQueue = queue;

            this.DefaultInput = -1;
            this.DefaultOutput = -1;

            this.InputSerializer = new Int32Serializer();
            this.OutputSerializer = new Int32Serializer();
        }

        #endregion Constructor(s)

        protected override void OnSubscribeException(Exception exception)
        {
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);

            this.LastException = exception;
        }

        protected override int Process(
            int input,
            BasicDeliverEventArgs eventArgs
        )
        {
            var output = this.DefaultOutput;
            try
            {
                output = input + 1;
                this.UsingQueue.Channel
                    .BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                this.OnSubscribeException(ex);
                this.UsingQueue.Channel
                    .BasicReject(eventArgs.DeliveryTag, false);
            }

            return output;
        }
    }
}