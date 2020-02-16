using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Tests.Abstracts
{
    public abstract class BaseTest : IDisposable
    {
        protected IConnection Connection { get; }

        #region Constructor(s)

        public BaseTest()
        {
            this.Connection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.CONNECTION_STRING)
            }.CreateConnection();
        }

        #endregion Constructor(s)

        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }
    }
}