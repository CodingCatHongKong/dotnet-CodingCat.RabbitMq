using CodingCat.RabbitMq.Interfaces;
using CodingCat.Serializers.Interfaces;
using System;

namespace CodingCat.RabbitMq.PubSub.Interfaces
{
    public interface IPubSub : IDisposable
    {
        event EventHandler Disposing;

        IQueue UsingQueue { get; }
    }

    public interface IPubSub<TIntput>
    {
        ISerializer<TIntput> InputSerializer { get; }
    }

    public interface IPubSub<TInput, TOutput> : IPubSub<TInput>
    {
        ISerializer<TOutput> OutputSerializer { get; }
    }
}
