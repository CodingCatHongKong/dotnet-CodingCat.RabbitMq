using CodingCat.RabbitMq.Abstractions;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class FanoutExchange : BasicExchange
    {
        #region Constructor(s)

        public FanoutExchange() : base()
        {
            this.ExchangeType = ExchangeTypes.Fanout;
            this.IsAutoDelete = false;
            this.IsDurable = false;
            this.Name = nameof(FanoutExchange);
        }

        #endregion Constructor(s)
    }
}