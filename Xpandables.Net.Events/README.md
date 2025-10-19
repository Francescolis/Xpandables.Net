# 📝 Xpandables.Net.Events

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Sourcing & CQRS** - Complete event sourcing implementation with aggregate roots, event stores, domain events, and integration events.

---

## 📋 Overview

`Xpandables.Net.Events` provides a production-ready event sourcing framework that enables building applications using Domain-Driven Design (DDD) and Event Sourcing patterns. Store state as a sequence of events rather than current state.

### 🎯 Key Features

- 🎯 **Aggregate Roots** - Event-sourced aggregates with business logic
- 🎯 **Event Store** - Persistent event storage and retrieval
- 🎯 **Event Replay** - Rebuild state from historical events
- 🎯 **Domain Events** - Internal domain event handling
- 🎯 **Integration Events** - Cross-boundary event communication
- 🎯 **Snapshots** - Performance optimization for large streams
- 🎯 **Optimistic Concurrency** - Stream versioning support

---

## 📦 Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.Events
dotnet add package Xpandables.Net.Events.EntityFramework
```

### Basic Usage

```csharp
// 1. Define events - inherit from DomainEvent base record
public sealed record OrderCreatedEvent : DomainEvent
{
    public required Guid CustomerId { get; init; }
    public required decimal Total { get; init; }
}

public sealed record OrderItemAddedEvent : DomainEvent
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal Price { get; init; }
}

// 2. Create an aggregate
public sealed class Order : Aggregate
{
    private Guid _customerId;
    private decimal _total;
    private List<OrderItem> _items = [];
    
    // Register event handlers in constructor
    public Order()
    {
        On<OrderCreatedEvent>(Apply);
        On<OrderItemAddedEvent>(Apply);
    }
    
    // Create new order
    public static Order Create(Guid orderId, Guid customerId)
    {
        var order = new Order();
        var @event = new OrderCreatedEvent
        {
            StreamId = orderId,
            StreamName = nameof(Order),
            CustomerId = customerId,
            Total = 0
        };
        order.PushVersioningEvent(@event);
        return order;
    }
    
    // Add item
    public void AddItem(Guid productId, int quantity, decimal price)
    {
        var @event = new OrderItemAddedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(Order),
            ProductId = productId,
            Quantity = quantity,
            Price = price
        };
        PushVersioningEvent(@event);
    }
    
    // Event handlers - rebuild state
    private void Apply(OrderCreatedEvent @event)
    {
        _customerId = @event.CustomerId;
        _total = @event.Total;
    }
    
    private void Apply(OrderItemAddedEvent @event)
    {
        _items.Add(new OrderItem(
            @event.ProductId,
            @event.Quantity,
            @event.Price));
        _total += @event.Quantity * @event.Price;
    }
}

// 3. Use the aggregate
var order = Order.Create(Guid.NewGuid(), customerId);
order.AddItem(productId, quantity: 2, price: 19.99m);

await _aggregateStore.AppendAsync(order);
```

---

## 🏗️ Core Concepts

### Domain Events

Events represent facts that have happened in your domain. They inherit from the `DomainEvent` base record.

```csharp
public sealed record BankAccountCreatedEvent : DomainEvent
{
    public required string AccountNumber { get; init; }
    public required string Owner { get; init; }
    public required decimal InitialBalance { get; init; }
}

public sealed record MoneyDepositedEvent : DomainEvent
{
    public required decimal Amount { get; init; }
}

public sealed record MoneyWithdrawnEvent : DomainEvent
{
    public required decimal Amount { get; init; }
}
```

### Aggregates

Aggregates encapsulate business logic and maintain state through events.

```csharp
public sealed class BankAccount : Aggregate
{
    private string _accountNumber = string.Empty;
    private string _owner = string.Empty;
    private decimal _balance;
    
    // Register event handlers in constructor
    public BankAccount()
    {
        On<BankAccountCreatedEvent>(Apply);
        On<MoneyDepositedEvent>(Apply);
        On<MoneyWithdrawnEvent>(Apply);
    }
    
    // Public read-only properties
    public string AccountNumber => _accountNumber;
    public decimal Balance => _balance;
    
    public static BankAccount Create(Guid streamId, string accountNumber, string owner, decimal initialBalance)
    {
        var account = new BankAccount();
        var @event = new BankAccountCreatedEvent
        {
            StreamId = streamId,
            StreamName = nameof(BankAccount),
            AccountNumber = accountNumber,
            Owner = owner,
            InitialBalance = initialBalance
        };
        account.PushVersioningEvent(@event);
        return account;
    }
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        var @event = new MoneyDepositedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(BankAccount),
            Amount = amount
        };
        PushVersioningEvent(@event);
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        if (_balance < amount)
            throw new InvalidOperationException("Insufficient funds");
        
        var @event = new MoneyWithdrawnEvent
        {
            StreamId = StreamId,
            StreamName = nameof(BankAccount),
            Amount = amount
        };
        PushVersioningEvent(@event);
    }
    
    // Event handlers - apply state changes
    private void Apply(BankAccountCreatedEvent @event)
    {
        _accountNumber = @event.AccountNumber;
        _owner = @event.Owner;
        _balance = @event.InitialBalance;
    }
    
    private void Apply(MoneyDepositedEvent @event)
    {
        _balance += @event.Amount;
    }
    
    private void Apply(MoneyWithdrawnEvent @event)
    {
        _balance -= @event.Amount;
    }
    
    // Optional: Track significant business events
    protected override bool IsSignificantBusinessEvent(IDomainEvent domainEvent)
    {
        return domainEvent is BankAccountCreatedEvent 
            or MoneyDepositedEvent 
            or MoneyWithdrawnEvent;
    }
}
```

### Event Store Operations

```csharp
// Append events
var account = BankAccount.Create(Guid.NewGuid(), "ACC001", "John Doe", 1000m);
account.Deposit(500m);
await _aggregateStore.AppendAsync(account);

// Load aggregate from events
var loadedAccount = await _aggregateStore.ReadAsync<BankAccount>(accountId);

// Read events from stream
var events = await _eventStore.ReadAsync(new ReadStreamRequest
{
    StreamId = accountId,
    FromVersion = 0
});

await foreach (var envelope in events)
{
    Console.WriteLine($"Event: {envelope.Event.GetType().Name}");
}
```

---

## 💎 Advanced Features

### Integration Events

Integration events enable communication across bounded contexts. Inherit from `IntegrationEvent`.

```csharp
// Define integration event
public sealed record OrderShippedIntegrationEvent : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string TrackingNumber { get; init; }
}

// Publish from application service
public sealed class OrderService
{
    private readonly IAggregateStore _aggregateStore;
    private readonly IPublisher _publisher;
    
    public async Task ShipOrderAsync(Guid orderId, string trackingNumber)
    {
        var order = await _aggregateStore.ReadAsync<Order>(orderId);
        order.Ship(trackingNumber);
        
        await _aggregateStore.AppendAsync(order);
        
        // Publish integration event for external systems
        await _publisher.PublishAsync(new OrderShippedIntegrationEvent
        {
            OrderId = orderId,
            CustomerId = order.CustomerId,
            TrackingNumber = trackingNumber
        });
    }
}

// Integration events are stored in outbox for reliable publishing
```

### Event Subscriptions

```csharp
// Subscribe to specific stream
await _eventStore.SubscribeToStreamAsync(new SubscribeToStreamRequest
{
    StreamId = orderId,
    OnEvent = async (envelope, ct) =>
    {
        Console.WriteLine($"New event: {envelope.Event.GetType().Name}");
        await ProcessEventAsync(envelope.Event);
    }
});

// Subscribe to all streams
await _eventStore.SubscribeToAllStreamsAsync(new SubscribeToAllStreamsRequest
{
    OnEvent = async (envelope, ct) =>
    {
        await UpdateReadModelAsync(envelope);
    }
});
```

---

## 💡 Complete Example: E-Commerce Order

```csharp
// Events
public sealed record OrderCreatedEvent : DomainEvent
{
    public required Guid CustomerId { get; init; }
}

public sealed record OrderItemAddedEvent : DomainEvent
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}

public sealed record OrderShippedEvent : DomainEvent
{
    public required string TrackingNumber { get; init; }
}

// Aggregate
public sealed class Order : Aggregate
{
    public enum OrderStatus { Created, Shipped, Delivered, Cancelled }
    
    private Guid _customerId;
    private List<OrderItem> _items = [];
    private OrderStatus _status;
    private string? _trackingNumber;
    
    public Order()
    {
        On<OrderCreatedEvent>(Apply);
        On<OrderItemAddedEvent>(Apply);
        On<OrderShippedEvent>(Apply);
    }
    
    public Guid CustomerId => _customerId;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(i => i.Quantity * i.UnitPrice);
    
    public static Order Create(Guid orderId, Guid customerId)
    {
        var order = new Order();
        var @event = new OrderCreatedEvent
        {
            StreamId = orderId,
            StreamName = nameof(Order),
            CustomerId = customerId
        };
        order.PushVersioningEvent(@event);
        return order;
    }
    
    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (_status != OrderStatus.Created)
            throw new InvalidOperationException("Cannot modify shipped order");
        
        var @event = new OrderItemAddedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(Order),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
        PushVersioningEvent(@event);
    }
    
    public void Ship(string trackingNumber)
    {
        if (_status != OrderStatus.Created)
            throw new InvalidOperationException("Order already shipped");
        
        if (!_items.Any())
            throw new InvalidOperationException("Cannot ship empty order");
        
        var @event = new OrderShippedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(Order),
            TrackingNumber = trackingNumber
        };
        PushVersioningEvent(@event);
    }
    
    private void Apply(OrderCreatedEvent @event)
    {
        _customerId = @event.CustomerId;
        _status = OrderStatus.Created;
    }
    
    private void Apply(OrderItemAddedEvent @event)
    {
        _items.Add(new OrderItem(
            @event.ProductId,
            @event.Quantity,
            @event.UnitPrice));
    }
    
    private void Apply(OrderShippedEvent @event)
    {
        _status = OrderStatus.Shipped;
        _trackingNumber = @event.TrackingNumber;
    }
}

// Usage
public sealed class OrderService
{
    private readonly IAggregateStore _aggregateStore;
    
    public async Task<Guid> CreateOrderAsync(
        Guid customerId,
        List<OrderItemDto> items)
    {
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, customerId);
        
        foreach (var item in items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }
        
        await _aggregateStore.AppendAsync(order);
        
        return orderId;
    }
    
    public async Task ShipOrderAsync(Guid orderId, string trackingNumber)
    {
        var order = await _aggregateStore.ReadAsync<Order>(orderId);
        
        order.Ship(trackingNumber);
        
        await _aggregateStore.AppendAsync(order);
    }
}
```

---

## 🗄️ Entity Framework Integration

See [`Xpandables.Net.Events.EntityFramework`](../Xpandables.Net.Events.EntityFramework/README.md) for EF Core event store implementation.

---

## 💡 Best Practices

1. **Events are Immutable**: Never modify events after creation - use records with `init` properties
2. **Small, Focused Events**: One event per business fact
3. **Version Your Events**: Plan for schema evolution
4. **Register Handlers in Constructor**: Use `On<TEvent>()` in the aggregate constructor
5. **Use PushVersioningEvent()**: Let the framework handle versioning automatically
6. **Set StreamId and StreamName**: Always include these properties on events
7. **Optimistic Concurrency**: Handle version conflicts gracefully
8. **Separate Commands from Events**: Commands express intent, events express facts
9. **Test Event Handlers**: Unit test event application logic independently
10. **Use Integration Events**: For cross-boundary communication

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

