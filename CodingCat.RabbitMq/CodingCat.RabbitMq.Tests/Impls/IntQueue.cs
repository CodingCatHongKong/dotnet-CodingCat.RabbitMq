namespace CodingCat.RabbitMq.Tests.Impls
{
    public class IntQueue : BasicQueue
    {
        #region Constructor(s)

        public IntQueue() : base()
        {
            this.BindingKey = nameof(IntQueue);
            this.IsAutoDelete = false;
            this.IsDurable = false;
            this.IsExclusive = false;
            this.Name = nameof(IntQueue);
        }

        #endregion Constructor(s)
    }
}