using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Tests.Abstracts
{
    public class BaseTest : IDisposable
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
        #endregion

        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }
    }
}
