# ?? System.Events

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Abstractions** - Core interfaces and base types for domain events, integration events, event publishing/subscribing, and event sourcing patterns.

---

## ?? Overview

`System.Events` provides foundational abstractions for event-driven architectures, including domain events, integration events, pub/sub patterns, and aggregate roots for event sourcing.

### ? Key Features

- ?? **IEvent** - Base event interface with EventId and OccurredOn
- ?? **IDomainEvent** - Domain events for within bounded context
- ?? **IIntegrationEvent** - Events for cross-service communication
- ?? **IEventPublisher** - Publish events to subscribers
- ?? **IEventSubscriber** - Subscribe to events with handlers
- ?? **IAggregate** - Aggregate root for event sourcing
- ?? **IEventStore** - Store and retrieve domain events
- ?? **IOutboxStore** - Outbox pattern for reliable event delivery

---

## ?? Quick Start

### Installation

```bash
dotnet add package System.Events
```

### Domain Events

```csharp
using System.Events.Domain;

// Define domain event
public sealed record UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

// Publish event
await _publisher.PublishAsync(new UserCreatedEvent
{
    UserId = user.Id,
    Name = user.Name,
    Email = user.Email
});

// Subscribe to event
_subscriber.Subscribe<UserCreatedEvent>(evt =>
    Console.WriteLine($"User created: {evt.Name}"));
```

### Integration Events

```csharp
using System.Events.Integration;

// Integration event wraps domain event for external systems
public sealed record UserCreatedIntegrationEvent
    : IntegrationEvent<UserCreatedEvent>
{
    public UserCreatedIntegrationEvent(UserCreatedEvent domainEvent)
        : base(domainEvent) { }
}

// Publish to external systems
await _publisher.PublishAsync(new UserCreatedIntegrationEvent(domainEvent));
```

### Event Sourcing with Aggregates

```csharp
using System.Events.Aggregates;

public sealed class OrderAggregate : Aggregate
{
    public string OrderNumber { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    
    public static OrderAggregate Create(string orderNumber, decimal amount)
    {
        var aggregate = new OrderAggregate();
        aggregate.AppendEvent(new OrderCreatedEvent
        {
            OrderNumber = orderNumber,
            Amount = amount
        });
        return aggregate;
    }
    
    public void AddItem(string productId, decimal price)
    {
        AppendEvent(new ItemAddedEvent
        {
            ProductId = productId,
            Price = price
        });
    }
    
    // Event handlers
    private void On(OrderCreatedEvent evt)
    {
        OrderNumber = evt.OrderNumber;
        TotalAmount = evt.Amount;
    }
    
    private void On(ItemAddedEvent evt)
    {
        TotalAmount += evt.Price;
    }
}
```

---

## ?? Core Interfaces

### IEvent

```csharp
public interface IEvent
{
    DateTimeOffset OccurredOn { get; init; }
    Guid EventId { get; init; }
    string GetEventName();
}
```

### IDomainEvent / IIntegrationEvent

```csharp
public interface IDomainEvent : IEvent { }
public interface IIntegrationEvent : IEvent { }
```

### IEventPublisher

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
}
```

### IEventSubscriber

```csharp
public interface IEventSubscriber
{
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent;
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class, IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent;
    bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent;
    IDisposable SubscribeDisposable<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent;
}
```

---

## ?? Best Practices

1. **Use DomainEvent for internal** - Within bounded context
2. **Use IntegrationEvent for external** - Cross-service communication
3. **Immutable events** - Use records for events
4. **Event versioning** - Include version information
5. **Idempotent handlers** - Handle duplicate events gracefully

---

## ?? Related Packages

- **System.Events.EntityFramework** - EF Core event store implementation
- **System.ExecutionResults** - ExecutionResult integration
- **System.Data.Repositories** - Repository abstractions

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
