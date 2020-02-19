using System;

namespace CodingCat.RabbitMq.PubSub.Interfaces
{
    public interface ISubscriber
    {
        event EventHandler MessageCompleted;

        bool IsAutoAck { get; }
    }

    public interface ISubscriber<TInput>
    {
        TInput DefaultInput { get; }
    }

    public interface ISubscriber<TInput, TOutput> : ISubscriber<TInput>
    {
    }
}