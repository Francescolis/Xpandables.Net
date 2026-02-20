# System.Events

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Events.svg)](https://www.nuget.org/packages/Xpandables.Events)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Events.svg)](https://www.nuget.org/packages/Xpandables.Events)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Event sourcing and domain events infrastructure for building event-driven applications.

## Overview

`System.Events` is the core package for building event-sourced, event-driven applications. It provides aggregates with uncommitted event queues, domain and integration events, publisher/subscriber abstractions, outbox/inbox patterns, and correlation/causation propagation.

Built for .NET 10 with full async support.

## Features

### Core Events
- **`IEvent`** — Base event interface with `EventId`, `OccurredOn`, `CausationId`, `CorrelationId`
- **`IDomainEvent`** — Domain events with `StreamId`, `StreamVersion`, `StreamName`, metadata
- **`IIntegrationEvent`** — Integration events for cross-service communication
- **`ISnapshotEvent`** — Snapshot events for aggregate state

### Aggregates
- **`Aggregate`** — Base class for event-sourced aggregates with event replay
- **`IAggregateFactory<T>`** — Factory interface for aggregate creation
- **`IAggregateStore`** — Load and save aggregates with event persistence
- **`SnapshotStore`** — Snapshot management for aggregates
- **`SnapshotOptions`** — Configure snapshot frequency and storage

### Event Stores
- **`IEventStore`** — Event store abstraction (append, read, subscribe, truncate, delete)
- **`ISnapshotEventStore`** — Snapshot-specific event store operations
- **`IEventSubscriber`** — Subscribe to event streams

### Publisher/Subscriber
- **`IEventPublisher`** — Publish events to handlers
- **`IEventHandler<TEvent>`** — Handle specific event types
- **`IEventHandlerRegistry`** — Registry for event handlers
- **`EventBusPublisher`** — Event bus implementation
- **`CompositeEventPublisher`** — Combine multiple publishers

### Outbox/Inbox Patterns
- **`IOutboxStore`** — Transactional outbox for reliable delivery
- **`IInboxStore`** — Inbox for exactly-once consumption (idempotency)
- **`InboxEventHandlerDecorator`** — Decorator for inbox processing

### Scheduling
- **`IScheduler`** — Background scheduler for pending events
- **`HostedScheduler`** — Background service for scheduled processing
- **`SchedulerOptions`** — Configure scheduler behavior

### Event Context
- **`EventContext`** — Current correlation/causation context
- **`IEventContextAccessor`** — Access current event context
- **`AsyncLocalEventContextAccessor`** — AsyncLocal-based accessor

### Enrichers
- **`IDomainEventEnricher`** — Enrich domain events with metadata
- **`IIntegrationEventEnricher`** — Enrich integration events
- **`IPendingDomainEventsBuffer`** — Buffer pending domain events
- **`IPendingIntegrationEventsBuffer`** — Buffer pending integration events

## Installation

```bash
dotnet add package Xpandables.Events
```

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddXEventPublisher(EventRegistryMode.Static);
services.AddXEventHandlers(typeof(Program).Assembly);
services.AddXAggregateStore<OrderAggregate>();
services.AddXHostedScheduler();
```

### Define an Aggregate

```csharp
using System.Events.Aggregates;
using System.Events.Domain;

public sealed class OrderAggregate : Aggregate, IAggregateFactory<OrderAggregate>
{
    public Guid CustomerId { get; private set; }
    public decimal Total { get; private set; }

    public OrderAggregate()
    {
        On<OrderCreated>(Apply);
        On<OrderItemAdded>(Apply);
    }

    public static OrderAggregate Create() => new();

    public void CreateOrder(Guid orderId, Guid customerId)
    {
        AppendEvent(new OrderCreated
        {
            StreamId = orderId,
            CustomerId = customerId
        });
    }

    private void Apply(OrderCreated e)
    {
        StreamId = e.StreamId;
        CustomerId = e.CustomerId;
    }

    private void Apply(OrderItemAdded e)
    {
        Total += e.Price * e.Quantity;
    }
}
```

### Define Domain Events

```csharp
public sealed record OrderCreated : IDomainEvent
{
    public required Guid StreamId { get; init; }
    public required Guid CustomerId { get; init; }
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public long StreamVersion { get; init; }
    // ... other required properties
}
```

### Handle Events

```csharp
public sealed class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public async ValueTask HandleAsync(OrderCreated @event, CancellationToken ct)
    {
        // Handle the event
    }
}
```

### Use Aggregate Store

```csharp
public class OrderService(IAggregateStore aggregateStore)
{
    public async Task CreateOrderAsync(Guid orderId, Guid customerId, CancellationToken ct)
    {
        var aggregate = OrderAggregate.Create();
        aggregate.CreateOrder(orderId, customerId);
        await aggregateStore.SaveAsync(aggregate, ct);
    }

    public async Task<OrderAggregate> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        return await aggregateStore.LoadAsync<OrderAggregate>(orderId, ct);
    }
}
```

## Core Types

| Type | Description |
|------|-------------|
| `IEvent` | Base event interface |
| `IDomainEvent` | Domain event with stream info |
| `IIntegrationEvent` | Cross-service event |
| `Aggregate` | Event-sourced aggregate base |
| `IAggregateStore` | Aggregate persistence |
| `IEventStore` | Event stream storage |
| `IEventPublisher` | Event publishing |
| `IEventHandler<T>` | Event handling |
| `IOutboxStore` | Transactional outbox |
| `IInboxStore` | Idempotent inbox |

---

### Use Aggregate Store

```csharp
using System.Events.Aggregates;

public class OrderService(IAggregateStore<OrderAggregate> aggregateStore)
{
    public async Task<Guid> CreateOrderAsync(Guid customerId, CancellationToken ct)
    {
        var orderId = Guid.CreateVersion7();
        var order = OrderAggregate.Create();
        
        order.CreateOrder(orderId, customerId);
        
        await aggregateStore.SaveAsync(order, ct);
        
        return orderId;
    }

    public async Task AddItemAsync(Guid orderId, AddItemRequest request, CancellationToken ct)
    {
        var order = await aggregateStore.LoadAsync(orderId, ct);
        
        order.AddItem(request.ProductId, request.ProductName, request.Price, request.Quantity);
        
        await aggregateStore.SaveAsync(order, ct);
    }

    public async Task ConfirmOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await aggregateStore.LoadAsync(orderId, ct);
        
        order.Confirm();
        
        await aggregateStore.SaveAsync(order, ct);
    }
}
```

---

## 📦 Event Store Operations

### IEventStore Interface

```csharp
using System.Events.Domain;

public class EventStoreService(IEventStore eventStore)
{
    // Append events to a stream
    public async Task AppendEventsAsync(Guid streamId, IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        var request = new AppendRequest
        {
            StreamId = streamId,
            Events = events.ToArray(),
            ExpectedVersion = -1 // Any version (or specific for optimistic concurrency)
        };

        AppendResult result = await eventStore.AppendToStreamAsync(request, ct);
        Console.WriteLine($"Appended {result.EventsCount} events, new version: {result.StreamVersion}");
    }

    // Read events from a stream
    public async Task<List<IDomainEvent>> ReadStreamAsync(Guid streamId, CancellationToken ct)
    {
        var request = new ReadStreamRequest
        {
            StreamId = streamId,
            FromVersion = 0 // Read from beginning
        };

        var events = new List<IDomainEvent>();
        await foreach (var envelope in eventStore.ReadStreamAsync(request, ct))
        {
            events.Add(envelope.Event);
        }

        return events;
    }

    // Read events from all streams
    public async IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(CancellationToken ct)
    {
        var request = new ReadAllStreamsRequest
        {
            FromPosition = 0
        };

        await foreach (var envelope in eventStore.ReadAllStreamsAsync(request, ct))
        {
            yield return envelope;
        }
    }

    // Check stream existence and version
    public async Task<(bool Exists, long Version)> GetStreamInfoAsync(Guid streamId, CancellationToken ct)
    {
        bool exists = await eventStore.StreamExistsAsync(streamId, ct);
        long version = await eventStore.GetStreamVersionAsync(streamId, ct);

        return (exists, version);
    }

    // Subscribe to live events
    public IAsyncDisposable SubscribeToStream(Guid streamId, CancellationToken ct)
    {
        var request = new SubscribeToStreamRequest
        {
            StreamId = streamId,
            FromVersion = 0,
            OnEvent = async (envelope, token) =>
            {
                Console.WriteLine($"Received event: {envelope.Event.GetEventName()}");
                await Task.CompletedTask;
            }
        };

        return eventStore.SubscribeToStream(request, ct);
    }

    // Delete stream
    public async Task DeleteStreamAsync(Guid streamId, bool hardDelete, CancellationToken ct)
    {
        var request = new DeleteStreamRequest
        {
            StreamId = streamId,
            HardDelete = hardDelete
        };

        await eventStore.DeleteStreamAsync(request, ct);
    }

    // Truncate stream (remove old events)
    public async Task TruncateStreamAsync(Guid streamId, long beforeVersion, CancellationToken ct)
    {
        var request = new TruncateStreamRequest
        {
            StreamId = streamId,
            BeforeVersion = beforeVersion
        };

        await eventStore.TruncateStreamAsync(request, ct);
    }
}
```

---

## 📢 Event Publishing & Handling

### Define Event Handlers

```csharp
using System.Events;

public sealed class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public async Task HandleAsync(OrderCreated eventInstance, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Order {eventInstance.StreamId} created for customer {eventInstance.CustomerId}");
        
        // Send notification, update read model, etc.
        await Task.CompletedTask;
    }
}

public sealed class OrderConfirmedHandler : IEventHandler<OrderConfirmed>
{
    public async Task HandleAsync(OrderConfirmed eventInstance, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Order {eventInstance.StreamId} confirmed at {eventInstance.OccurredOn}");
        
        // Trigger fulfillment process, send confirmation email, etc.
        await Task.CompletedTask;
    }
}
```

### Register Handlers

```csharp
// Register all handlers from assemblies
builder.Services.AddXEventHandlers(typeof(Program).Assembly);

// Or register individual handlers
builder.Services.AddXEventHandler<OrderCreated, OrderCreatedHandler>(factory: null);
builder.Services.AddXEventHandler<OrderConfirmed, OrderConfirmedHandler>(factory: null);

// With custom factory
builder.Services.AddXEventHandler<OrderCancelled, OrderCancelledHandler>(
    provider => new OrderCancelledHandler(
        provider.GetRequiredService<INotificationService>()));
```

### Publish Events

```csharp
using System.Events;

public class OrderProcessor(IEventPublisher eventPublisher)
{
    public async Task ProcessOrderAsync(OrderAggregate order, CancellationToken ct)
    {
        // Get uncommitted events from aggregate
        var events = order.DequeueUncommittedEvents();

        // Publish each event to handlers
        await eventPublisher.PublishAsync(events, ct);
    }

    // Publish single event
    public async Task NotifyOrderShippedAsync(Guid orderId, CancellationToken ct)
    {
        var @event = new OrderShipped { StreamId = orderId };
        await eventPublisher.PublishAsync(@event, ct);
    }
}
```

### Registry modes

```csharp
// Default: Per-request handler resolution
builder.Services.AddXEventPublisher(EventRegistryMode.Default);

// Static: Compile-time handler registration (fastest)
builder.Services.AddXEventPublisher(EventRegistryMode.Static);

// Dynamic: Runtime handler registration/unregistration
builder.Services.AddXEventPublisher(EventRegistryMode.Dynamic);

// Composite: Both static and dynamic registries
builder.Services.AddXEventPublisher(EventRegistryMode.Composite);
```

---

## 🔗 Integration Events & Outbox Pattern

### Define Integration Events

```csharp
using System.Events.Integration;
using System.Events.Domain;

// Simple integration event
public record OrderPlacedIntegrationEvent : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal Total { get; init; }
}

// Integration event wrapping a domain event
public record OrderConfirmedIntegrationEvent : IntegrationEvent<OrderConfirmed>
{
    public OrderConfirmedIntegrationEvent(OrderConfirmed domainEvent) 
        : base(domainEvent) { }
    
    public required string CustomerEmail { get; init; }
}
```

### Use Outbox Store

```csharp
using System.Events.Integration;

public class OrderIntegrationService(IOutboxStore outboxStore)
{
    // Enqueue events for reliable delivery
    public async Task PublishOrderPlacedAsync(OrderAggregate order, CancellationToken ct)
    {
        var integrationEvent = new OrderPlacedIntegrationEvent
        {
            OrderId = order.StreamId,
            CustomerId = order.CustomerId,
            Total = order.Total
        };

        await outboxStore.EnqueueAsync(ct, integrationEvent);
    }

    // Process pending events (called by scheduler)
    public async Task ProcessPendingEventsAsync(CancellationToken ct)
    {
        // Dequeue with visibility timeout (prevents duplicate processing)
        var events = await outboxStore.DequeueAsync(
            ct, 
            maxEvents: 10, 
            visibilityTimeout: TimeSpan.FromMinutes(5));

        var completedIds = new List<Guid>();
        var failures = new List<FailedEvent>();

        foreach (var @event in events)
        {
            try
            {
                await PublishToMessageBrokerAsync(@event, ct);
                completedIds.Add(@event.EventId);
            }
            catch (Exception ex)
            {
                failures.Add(new FailedEvent(@event.EventId, ex.Message));
            }
        }

        // Mark events as completed or failed
        if (completedIds.Count > 0)
            await outboxStore.CompleteAsync(ct, [.. completedIds]);

        if (failures.Count > 0)
            await outboxStore.FailAsync(ct, [.. failures]);
    }

    private Task PublishToMessageBrokerAsync(IIntegrationEvent @event, CancellationToken ct)
    {
        // Publish to RabbitMQ, Kafka, Azure Service Bus, etc.
        return Task.CompletedTask;
    }
}
```

### Publish integration events to an external bus

The outbox pattern ensures integration events are stored transactionally. To actually publish these events to an external
message broker, register an `IEventBus` implementation and use `AddXEventBus<TEventBus>()`.

The built-in scheduler will dequeue pending outbox events and publish them via `IEventPublisher`. When `AddXEventBus` is
registered, the publisher implementation used for `IIntegrationEvent` is `EventBusPublisher`.

```csharp
using System.Events;
using System.Events.Integration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// In-process publish/subscribe (optional)
builder.Services.AddXEventPublisher(EventRegistryMode.Static);
builder.Services.AddXEventHandlers(typeof(Program).Assembly);

// External bus publishing for integration events
builder.Services.AddXEventBus<MyEventBus>();

// Background outbox processing
builder.Services.AddXHostedScheduler();
```

Example `IEventBus` implementation (pseudo-code):

```csharp
using System.Events;
using System.Events.Integration;

public sealed class MyEventBus : IEventBus
{
    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        // Serialize + publish to RabbitMQ/Kafka/Azure Service Bus/etc.
        // Use @event.GetEventName() as a message type and @event.EventId for idempotency.
        return Task.CompletedTask;
    }
}
```

### Publish to in-process handlers and external bus (composite publisher)

If you want to publish the same event both:

- to in-process handlers (for local side effects, read models, etc.)
- and to an external bus (for cross-service communication)

register the composite publisher:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddXEventPublisher(EventRegistryMode.Static);
builder.Services.AddXEventHandlers(typeof(Program).Assembly);

builder.Services.AddXEventBus<MyEventBus>();

// Make IEventPublisher fan-out to all registered publishers
builder.Services.AddXCompositeEventPublisher();

builder.Services.AddXHostedScheduler();
```

With this setup, the hosted scheduler processes the outbox and calls `IEventPublisher.PublishAsync(@event)`.
The composite publisher will execute both the in-process dispatch and bus publish.

---

## 📥 Inbox Pattern (Exactly-Once Consumption)

The **Inbox pattern** complements the Outbox pattern by ensuring **exactly-once** processing of incoming integration events. It prevents duplicate handling when events are delivered multiple times (at-least-once delivery).

### How It Works

1. When an integration event arrives, the inbox records it with a `(EventId, Consumer)` key
2. If the event was already processed (status = `PUBLISHED`), handling is skipped
3. If processing is in progress (status = `PROCESSING`), handling is skipped
4. On success, the event is marked as `PUBLISHED`
5. On failure, the event is marked as `ONERROR` with exponential backoff for retry

### Define an Inbox-Enabled Event Handler

Implement `IInboxConsumer` on your event handler to enable inbox idempotency:

```csharp
using System.Events;
using System.Events.Integration;

public sealed class OrderPlacedEventHandler : IEventHandler<OrderPlacedIntegrationEvent>, IInboxConsumer
{
    // Logical consumer name used as part of the idempotency key
    public string Consumer => "OrderService.OrderPlacedHandler";

    public async Task HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken)
    {
        // This handler is guaranteed to execute at most once per (EventId, Consumer) pair
        Console.WriteLine($"Processing order {eventInstance.OrderId}");

        // Perform side effects: update database, send notifications, etc.
        await Task.CompletedTask;
    }
}
```

### Register the Inbox Decorator

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register event handlers
builder.Services.AddXEventHandlers(typeof(Program).Assembly);

// Add inbox decorator for handlers implementing IInboxConsumer
builder.Services.AddXEventHandlerInboxDecorator();

// Register inbox store (from System.Events.Data)
builder.Services.AddXInboxStore();
```

### IInboxStore Interface

```csharp
using System.Events.Integration;

// The inbox store provides three operations:
public interface IInboxStore
{
    // Register event for processing (returns status indicating if handler should proceed)
    Task<InboxReceiveResult> ReceiveAsync(
        IIntegrationEvent @event,
        string consumer,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default);

    // Mark event as successfully processed
    Task CompleteAsync(CancellationToken cancellationToken, params CompletedInboxEvent[] events);

    // Mark event as failed (with automatic retry scheduling)
    Task FailAsync(CancellationToken cancellationToken, params FailedInboxEvent[] failures);
}
```

### Inbox Status Flow

```
┌─────────────┐     ReceiveAsync     ┌─────────────┐
│   (new)     │ ──────────────────►  │  PROCESSING │
└─────────────┘                      └──────┬──────┘
                                            │
                        ┌───────────────────┴───────────────────┐
                        │                                       │
                        ▼                                       ▼
               ┌─────────────┐                         ┌─────────────┐
               │  PUBLISHED  │◄─── CompleteAsync       │   ONERROR   │◄─── FailAsync
               │  (success)  │                         │  (retry)    │
               └─────────────┘                         └──────┬──────┘
                    │                                         │
                    │                                         │ (after visibility timeout)
                    ▼                                         ▼
               ┌─────────────┐                         ┌─────────────┐
               │  DUPLICATE  │◄─── ReceiveAsync        │  PROCESSING │◄─── ReceiveAsync (retry)
               │  (skipped)  │     (same EventId)      └─────────────┘
               └─────────────┘
```

### Why Use Both Outbox and Inbox?

| Pattern | Purpose | Guarantees |
|---------|---------|------------|
| **Outbox** | Reliable event **publishing** | At-least-once delivery |
| **Inbox** | Reliable event **consumption** | Exactly-once processing |

Together, they provide **exactly-once semantics** for distributed event-driven systems.

---

## ⏰ Background Scheduler

### Register Hosted Scheduler

```csharp
// Register default hosted scheduler
builder.Services.AddXHostedScheduler();

// Or with custom implementation
builder.Services.AddXHostedScheduler<CustomHostedScheduler>();
```

### Custom Scheduler

```csharp
using System.Events.Integration;

public sealed class OutboxProcessorScheduler : IScheduler
{
    private readonly IOutboxStore _outboxStore;
    private readonly IEventPublisher _eventPublisher;

    public OutboxProcessorScheduler(IOutboxStore outboxStore, IEventPublisher eventPublisher)
    {
        _outboxStore = outboxStore;
        _eventPublisher = eventPublisher;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var events = await _outboxStore.DequeueAsync(cancellationToken, maxEvents: 50);

        foreach (var @event in events)
        {
            await _eventPublisher.PublishAsync(@event, cancellationToken);
            await _outboxStore.CompleteAsync(cancellationToken, @event.EventId);
        }
    }
}
```

---

## 📊 Extension Methods Summary

### IServiceCollection Extensions

| Method | Description |
|--------|-------------|
| `AddXEventPublisher()` | Registers event publisher with handler registry |
| `AddXEventPublisher<T>()` | Registers custom event publisher |
| `AddXEventBus<T>()` | Registers an external bus (`IEventBus`) and a publisher for integration events |
| `AddXCompositeEventPublisher()` | Registers a composite publisher (in-process + external bus fan-out) |
| `AddXEventHandlerInboxDecorator()` | Decorates `IInboxConsumer` handlers with inbox idempotency |
| `AddXEventContext()` | Registers `IEventContextAccessor` backed by `AsyncLocal` |

---

## Correlation & causation with `EventContext`

`EventContext` is an ambient container (stored via `AsyncLocal`) used to propagate and enrich correlation/causation IDs.

**Correlation** and **causation** identifiers are now `string?` to support:
- **W3C Trace Context** (`traceparent` header, e.g., `"00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"`)
- **GUID strings** (e.g., `"550e8400-e29b-41d4-a716-446655440000"`)
- **Custom identifiers** (ULID, Snowflake, Kafka offset, etc.)

For backward compatibility, use the helper methods:
```csharp
// Parse correlation/causation as GUID if possible
if (@event.TryGetCorrelationGuidId(out var correlationGuid))
{
    // Use correlationGuid
}

if (@event.TryGetCausationGuidId(out var causationGuid))
{
    // Use causationGuid
}
```

Typical usage:

- **HTTP**: use `AspNetCore.Events` middleware to read `traceparent`/`X-Causation-Id` and establish a scope.
- **Background processing**: create a scope manually around unit-of-work execution.

```csharp
using System.Events;

public sealed class PaymentJob(IEventContextAccessor accessor)
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        // Use W3C-compatible trace ID or GUID string
        var correlationId = Guid.CreateVersion7().ToString("N");

        using var _ = accessor.BeginScope(new EventContext
        {
            CorrelationId = correlationId,
            CausationId = null
        });

        // Any domain/integration events created while inside the scope can be enriched
        // with CorrelationId/CausationId by the configured enrichers.

        await Task.Delay(50, ct);
    }
}
```
| `AddXEventHandler<TEvent, THandler>()` | Registers specific event handler |
| `AddXEventHandlers(assemblies)` | Auto-discovers and registers all handlers |
| `AddXEventSubscriber()` | Registers event subscriber |
| `AddXAggregateStore()` | Registers default aggregate store |
| `AddXAggregateStore<T>()` | Registers aggregate store for specific type |
| `AddXScheduler()` | Registers scheduler service |
| `AddXHostedScheduler()` | Registers hosted background scheduler |
| `AddXDomainEventEnricher<T>()` | Registers domain event enricher |
| `AddXIntegrationEventEnricher<T>()` | Registers integration event enricher |

---

## ✅ Best Practices

### ✅ Do

- **Inherit from `Aggregate`** — Get event replay, uncommitted events, and version tracking
- **Use `DomainEvent` record** — Get automatic `EventId`, `OccurredOn`, and versioning
- **Implement `IAggregateFactory<T>`** — Enable aggregate store to create instances
- **Use `PushVersioningEvent`** — For events that need explicit version control
- **Register handlers via assembly scan** — Use `AddXEventHandlers(assemblies)`
- **Use outbox pattern** — For reliable integration event delivery (at-least-once)
- **Use inbox pattern** — For exactly-once consumption via `IInboxConsumer`

### ❌ Don't

- **Modify aggregate state directly** — Always use events via `PushEvent()`
- **Throw exceptions in event handlers** — Handle failures gracefully with retry logic
- **Skip event versioning** — Use `StreamVersion` for optimistic concurrency
- **Ignore causation/correlation** — Track event chains with `CausationId` and `CorrelationId`

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Events.Data** | EF Core implementation of event store, outbox, and inbox |
| **AspNetCore.Events** | ASP.NET Core middleware + DI helpers for `EventContext` |
| **System.Results** | Result pattern for operation outcomes |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

