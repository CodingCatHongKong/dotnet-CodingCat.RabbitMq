using CodingCat.Mq.Abstractions;

namespace CodingCat.RabbitMq
{
    public class DelegatedProcessor<T> : BaseDelegatedProcessor<T>
    {
        #region Constructor(s)

        public DelegatedProcessor(
            ProcessDelegate delegatedProcess
        ) : base(delegatedProcess)
        {
        }

        #endregion Constructor(s)
    }

    public class DelegatedProcessor<TInput, TOutput>
        : BaseDelegatedProcessor<TInput, TOutput>
    {
        #region Constructor(s)

        public DelegatedProcessor(
            ProcessDelegate delegatedProcess
        ) : base(delegatedProcess)
        {
        }

        #endregion Constructor(s)
    }
}