using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.PubSub.Impls
{
    public class BasicPublisher<TInput> : BaseBasicPublisher
    {
        public ISerializer<TInput> InputSerializer { get; set; }

        public void Send(TInput input) => this.Send(input, null);

        public void Send(TInput input, IBasicProperties properties)
        {
            var body = this.InputSerializer.ToBytes(input);
            this.Publish(body, properties);
        }
    }

    public class BasicPublisher<TInput, TOutput> : BaseBasicPublisher<TOutput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }

        public override void OnReceiveError(Exception exception)
        {
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }

        public TOutput Process(TInput input) => this.Process(input, null);

        public TOutput Process(TInput input, IBasicProperties properties)
        {
            var body = this.InputSerializer.ToBytes(input);
            return this.Process(body, properties);
        }
    }
}