using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Tests.Impls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingCat.RabbitMq.Tests
{
    [TestClass]
    public class TestDiExtensions : IDisposable
    {
        public IServiceProvider Provider { get; }

        #region Constructor(s)

        public TestDiExtensions()
        {
            this.Provider = new ServiceCollection()
                .AddSingleton(Constants.ConnectConfiguration)
                .AddSingleConnection(new ConnectionFactory()
                {
                    Uri = new Uri(Constants.CONNECTION_STRING)
                })
                .AddExchange(new DirectExchange())
                .AddExchange(new FanoutExchange())
                .AddQueue(new StringQueue())
                .AddQueue(new IntQueue())
                .AddFactory(provider =>
                    new StringSubscriberFactory(
                        provider.RequireQueue<StringQueue>(),
                        new DelegatedProcessor<string>(val =>
                            Console.WriteLine(val)
                        )
                    )
                )
                .AddFactory(provider =>
                    new IntSubscriberFactory(
                        provider.RequireQueue<IntQueue>(),
                        new DelegatedProcessor<int, int>(val => val + 1)
                    )
                )
                .BuildServiceProvider();
        }

        #endregion Constructor(s)

        [TestMethod]
        public void Test_Injected_AreResolved()
        {
            // Arrange
            using (var channel = this.Provider.RequireRabbitMqChannel())
            {
                foreach (var exchange in this.Provider.ResolveExchanges())
                    exchange.Declare(channel);
                foreach (var queue in this.Provider.ResolveQueues())
                    queue.Declare(channel);

                channel.Close();
            }

            // Act
            this.Provider.RequireExchange<DirectExchange>();
            this.Provider.RequireExchange<FanoutExchange>();

            var subscribers = this.Provider
                .GetRequiredService<IEnumerable<ISubscriberFactory>>()
                .Select(factory => factory
                    .GetSubscribed(this.Provider.RequireRabbitMqChannel())
                )
                .ToArray();

            // Assert
            Assert.AreEqual(2, subscribers.Count());

            foreach (var subscriber in subscribers)
                subscriber?.Dispose();
        }

        public void Dispose()
        {
            using (var connection = this.Provider.RequireRabbitMqConnection())
            {
                using (var channel = this.Provider.RequireRabbitMqChannel())
                {
                    foreach (var exchange in this.Provider.ResolveExchanges())
                        channel.ExchangeDelete(exchange.Name, false);

                    foreach (var queue in this.Provider.ResolveQueues())
                        channel.QueueDelete(queue.Name, false, false);
                }

                connection.Close();
            }
        }
    }
}