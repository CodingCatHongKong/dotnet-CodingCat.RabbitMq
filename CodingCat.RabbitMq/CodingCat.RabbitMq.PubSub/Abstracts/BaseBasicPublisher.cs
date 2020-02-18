using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.PubSub.Abstracts
{
    public abstract class BaseBasicPublisher : IPubSub, IPublisher
    {
        public event EventHandler Disposing;

        public IQueue UsingQueue { get; set; }
        public IExchangeProperty ExchangeProperty { get; set; }

        public string RoutingKey { get; set; } = null;
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
        )
        {
            return properties ??
                this.UsingQueue.Channel.CreateBasicProperties();
        }

        public void Dispose()
        {
            this.Disposing?.Invoke(this, null);
            this.UsingQueue.Dispose();
        }
    }

    public abstract class BaseBasicPublisher<TInput>
        : BaseBasicPublisher, IPubSub<TInput>, IPublisher<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }

        public void Send(TInput input, IBasicProperties properties = null)
        {
            var body = this.InputSerializer.ToBytes(input);
            this.Publish(body, properties);
        }
    }

    public abstract class BaseBasicPublisher<TInput, TOutput>
        : BaseBasicPublisher, IPubSub<TInput, TOutput>, IPublisher<TInput, TOutput>
    {
        public const int DEFAULT_TIMEOUT_IN_SECONDS = 90;
        public const int DEFAULT_CHECK_REPLY_INTERVAL_IN_MILLISECONDS = 5;
        
        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }
        public TOutput DefaultOutput { get; set; } = default(TOutput);

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_IN_SECONDS);
        public TimeSpan CheckReplyInterval { get; set; } = TimeSpan.FromMilliseconds(DEFAULT_CHECK_REPLY_INTERVAL_IN_MILLISECONDS);

        protected abstract void OnReceiveError(Exception exception);

        public virtual TOutput Process(
            TInput input,
            IBasicProperties properties = null
        )
        {
            var body = this.InputSerializer.ToBytes(input);
            return this.Process(body, properties);
        }

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

        protected virtual TOutput Receive(string replyTo)
        {
            var responsedEvent = new AutoResetEvent(false);
            var output = this.DefaultOutput;

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
                    Thread.Sleep(this.CheckReplyInterval);
                }

                if (message != null)
                {
                    try
                    {
                        output = this.OutputSerializer.FromBytes(
                            message.Body
                        );
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