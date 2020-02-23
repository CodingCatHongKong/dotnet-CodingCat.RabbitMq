using CodingCat.RabbitMq.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.PubSub.Interfaces
{
    public interface IPublisher
    {
        IExchangeProperty ExchangeProperty { get; }
        string RoutingKey { get; }
        bool IsMandatory { get; }
    }

    public interface IPublisher<TInput>
    {
        void Send(TInput input, IBasicProperties properties = null);
    }

    public interface IPublisher<TInput, TOutput>
    {
        TOutput Process(TInput input, IBasicProperties properties = null);
    }
}