using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IExchange
    {
        IModel Channel { get; }
    }
}