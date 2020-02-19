namespace CodingCat.RabbitMq.Interfaces
{
    public interface IExchangeProperty : IDeclarableProperty
    {
        string Type { get; }
    }
}