using CodingCat.RabbitMq.Abstractions;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class DirectExchange : BasicExchange
    {
        #region Constructor(s)

        public DirectExchange() : base()
        {
            this.ExchangeType = ExchangeTypes.Direct;
            this.IsAutoDelete = false;
            this.IsDurable = false;
            this.Name = nameof(DirectExchange);
        }

        #endregion Constructor(s)
    }
}