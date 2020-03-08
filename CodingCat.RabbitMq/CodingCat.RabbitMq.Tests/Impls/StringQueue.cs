namespace CodingCat.RabbitMq.Tests.Impls
{
    public class StringQueue : BasicQueue
    {
        #region Constructor(s)

        public StringQueue() : base()
        {
            this.BindingKey = nameof(StringQueue);
            this.IsAutoDelete = false;
            this.IsDurable = false;
            this.IsExclusive = false;
            this.Name = nameof(StringQueue);
        }

        #endregion Constructor(s)
    }
}