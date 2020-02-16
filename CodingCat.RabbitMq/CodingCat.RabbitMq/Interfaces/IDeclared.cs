using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IDeclared
    {
        IModel Channel { get; }
    }
}