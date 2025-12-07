# 🗄️ System.Events.Data

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Store with EF Core** - Entity Framework Core implementation of event sourcing with EventStoreDataContext, OutboxStoreDataContext, event converters, and stream management.

---

## 📋 Overview

`System.Events.Data` provides complete EF Core-based implementations for event sourcing patterns including event store, outbox pattern, snapshot support, and stream subscriptions. It enables persisting domain events, integration events, and aggregate snapshots using Entity Framework Core.

### ✨ Key Features

- 💾 **EventStoreDataContext** - EF Core DbContext for domain events and snapshots
- 📤 **OutboxStoreDataContext** - EF Core DbContext for integration events (outbox pattern)
- 📝 **EventStore** - Full IEventStore implementation with append, read, and stream operations
- 📨 **OutboxStore** - Reliable integration event publishing with transactional outbox
- 🔄 **Event Converters** - Convert between domain events and entity representations
- 📸 **Snapshot Support** - Store and retrieve aggregate snapshots for performance
- 📡 **Stream Subscriptions** - Subscribe to event streams with polling
- 🔒 **Transactional Flush** - Atomic commit of events and outbox together

---

## 📥 Installation

```bash
dotnet add package System.Events.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

---

## 🚀 Quick Start

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Event Store DataContext
builder.Services.AddXEventStoreDataContext(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStore"),
        sql => sql.MigrationsHistoryTable("__EventStoreMigrations")));

// Register Outbox Store DataContext
builder.Services.AddXOutboxStoreDataContext(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStore"),
        sql => sql.MigrationsHistoryTable("__OutboxStoreMigrations")));

// Register Event Store and Outbox Store
builder.Services.AddXEventStore();
builder.Services.AddXOutboxStore();
builder.Services.AddXEventConverterFactory();
```

### Using the Event Store

```csharp
using System.Events.Data;
using System.Events.Domain;

public class OrderService
{
    private readonly IEventStore _eventStore;

    public OrderService(IEventStore eventStore)
        => _eventStore = eventStore;

    public async Task CreateOrderAsync(Guid orderId, string customerId)
    {
        var events = new IDomainEvent[]
        {
            new OrderCreatedEvent { OrderId = orderId, CustomerId = customerId },
            new OrderItemAddedEvent { OrderId = orderId, ProductId = "PROD-1", Quantity = 2 }
        };

        var request = new AppendRequest
        {
            StreamId = orderId,
            Events = events,
            ExpectedVersion = null // No optimistic concurrency for new stream
        };

        AppendResult result = await _eventStore.AppendToStreamAsync(request);
        Console.WriteLine($"Appended to version {result.NextExpectedVersion}");

        // Flush events to database
        await _eventStore.FlushEventsAsync();
    }

    public async Task<Order> GetOrderAsync(Guid orderId)
    {
        var order = new Order();

        var request = new ReadStreamRequest
        {
            StreamId = orderId,
            FromVersion = 0,
            MaxCount = 1000
        };

        await foreach (var envelope in _eventStore.ReadStreamAsync(request))
        {
            order.Apply(envelope.Event);
        }

        return order;
    }
}
```

---

## 🧩 Core Components

### EventStoreDataContext

The EF Core DbContext for storing domain events and snapshots:

```csharp
using System.Events.Data;
using Microsoft.EntityFrameworkCore;

// The context is pre-configured with:
// - EntityDomainEvent entity for domain events
// - EntitySnapshotEvent entity for snapshots
// - Default schema "Events"

// Access via DI
public class MyService
{
    private readonly EventStoreDataContext _context;

    public MyService(EventStoreDataContext context)
        => _context = context;

    public async Task<int> GetEventCountAsync(Guid streamId)
    {
        return await _context.Domains
            .CountAsync(e => e.StreamId == streamId);
    }
}
```

### OutboxStoreDataContext

The EF Core DbContext for the outbox pattern (integration events):

```csharp
using System.Events.Data;

// The context stores integration events for reliable publishing
// Events are marked as processed after successful delivery

public class IntegrationEventPublisher
{
    private readonly OutboxStoreDataContext _outbox;
    private readonly IEventConverter _converter;

    public async Task PublishPendingEventsAsync(CancellationToken cancellationToken)
    {
        var pending = await _outbox.Set<EntityIntegrationEvent>()
            .Where(e => e.Status == EventStatus.PENDING)
            .ToListAsync(cancellationToken);

        foreach (var entity in pending)
        {
            var @event = _converter.ConvertEntityToEvent(entity);
            await PublishToMessageBusAsync(@event);
            entity.SetStatus(EventStatus.PROCESSED);
        }

        await _outbox.SaveChangesAsync(cancellationToken);
    }
}
```

### Event Converters

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

## 📡 Stream Operations

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
| CreatedOn | DateTime | When the snapshot was created |

### EntityIntegrationEvent

Stores integration events for outbox pattern:

| Property | Type | Description |
|----------|------|-------------|
| KeyId | Guid | Unique event identifier |
| EventName | string | Event type name |
| EventData | string | Serialized event data |
| Status | EventStatus | PENDING, PROCESSED, FAILED |
| CreatedOn | DateTime | When the event was created |

---

## ✅ Best Practices

1. **Always call FlushEventsAsync** - Events are batched; call flush to persist
2. **Use ExpectedVersion** - Enable optimistic concurrency for aggregates
3. **Take snapshots periodically** - Improve rebuild performance for long streams
4. **Subscribe for projections** - Use subscriptions for read model updates
5. **Truncate old events** - Clean up after taking snapshots
6. **Use transactions** - FlushEventsAsync commits events and outbox atomically

---

## 📚 Related Packages

- **System.Events** - Event abstractions (IDomainEvent, IEventStore, IAggregate)
- **System.Entities.Data** - EF Core repository pattern
- **System.Results** - Result types for error handling

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
