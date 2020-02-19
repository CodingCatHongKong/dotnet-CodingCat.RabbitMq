namespace CodingCat.RabbitMq.Interfaces
{
    public interface IQueueProperty : IDeclarableProperty
    {
        string BindingKey { get; }
        bool IsExclusive { get; }
    }
}