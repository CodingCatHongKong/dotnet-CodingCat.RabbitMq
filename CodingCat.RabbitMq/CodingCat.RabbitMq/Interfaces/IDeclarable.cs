using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Interfaces
{
    public interface IDeclarable<T>
    {
        T Declare(IConnection connection);
    }
}