using CodingCat.RabbitMq.Interfaces;
using CodingCat.RabbitMq.PubSub.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.PubSub.Abstracts
{
    public abstract class BaseBasicSubscriber :
        IPubSub,
        ISubscriber,
        ITimeoutPubSub
    {
        public const int DEFAULT_TIMEOUT_IN_SECONDS = 90;

        private string consumerTag { get; } = Guid.NewGuid().ToString();

        protected ManualResetEvent ProcessedOrTimedOutEvent { get; private set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_IN_SECONDS);

        public event EventHandler MessageCompleted;
        public event EventHandler Disposing;

        public IQueue UsingQueue { get; set; }
        public bool IsAutoAck { get; set; } = false;

        protected abstract void OnSubscribeException(
            Exception exception
        );

        protected abstract void Process(BasicDeliverEventArgs eventArgs);

        public void Subscribe()
        {
            if (this.UsingQueue.Channel == null)
                throw new InvalidOperationException(
                    "the queue is not yet declared"
                );

            var channel = this.UsingQueue.Channel;
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += this.OnReceived;
            channel.BasicConsume(
                queue: this.UsingQueue.Name,
                autoAck: this.IsAutoAck,
                consumerTag: this.consumerTag,
                consumer: consumer
            );
        }

        protected void OnReceived(
            object sender,
            BasicDeliverEventArgs eventArgs
        )
        {
            this.ProcessedOrTimedOutEvent = new ManualResetEvent(false);

            Task.Delay(this.Timeout)
                .ContinueWith(task => this.ProcessedOrTimedOutEvent.Set());
            Task.Run(() =>
            {
                try
                {
                    this.Process(eventArgs);
                }
                catch (Exception ex)
                {
                    this.OnSubscribeException(ex);
                }
            });

            this.ProcessedOrTimedOutEvent.WaitOne();
            this.MessageCompleted?.Invoke(this, null);
        }

        public void Dispose()
        {
            this.UsingQueue.Channel.BasicCancel(this.consumerTag);
            this.Disposing?.Invoke(this, null);
            this.UsingQueue.Dispose();
        }
    }

    public abstract class BaseBasicSubscriber<TInput>
        : BaseBasicSubscriber, IPubSub<TInput>, ISubscriber<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public TInput DefaultInput { get; set; } = default(TInput);

        protected abstract void Process(
            TInput input,
            BasicDeliverEventArgs eventArgs
        );

        protected override void Process(BasicDeliverEventArgs eventArgs)
        {
            var input = this.DefaultInput;

            try
            {
                input = this.InputSerializer.FromBytes(eventArgs.Body);
            }
            catch (Exception ex)
            {
                this.OnSubscribeException(ex);
            }

            this.Process(input, eventArgs);
            this.ProcessedOrTimedOutEvent.Set();
        }
    }

    public abstract class BaseBasicSubscriber<TInput, TOutput> :
        BaseBasicSubscriber,
        IPubSub<TInput, TOutput>,
        ISubscriber<TInput, TOutput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }

        public TInput DefaultInput { get; set; } = default(TInput);
        public TOutput DefaultOutput { get; set; } = default(TOutput);

        protected abstract TOutput Process(
            TInput input,
            BasicDeliverEventArgs eventArgs
        );

        protected override void Process(BasicDeliverEventArgs eventArgs)
        {
            var input = this.DefaultInput;
            var output = this.DefaultOutput;

            var channel = this.UsingQueue.Channel;
            var replyTo = eventArgs?.BasicProperties?.ReplyTo;

            try
            {
                input = this.InputSerializer.FromBytes(eventArgs.Body);
            }
            catch (Exception ex)
            {
                this.OnSubscribeException(ex);
            }

            Task.Run(() =>
            {
                output = this.Process(input, eventArgs);
                this.ProcessedOrTimedOutEvent.Set();
            });

            this.ProcessedOrTimedOutEvent.WaitOne();
            if (!string.IsNullOrEmpty(replyTo))
            {
                var body = this.OutputSerializer.ToBytes(output);
                Task.Run(() =>
                    channel.BasicPublish(
                        exchange: string.Empty,
                        routingKey: replyTo,
                        body: body
                    )
                );
            }
        }
    }
}