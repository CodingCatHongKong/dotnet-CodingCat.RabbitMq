using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IExchange : IExchangeProperty
    {
        IModel Channel { get; }
    }
}