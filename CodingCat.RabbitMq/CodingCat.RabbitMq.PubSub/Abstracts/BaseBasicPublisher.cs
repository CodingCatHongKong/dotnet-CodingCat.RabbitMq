using CodingCat.RabbitMq.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.PubSub.Abstracts
{
    public abstract class BaseBasicPublisher : IDisposable
    {
        public event EventHandler Disposing;

        public IExchangeProperty ExchangeProperty { get; set; }
        public IQueue UsingQueue { get; set; }

        public string RoutingKey { get; set; }
        public bool IsMandatory { get; set; } = false;

        protected void Publish(byte[] body, IBasicProperties properties)
        {
            if (this.UsingQueue.Channel == null)
                throw new InvalidOperationException(
                    "the queue is not yet declared"
                );

            var exchangeName = this.ExchangeProperty?.Name ?? "";
            var routingKey = this.RoutingKey ?? this.UsingQueue.Name;
            properties = this.GetOrCreateProperties(properties);

            this.UsingQueue.Channel
                .BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: this.IsMandatory,
                    basicProperties: properties,
                    body: body
                );
        }

        protected IBasicProperties GetOrCreateProperties(
            IBasicProperties properties
        ) => properties ?? this.UsingQueue.Channel.CreateBasicProperties();

        public void Dispose()
        {
            this.Disposing?.Invoke(this, null);

            this.UsingQueue.Dispose();
        }
    }

    public abstract class BaseBasicPublisher<TOutput> : BaseBasicPublisher
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
        public TOutput DefaultValue { get; set; }

        public ISerializer<TOutput> OutputSerializer { get; set; }

        public abstract void OnReceiveError(Exception exception);

        protected virtual TOutput Process(
            byte[] body,
            IBasicProperties properties
        )
        {
            var replyTo = this.UsingQueue.Channel.QueueDeclare(
                queue: string.Empty,
                durable: false,
                exclusive: false,
                autoDelete: true
            ).QueueName;

            properties = this.GetOrCreateProperties(properties);
            properties.ReplyTo = replyTo;

            this.Publish(body, properties);
            return this.Receive(replyTo);
        }

        protected TOutput Receive(string replyTo)
        {
            var responsedEvent = new AutoResetEvent(false);
            var output = this.DefaultValue;

            var isTimedOut = false;
            var channel = this.UsingQueue.Channel;
            var message = null as BasicGetResult;

            Task.Delay(this.Timeout)
                .ContinueWith(task => isTimedOut = true);
            Task.Run(() =>
            {
                while (message == null)
                {
                    if (isTimedOut) break;
                    message = channel.BasicGet(replyTo, true);
                }

                if (message != null)
                {
                    try
                    {
                        output = this.OutputSerializer
                            .FromBytes(message.Body);
                    }
                    catch (Exception ex)
                    {
                        this.OnReceiveError(ex);
                    }
                }

                responsedEvent.Set();
            });

            responsedEvent.WaitOne();

            channel.QueueDelete(replyTo, false, false);
            return output;
        }
    }
}