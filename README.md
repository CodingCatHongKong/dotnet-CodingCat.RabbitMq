# CodingCat.RabbitMq

Wrapped the official RabbitMq dotnet client [https://github.com/rabbitmq/rabbitmq-dotnet-client] for easier micro-services adoption.

#### Declaring the `Exchange` & `Queue`

`IExchange` shall be declared through the `IExchangeProperty` (and `IQueue` from `IQueueProperty`) by design, as we would like to load their properties through a configuration file, when keeping the configuration classes clean and easy to read.

```csharp
// -- Program.cs
public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
  return WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, builder) =>
      builder
        .Auto<PaymentStatusConfigurations>() // -- Contains ExchangeProperty, QueueProperty
        .Auto<...>()
        ...;
    )
    .UseStartup<Startup>();
}

// -- Startup.cs
public Startup(IConfiguration configuration) {
  this.PaymentStatusExchangeProperty = configuration.Bind<PaymentStatusExchangeProperty>();      
  this.PaymentStatusQueueProperty = configuration.Bind<PaymentStatusQueueProperty>();  
  ....
}

public void ConfigureServices(IServiceCollection services) {
  services
    .AddSingleton(this.GetRabbitMqConnection())
    .AddSingleton<IPaymentStatusInquirer>(provider => {
      var rabbitMqConnection = provider.GetRequiredService<IConnection>();
      var exchange = this.PaymentStatusExchangePropert.Declare(rabbitMqConnection);
      var queue = this.PaymentStatusQueueProperty.Declare(rabbitMqConnection);
      
      return new PaymentStatusInquirer(exchange, queue);
    });
}
```

The `IQueue` could also bind with an exchange by simply `queue.Bind(exchange)`, while the binding key should already be configured in the `IQueueProperty`


#### The Basic of `IPublisher`

There are 2 types of publisher, the `IPublisher<TInput>` & `IPublisher<TInput, TOutput>` representing if there any response should be waited for, after the message is published.

The minimum information required for a publisher will be the declared `IQueue` and the `ISerializer<TInput>` for the `IPublisher<TInput>`. The `IPublisher<TInput, TOutput>` will requires an extra info `ISerializer<TOutput>` as the `OutputSerializer`.

If the messages will be published to an exchange, setting a `IExchangeProperty` and `RoutingKey` to its `ExchangeProperty` and `RoutingKey` should be fine.

```csharp
public class SmsCommandSender : BaseBasicPublisher<Commands> {
  public SmsCommandSender(
    IQueue declaredQueue,
    IExchangeProperty exchange
  ) {
    this.UsingQueue = declaredQueue;
    this.InputSerializer = new EnumSerializer<Commands>();
    
    this.ExchangeProperty = exchange;
    if (exchange != null)
      this.RoutingKey = this.UsingQueue.BindingKey; // -- assuming this is a direct exchange
  }
}

public class SmsCommandOperator : BaseBasicPublisher<Operations, bool> {
  public SmsCommandOperator(
    IQueue declaredQueue,
    IExchangeProperty exchange
  ) {
    this.UsingQueue = declaredQueue;
    this.InputSerializer = new EnumSerializer<Operations>();
    this.OutputSerializer = new BooleanSerializer();
    
    this.ExchangeProperty = exchange;
    if (exchange != null)
      this.RoutingKey = this.UsingQueue.BindingKey; // -- assuming this is a direct exchange
  }
}

// -- somewhere in the program
this.smsCommandSender.Send(Commands.NoMoreIdle);

var isAnyResponse = this.smsCommandOperator.Process(Operations.Echo);
```


#### The Basic of `ISubscriber`

Like the `IPublisher`, there are the `ISubscriber<TInput>` and `ISubscriber<TInput, TOutput>` and they requires the exact same minimum information as `IPublisher` to operate.

However, each subscriber inheriting from the `BaseBasicSubscriber` will need to take care of the below abstracts befor use:

- void OnError(Exception exception)
- void Process(TInput input, BasicDeliverEventArgs eventArgs) *`ISubscriber<TInput>`*
- TOutput Process(TInput input, BasicDeliverEventArgs eventArgs) *`ISubscriber<TInput, TOutput>`*

In order to make the `ISubscriber` to actual *subscribe* to the queue, its `Subscribe` function should be invoked

```csharp
public class PaymentStatusChecker : BaseBasicSubscriber<string, Status> {
  public PaymentStatusChecker() {
    this.IsAutoAck = true;
    this.InputSerializer = new StringSerializer();
    this.OutputSerializer = new EnumSerializer<Status>();
  }
  
  public PaymentStatusChecker(IQueue declaredQueue) : this() {
    this.UsingQueue = declaredQueue;
  }
  
  protected override void OnError(Exception exception) {
    ....
  }
  
  protected override Status Process(string orderReference, BasicDeliverEventArgs e) {
    var isPaid = false;
    var canBeFound = this.SearchCache(orderReference, ref isPaid) ||
      this.SearchDatabase(orderReference, ref isPaid) ||
      this.SearchPaymentGateway(orderReference, ref isPaid); // -- BAD DESIGN!!
      
    return canBeFound ? (isPaid ? Status.Paid : Status.NotPaid) : Status.Pending;
  }
}

// -- somewhere when the program initializing
// -- after a loads of configuraitons
this.PaymentStatusChecker.Subscribe();
```


#### Taking the full advantages of `Publisher` & `Subscriber`

The `Publisher` & `Subscriber` can serve more than mentioned. We have had encountered some cases that the client requires to be responded within a certain period of time even the output is not yet ready, or a not well formatted message was sent to the RabbitMq (most likely human error) causes the subscriber down repeatedly for infinite requeue. Thus we have included the default values and timeout to them.

If the `Publisher` or `Subscriber` are not configured for such properties, it will uses `default(T)` for both `TInput` (only available to `ISubscriber`) and `TOutput`, and the default timeout will be 90 seconds.

For only the `BaseBasicPublisher<TInput, TOutput>`, it uses the `BasicGet` internally and having a 5ms interval check for output by default.

The flow for the `Publisher<TInput, TOutput>`:
1. Publish the message
2. Check for output every {check_reply_interval}
3. Return the default value if waited for {timeout}, or the received output

The flow for the `Subscriber<TInput>`:
1. Received a message
2. Try to process the message
3. Raise the message completed event no matter processed for {timeout} or actual processed

The flow for the `Subscriber<TInput, TOutput>`:
1. Received a message
2. Try to process for the output
3. return the default value if processed for {timeout} or the actual processed output

```csharp
var userActivityLogger = new UserActivityLogger(exchange, queue, routingKey) {
  DefaultOutput = false,
  Timeout = TimeSpan.FromSeconds(5),
  CheckReplyInterval = TimeSpan.FromSeconds(1)
};

...

var userActivityLogsSubscriber = new UserActivityLogsSubscriber(queue) {
  DefaultInput = new UserActivity() {
    Username = null,
    Activity = Activities.Unknown
  },
  DefaultOutput = false,
  Timeout = TimeSpan.FromSeconds(1)
};
```


#### Bonus

A rare case when a micro-service is operating within a docker container, the RabbitMq connection is failed to create even the server is up and other services are consuming the RabbitMq happily. We have added an extension to the `IConnection` in order to support timeout and retry

```csharp
var rabbitMqConnection = new ConnectionFactory() { ... }
  .CreateConnection(
    timeoutPerTry: TimeSpan.FromSeconds(30),
    retryInterval: TimeSpan.FromSeconds(3),
    retryUpTo: 5
  );
```


#### Target Frameworks

- .Net 4.6.1+
- .Net Standard 2.0+