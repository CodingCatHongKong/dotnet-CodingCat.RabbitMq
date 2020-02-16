using CodingCat.RabbitMq.PubSub.Abstracts;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

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

    public class BasicPublisher<TInput, TOutput> : BaseBasicPublisher
    {
        public const string REPLY_HEADER = "reply-to";

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
        public TOutput DefaultValue { get; set; }

        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }

        public TOutput Send(TInput input) => this.Send(input, null);

        public TOutput Send(TInput input, IBasicProperties properties)
        {
            var queueName = this.UsingQueue.Channel.QueueDeclare(
                queue: string.Empty,
                durable: false,
                exclusive: false,
                autoDelete: true
            ).QueueName;

            var body = this.InputSerializer.ToBytes(input);
            properties = this.GetOrCreateProperties(properties);
            properties.ReplyTo = queueName;

            this.Publish(body, properties);

            return this.Subscribe(queueName);
        }

        private TOutput Subscribe(string queueName)
        {
            var reset = new AutoResetEvent(false);
            var output = this.DefaultValue;

            var channel = this.UsingQueue.Channel;
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, @event) =>
            {
                try
                {
                    output = this.OutputSerializer.FromBytes(@event.Body);
                }
                catch { }

                reset.Set();
                channel.BasicCancel(consumer.ConsumerTag);
            };
            channel.BasicConsume(queueName, true, consumer);

            reset.WaitOne();
            return output;
        }
    }
}