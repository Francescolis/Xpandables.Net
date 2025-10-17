# ?? Xpandables.Net.Events

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Sourcing & CQRS** - Complete event sourcing implementation with aggregate roots, event stores, domain events, and integration events.

---

## ?? Overview

`Xpandables.Net.Events` provides a production-ready event sourcing framework that enables building applications using Domain-Driven Design (DDD) and Event Sourcing patterns. Store state as a sequence of events rather than current state.

### ? Key Features

- ?? **Aggregate Roots** - Event-sourced aggregates with business logic
- ?? **Event Store** - Persistent event storage and retrieval
- ?? **Event Replay** - Rebuild state from historical events
- ?? **Domain Events** - Internal domain event handling
- ?? **Integration Events** - Cross-boundary event communication
- ?? **Snapshots** - Performance optimization for large streams
- ?? **Optimistic Concurrency** - Stream versioning support

---

## ?? Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.Events
dotnet add package Xpandables.Net.Events.EntityFramework
```

### Basic Usage

```csharp
// 1. Define events
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Total) : IDomainEvent;

public sealed record OrderItemAddedEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal Price) : IDomainEvent;

// 2. Create an aggregate
public sealed class Order : Aggregate
{
    private Guid _customerId;
    private decimal _total;
    private List<OrderItem> _items = [];
    
    // For reconstitution
    private Order() { }
    
    // Create new order
    public static Order Create(Guid customerId)
    {
        var order = new Order();
        order.RaiseEvent(new OrderCreatedEvent(
            Guid.NewGuid(),
            customerId,
            0));
        return order;
    }
    
    // Add item
    public void AddItem(Guid productId, int quantity, decimal price)
    {
        RaiseEvent(new OrderItemAddedEvent(
            StreamId,
            productId,
            quantity,
            price));
    }
    
    // Event handlers (rebuild state)
    protected override void On(IEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent created:
                StreamId = created.OrderId;
                _customerId = created.CustomerId;
                _total = created.Total;
                break;
                
            case OrderItemAddedEvent itemAdded:
                _items.Add(new OrderItem(
                    itemAdded.ProductId,
                    itemAdded.Quantity,
                    itemAdded.Price));
                _total += itemAdded.Quantity * itemAdded.Price;
                break;
        }
    }
}

// 3. Use the aggregate
var order = Order.Create(customerId);
order.AddItem(productId, quantity: 2, price: 19.99m);

await _aggregateStore.AppendAsync(order);
```

---

## ?? Core Concepts

### Aggregates

```csharp
public sealed class BankAccount : Aggregate
{
    private decimal _balance;
    private bool _isActive;
    
    private BankAccount() { } // For reconstitution
    
    public static BankAccount Open(string accountNumber, decimal initialDeposit)
    {
        var account = new BankAccount();
        account.RaiseEvent(new AccountOpenedEvent(
            Guid.NewGuid(),
            accountNumber,
            initialDeposit));
        return account;
    }
    
    public void Deposit(decimal amount)
    {
        if (!_isActive)
            throw new InvalidOperationException("Account is closed");
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        RaiseEvent(new MoneyDepositedEvent(StreamId, amount));
    }
    
    public void Withdraw(decimal amount)
    {
        if (!_isActive)
            throw new InvalidOperationException("Account is closed");
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        if (_balance < amount)
            throw new InvalidOperationException("Insufficient funds");
        
        RaiseEvent(new MoneyWithdrawnEvent(StreamId, amount));
    }
    
    protected override void On(IEvent @event)
    {
        switch (@event)
        {
            case AccountOpenedEvent opened:
                StreamId = opened.AccountId;
                _balance = opened.InitialDeposit;
                _isActive = true;
                break;
                
            case MoneyDepositedEvent deposited:
                _balance += deposited.Amount;
                break;
                
            case MoneyWithdrawnEvent withdrawn:
                _balance -= withdrawn.Amount;
                break;
                
            case AccountClosedEvent:
                _isActive = false;
                break;
        }
    }
}
```

### Event Store Operations

```csharp
// Append events
var order = Order.Create(customerId);
order.AddItem(productId, 2, 19.99m);
await _aggregateStore.AppendAsync(order);

// Load aggregate from events
var loadedOrder = await _aggregateStore
    .ReadAsync<Order>(orderId);

// Read events from stream
var events = await _eventStore.ReadAsync(new ReadStreamRequest
{
    StreamId = orderId,
    FromVersion = 0
});

await foreach (var envelope in events)
{
    Console.WriteLine($"Event: {envelope.Event.GetType().Name}");
}
```

---

## ?? Advanced Features

### Snapshots for Performance

```csharp
public sealed record OrderSnapshot(
    Guid OrderId,
    Guid CustomerId,
    decimal Total,
    List<OrderItem> Items) : ISnapshotEvent;

public sealed class Order : Aggregate
{
    protected override void On(IEvent @event)
    {
        if (@event is OrderSnapshot snapshot)
        {
            StreamId = snapshot.OrderId;
            _customerId = snapshot.CustomerId;
            _total = snapshot.Total;
            _items = snapshot.Items;
            return;
        }
        
        // Handle other events...
    }
    
    protected override ISnapshotEvent? CreateSnapshot()
    {
        return new OrderSnapshot(
            StreamId,
            _customerId,
            _total,
            _items);
    }
}

// Snapshots are created automatically every N events
```

### Integration Events

```csharp
// Define integration event
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Total) : IIntegrationEvent;

// Raise from aggregate
public sealed class Order : Aggregate
{
    public static Order Create(Guid customerId)
    {
        var order = new Order();
        order.RaiseEvent(new OrderCreatedEvent(...));
        
        // Raise integration event for external systems
        order.RaiseIntegrationEvent(new OrderCreatedIntegrationEvent(
            order.StreamId,
            customerId,
            0));
        
        return order;
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

## ?? Complete Example: E-Commerce Order

```csharp
// Events
public sealed record OrderCreatedEvent(Guid OrderId, Guid CustomerId) 
    : IDomainEvent;

public sealed record OrderItemAddedEvent(
    Guid OrderId, Guid ProductId, int Quantity, decimal UnitPrice) 
    : IDomainEvent;

public sealed record OrderShippedEvent(
    Guid OrderId, string TrackingNumber) 
    : IDomainEvent;

// Aggregate
public sealed class Order : Aggregate
{
    public enum OrderStatus { Created, Shipped, Delivered, Cancelled }
    
    private Guid _customerId;
    private List<OrderItem> _items = [];
    private OrderStatus _status;
    private string? _trackingNumber;
    
    private Order() { }
    
    public static Order Create(Guid customerId)
    {
        var order = new Order();
        order.RaiseEvent(new OrderCreatedEvent(Guid.NewGuid(), customerId));
        return order;
    }
    
    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (_status != OrderStatus.Created)
            throw new InvalidOperationException("Cannot modify shipped order");
        
        RaiseEvent(new OrderItemAddedEvent(
            StreamId, productId, quantity, unitPrice));
    }
    
    public void Ship(string trackingNumber)
    {
        if (_status != OrderStatus.Created)
            throw new InvalidOperationException("Order already shipped");
        
        if (!_items.Any())
            throw new InvalidOperationException("Cannot ship empty order");
        
        RaiseEvent(new OrderShippedEvent(StreamId, trackingNumber));
        
        // Raise integration event
        RaiseIntegrationEvent(new OrderShippedIntegrationEvent(
            StreamId, trackingNumber, _customerId));
    }
    
    protected override void On(IEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent created:
                StreamId = created.OrderId;
                _customerId = created.CustomerId;
                _status = OrderStatus.Created;
                break;
                
            case OrderItemAddedEvent itemAdded:
                _items.Add(new OrderItem(
                    itemAdded.ProductId,
                    itemAdded.Quantity,
                    itemAdded.UnitPrice));
                break;
                
            case OrderShippedEvent shipped:
                _status = OrderStatus.Shipped;
                _trackingNumber = shipped.TrackingNumber;
                break;
        }
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
        var order = Order.Create(customerId);
        
        foreach (var item in items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }
        
        await _aggregateStore.AppendAsync(order);
        
        return order.StreamId;
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

## ?? Entity Framework Integration

See [`Xpandables.Net.Events.EntityFramework`](../Xpandables.Net.Events.EntityFramework/README.md) for EF Core event store implementation.

---

## ?? Best Practices

1. **Events are Immutable**: Never modify events after creation
2. **Small, Focused Events**: One event per business fact
3. **Version Your Events**: Plan for schema evolution
4. **Use Snapshots**: For aggregates with many events
5. **Optimistic Concurrency**: Handle version conflicts

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
