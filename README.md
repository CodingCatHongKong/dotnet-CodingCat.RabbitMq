# CodingCat.RabbitMq

Provided the concrete classes of https://github.com/CodingCatHongKong/dotnet-CodingCat.RabbitMq.Abstractions


### Program.cs

```csharp
public class Program : BaseHostFactory
{
    public Program(
        IConnection rabbitMq,
        IInitializer rabbitMqInitializer
    ) : base(rabbitMq, rabbitMqInitializer) { }
    
    public override void Dispose()
    {
        this.Connection?.Close();
        base.Dispose();
    }

    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                var config = new MyConfiguration(context.Configuration);
                services
                    .AddSingleton<IConnectConfiguration>(config)
                    .AddSingleConnection(new ConnectionFactory()
                    {
                        Uri = new Uri(config.RabbitMqConnectionString)
                    })
                    .AddExchange(config.DirectExchange)
                    .AddQueue(config.UsersQueue)
                    .AddFactory<UsersSubscriberFactory>()
                    .AddInitializer<RabbitMqInitializer>()
                    .AddHostedService<Program>();
            })
            .UseConsoleLifetime()
            .Build();

        using (host)
        {
            host.StartAsync();
            host.WaitForShutdown();
        }

        System.Environment.Exit(0);
    }
}
```


### RabbitMqInitializer.cs

```csharp
public class RabbitMqInitializer : BaseInitializer
{
    public IExchange DirectExchange { get; }
    public IQueue UsersQueue { get; }

    #region Constructor(s)

    public RabbitMqInitializer(
        IModel channel,
        IEnumerable<ISubscriberFactory> factories,
        IEnumerable<IExchange> exchanges,
        IEnumerable<IQueue> queues,
        DirectExchange directExchange,
        UsersQueue usersQueue
    ) : base(channel, factories, exchanges, queues)
    {
        this.DirectExchange = directExchange;
        this.UsersQueue = usersQueue;
    }

    #endregion Constructor(s)

    public override IInitializer Configure(IModel channel)
    {
        using (channel)
        {
            this.UsersQueue.BindExchange(
                channel,
                this.DirectExchange.Name
            );

            channel.Close();
        }

        return this;
    }
}
```


### UsersSubscriberFactory

```csharp
public class UsersSubscriberFactory : ISubscriberFactory
{
    private IQueue serviceQueue { get; }
    private ILogger logger { get; }

    #region Constructor(s)

    public UserReaderSubscriberFactory(
        UsersQueue serviceQueue,
        ILogger<Subscriber> logger
    )
    {
        this.serviceQueue = serviceQueue;
        this.logger = logger;
    }

    #endregion Constructor(s)

    public ISubscriber GetSubscribed(IModel channel)
    {
        return new Subscriber(
            channel,
            this.serviceQueue.Name,
            this.GetProcessor()
        ).Subscribe();
    }
    
    private IProcessor GetProcessor() { ... }
}
```


### Target Frameworks

- .Net 4.6.1+
- .Net Standard 2.0+