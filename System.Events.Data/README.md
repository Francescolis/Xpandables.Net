# System.Events.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Events.Data.svg)](https://www.nuget.org/packages/Xpandables.Events.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Entity Framework Core persistence for event sourcing with `System.Events`.

## Overview

`System.Events.Data` provides EF Core implementations for the event store abstractions in `System.Events`. It includes `EventDataContext` for storing domain events, snapshots, outbox, and inbox entities, along with type configurations and value converters.

Built for .NET 10 with Entity Framework Core 10.

## Features

### EventDataContext
- **`EventDataContext`** — EF Core DbContext for event storage
- `Domains` — DbSet for domain events (`EntityEventDomain`)
- `Snapshots` — DbSet for snapshot events (`EntityEventSnapshot`)
- `OutboxEvents` — DbSet for outbox events (`EntityEventOutbox`)
- `InboxEvents` — DbSet for inbox events (`EntityEventInbox`)
- Default schema: `Events`

### Repository
- **`EventRepository<TEntityEvent>`** — Generic repository for event entities

### Type Configurations
- **`EntityDomainEventTypeConfiguration`** — Domain event table configuration
- **`EntitySnapShotEventTypeConfiguration`** — Snapshot event table configuration
- **`EntityEventOutboxTypeConfiguration`** — Outbox event table configuration
- **`EntityEventInboxTypeConfiguration`** — Inbox event table configuration

### Value Converters
- **`EventJsonDocumentValueConverter`** — Convert event data to/from JSON

### Model Customizer
- **`EventStoreSqlServerModelCustomizer`** — SQL Server specific customizations

## Installation

```bash
dotnet add package Xpandables.Events.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**Dependencies:** `System.Events`, `System.Entities.Data`, `Microsoft.EntityFrameworkCore`

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

services.AddDbContext<EventDataContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.MigrationsHistoryTable("__EventStoreMigrations", "Events")));

services.AddScoped<IEventStore, EventStore>();
services.AddScoped<IOutboxStore, OutboxStore>();
services.AddScoped<IInboxStore, InboxStore>();
services.AddXEventConverterFactory();
```

### Use the Event Store

```csharp
using System.Events.Domain;

public class OrderService(IEventStore eventStore)
{
    public async Task CreateOrderAsync(Guid orderId, CancellationToken ct)
    {
        var events = new IDomainEvent[]
        {
            new OrderCreated { StreamId = orderId }
        };

        var request = new AppendRequest
        {
            StreamId = orderId,
            Events = events,
            ExpectedVersion = null
        };

        await eventStore.AppendToStreamAsync(request, ct);
        await eventStore.FlushEventsAsync(ct);
    }

    public async Task<IList<IDomainEvent>> GetEventsAsync(Guid streamId, CancellationToken ct)
    {
        var request = new ReadStreamRequest
        {
            StreamId = streamId,
            FromVersion = 0
        };

        var events = new List<IDomainEvent>();
        await foreach (var envelope in eventStore.ReadStreamAsync(request, ct))
        {
            events.Add(envelope.Event);
        }
        return events;
    }
}
```

### Database Tables

The `EventDataContext` creates the following tables in the `Events` schema:

| Table | Description |
|-------|-------------|
| `Domains` | Domain events with stream versioning |
| `Snapshots` | Aggregate snapshot events |
| `OutboxEvents` | Integration events for outbox pattern |
| `InboxEvents` | Integration events for inbox pattern |

### Apply Migrations

```bash
dotnet ef migrations add InitialEventStore --context EventDataContext
dotnet ef database update --context EventDataContext
```

## Core Types

| Type | Description |
|------|-------------|
| `EventDataContext` | EF Core DbContext for events |
| `EventRepository<T>` | Generic event entity repository |
| `EntityEventDomain` | Domain event entity |
| `EntityEventSnapshot` | Snapshot event entity |
| `EntityEventOutbox` | Outbox event entity |
| `EntityEventInbox` | Inbox event entity |

## License

Apache License 2.0
    }
}
```

### `InboxStoreDataContext`

The EF Core DbContext for the inbox pattern (exactly-once consumption):

```csharp
using System.Events.Data;

// The context stores consumed integration events to ensure exactly-once processing
// Events are tracked by (EventId, Consumer) composite key

// Register via DI
builder.Services.AddXInboxStoreDataContext(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStoreDb"),
        sql => sql.MigrationsHistoryTable("__InboxStoreMigrations")));
```

### `InboxStore`

The EF Core implementation of `IInboxStore` for idempotent event consumption:

```csharp
using System.Events.Integration;

public sealed class OrderPlacedEventHandler : IEventHandler<OrderPlacedIntegrationEvent>, IInboxConsumer
{
    private readonly IOrderService _orderService;

    // Consumer name forms part of the idempotency key (EventId + Consumer)
    public string Consumer => "OrderService.OrderPlacedHandler";

    public OrderPlacedEventHandler(IOrderService orderService)
        => _orderService = orderService;

    public async Task HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken)
    {
        // This handler is guaranteed to execute at most once per (EventId, Consumer) pair
        // The InboxEventHandlerDecorator wraps this handler automatically
        await _orderService.ProcessOrderAsync(eventInstance.OrderId, cancellationToken);
    }
}
```

**Inbox registration:**

```csharp
// Register inbox store and decorator
builder.Services.AddXInboxStore();
builder.Services.AddXEventHandlerInboxDecorator();
```

### Event converters

Convert between domain objects and entity representations:

```csharp
using System.Events.Data;

// Get converters from factory
public class EventProcessor
{
    private readonly IEventConverterFactory _factory;

    public EventProcessor(IEventConverterFactory factory)
        => _factory = factory;

    public EntityDomainEvent ConvertToEntity(IDomainEvent domainEvent)
    {
        var converter = _factory.GetEventConverter<IDomainEvent>();
        return (EntityDomainEvent)converter.ConvertEventToEntity(
            domainEvent,
            EventConverter.SerializerOptions);
    }

    public IDomainEvent ConvertFromEntity(EntityDomainEvent entity)
    {
        var converter = _factory.GetEventConverter<IDomainEvent>();
        return (IDomainEvent)converter.ConvertEntityToEvent(entity);
    }
}
```

---

## Stream operations

### Append Events

```csharp
// Append with optimistic concurrency
var request = new AppendRequest
{
    StreamId = aggregateId,
    Events = domainEvents,
    ExpectedVersion = currentVersion // Will throw if version mismatch
};

try
{
    var result = await _eventStore.AppendToStreamAsync(request);
    await _eventStore.FlushEventsAsync();
}
catch (InvalidOperationException ex)
{
    // Handle concurrency conflict
    Console.WriteLine($"Concurrency error: {ex.Message}");
}
```

### Read Stream

```csharp
// Read all events from a stream
var request = new ReadStreamRequest
{
    StreamId = aggregateId,
    FromVersion = 0,
    MaxCount = int.MaxValue
};

var events = new List<IDomainEvent>();
await foreach (var envelope in _eventStore.ReadStreamAsync(request))
{
    events.Add((IDomainEvent)envelope.Event);
}
```

### Read All Streams

```csharp
// Read events across all streams (for projections)
var request = new ReadAllStreamsRequest
{
    FromPosition = lastProcessedPosition,
    MaxCount = 100
};

await foreach (var envelope in _eventStore.ReadAllStreamsAsync(request))
{
    await UpdateProjectionAsync(envelope);
    lastProcessedPosition = envelope.GlobalPosition;
}
```

### Subscribe to Stream

```csharp
// Subscribe to new events on a stream
var request = new SubscribeToStreamRequest
{
    StreamId = aggregateId,
    PollingInterval = TimeSpan.FromSeconds(1),
    OnEvent = async domainEvent =>
    {
        Console.WriteLine($"New event: {domainEvent.GetEventName()}");
        await ProcessEventAsync(domainEvent);
    }
};

await using var subscription = _eventStore.SubscribeToStream(request);

// Subscription runs until disposed
await Task.Delay(TimeSpan.FromMinutes(5));
```

### Subscribe to All Streams

```csharp
// Subscribe to all new events (for projections)
var request = new SubscribeToAllStreamsRequest
{
    PollingInterval = TimeSpan.FromMilliseconds(500),
    OnEvent = async domainEvent =>
    {
        await UpdateReadModelAsync(domainEvent);
    }
};

await using var subscription = _eventStore.SubscribeToAllStreams(request);
```

---

## 📸 Snapshot Support

```csharp
// Save a snapshot
var snapshotEvent = new OrderSnapshotEvent
{
    OwnerId = orderId,
    OrderNumber = order.OrderNumber,
    TotalAmount = order.TotalAmount,
    Status = order.Status
};

await _eventStore.AppendSnapshotAsync(snapshotEvent);
await _eventStore.FlushEventsAsync();

// Load latest snapshot
var snapshot = await _eventStore.GetLatestSnapshotAsync(orderId);
if (snapshot is not null)
{
    var orderSnapshot = (OrderSnapshotEvent)snapshot.Event;
    // Rebuild from snapshot + events after snapshot version
}
```

---

## 🗑️ Stream Management

### Delete Stream

```csharp
// Soft delete (marks events as deleted)
var request = new DeleteStreamRequest
{
    StreamId = aggregateId,
    HardDelete = false
};
await _eventStore.DeleteStreamAsync(request);
await _eventStore.FlushEventsAsync();

// Hard delete (permanently removes events)
var hardDeleteRequest = new DeleteStreamRequest
{
    StreamId = aggregateId,
    HardDelete = true
};
await _eventStore.DeleteStreamAsync(hardDeleteRequest);
await _eventStore.FlushEventsAsync();
```

### Truncate Stream

```csharp
// Remove events before a specific version (for cleanup)
var request = new TruncateStreamRequest
{
    StreamId = aggregateId,
    TruncateBeforeVersion = 100 // Keep only events from version 100 onwards
};
await _eventStore.TruncateStreamAsync(request);
await _eventStore.FlushEventsAsync();
```

### Check Stream Existence

```csharp
bool exists = await _eventStore.StreamExistsAsync(aggregateId);
long version = await _eventStore.GetStreamVersionAsync(aggregateId);
```

---

## ⚙️ Entity Types

### EntityDomainEvent

Stores domain events with stream information:

| Property | Type | Description |
|----------|------|-------------|
| KeyId | Guid | Unique event identifier |
| StreamId | Guid | Aggregate/stream identifier |
| StreamName | string | Name of the stream |
| StreamVersion | long | Version within the stream |
| Sequence | long | Global sequence number |
| EventName | string | Event type name |
| EventData | string | Serialized event data (JSON) |
| CausationId | string? | String-based causation identifier (supports W3C, GUID, custom formats) |
| CorrelationId | string? | String-based correlation identifier (supports W3C trace IDs, GUIDs) |
| CreatedOn | DateTime | When the event occurred |
| Status | EventStatus | ACTIVE, DELETED, etc. |

### EntitySnapshotEvent

Stores aggregate snapshots:

| Property | Type | Description |
|----------|------|-------------|
| KeyId | Guid | Unique snapshot identifier |
| OwnerId | Guid | Aggregate identifier |
| Sequence | long | Snapshot sequence number |
| EventName | string | Snapshot type name |
| EventData | string | Serialized snapshot data |
| CausationId | string? | String-based causation identifier |
| CorrelationId | string? | String-based correlation identifier |
| CreatedOn | DateTime | When the snapshot was created |

### EntityEventOutbox

Stores integration events for outbox pattern (reliable publishing):

| Property | Type | Description |
|----------|------|-------------|
| KeyId | Guid | Unique event identifier |
| EventName | string | Event type name |
| EventData | string | Serialized event data |
| CausationId | string? | String-based causation identifier |
| CorrelationId | string? | String-based correlation identifier (W3C trace ID, GUID, etc.) |
| Status | EntityStatus | PENDING, PUBLISHED, ONERROR |
| CreatedOn | DateTime | When the event was created |

### EntityEventInbox

Stores integration events for inbox pattern (exactly-once consumption):

| Property | Type | Description |
|----------|------|-------------|
| KeyId | Guid | Unique event identifier |
| Consumer | string | Logical consumer/handler name (part of composite key) |
| EventName | string | Event type name |
| EventData | string | Serialized event data |
| CausationId | string? | String-based causation identifier |
| CorrelationId | string? | String-based correlation identifier |
| Status | EntityStatus | PROCESSING, PUBLISHED (success), ONERROR (failed) |
| ClaimId | Guid? | Lease identifier for visibility timeout |
| NextAttemptOn | DateTime? | When retry is allowed (after visibility timeout) |
| CreatedOn | DateTime | When the event was received |

---

## Best practices

1. **Always call `FlushEventsAsync`** - events are batched in the unit-of-work
2. **Use ExpectedVersion** - Enable optimistic concurrency for aggregates
3. **Take snapshots periodically** - Improve rebuild performance for long streams
4. **Subscribe for projections** - Use subscriptions for read model updates
5. **Truncate old events** - Clean up after taking snapshots
6. **Keep correlation IDs** - flow `EventContext` in your app to keep causation/correlation consistent
7. **Use W3C trace IDs** - leverage `traceparent` for distributed tracing interoperability
8. **Combine Outbox + Inbox** - Use outbox for reliable publishing, inbox for exactly-once consumption

---

## 📚 Related Packages

- **System.Events** - Event abstractions (IDomainEvent, IEventStore, IAggregate)
- **System.Entities.Data** - EF Core repository pattern
- **System.Results** - Result types for error handling

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
