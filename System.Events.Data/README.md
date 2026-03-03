# System.Events.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Events.Data.svg)](https://www.nuget.org/packages/Xpandables.Events.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

ADO.NET persistence for event sourcing with `System.Events`.

## Overview

`System.Events.Data` provides ADO.NET implementations for the event store, outbox, and inbox abstractions defined in `System.Events`. It uses `System.Data` (IDataRepository, IDataUnitOfWork) for raw ADO.NET persistence with SQL scripts for table creation across SQL Server, PostgreSQL, and SQLite.

Built for .NET 10. **Does not use Entity Framework Core.**

## Features

### Domain Store
- **`DomainStore<TDomain, TSnapshot>`** — ADO.NET domain store implementing `IDomainStore`
- Supports append, read, subscribe, truncate, delete stream, and snapshot operations

### Outbox/Inbox Stores
- **`OutboxStore<TOutbox>`** — ADO.NET outbox implementing `IOutboxStore`
- **`InboxStore<TInbox>`** — ADO.NET inbox implementing `IInboxStore`

### Data Entities
- **`DataEvent`** — Base entity for event records
- **`DataEventDomain`** — Domain event entity (`IDataEventDomain`)
- **`DataEventSnapshot`** — Snapshot event entity (`IDataEventSnapshot`)
- **`DataEventOutbox`** — Outbox event entity (`IDataEventOutbox`)
- **`DataEventInbox`** — Inbox event entity (`IDataEventInbox`)

### Event Converters
- **`IEventConverter<TDataEvent, TEvent>`** — Convert between data entities and event types
- **`IEventConverterFactory`** — Factory for obtaining converters
- **`EventConverterFactory`** — Default factory implementation
- **`EventConverterDomain`** — Domain event converter
- **`EventConverterSnapshot`** — Snapshot event converter
- **`EventConverterOutbox`** — Outbox event converter
- **`EventConverterInbox`** — Inbox event converter
- **`IEventConverterContext`** — Shared context for converters
- **`DefaultEventConverterContext`** — Default converter context

### SQL Scripts
- **`IEventTableScriptProvider`** — Provides SQL scripts for creating/dropping event tables
- **`SqlServerEventTableScripts`** — SQL Server table scripts
- **`PostgreSqlEventTableScripts`** — PostgreSQL table scripts
- **`SqliteEventTableScripts`** — SQLite table scripts
- **`EventTableScriptExporter`** — Export scripts to files

## Installation

```bash
dotnet add package Xpandables.Events.Data
```

**Dependencies:** `System.Events`, `System.Data`

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register all event stores (DomainStore, OutboxStore, InboxStore)
services.AddXEventStores();

// Or register individually
services.AddXDomainStore();
services.AddXOutboxStore();
services.AddXInboxStore();

// Register converter factory and context
services.AddXEventConverterFactory();
services.AddXEventConverterContext();
```

### Create Database Tables

```csharp
using System.Events.Data.Scripts;

// Get SQL scripts for your database
var scripts = new SqlServerEventTableScripts();
string createScript = scripts.GetCreateAllTablesScript(schema: "Events");
string dropScript = scripts.GetDropAllTablesScript(schema: "Events");

// Or use PostgreSQL/SQLite
var pgScripts = new PostgreSqlEventTableScripts();
var sqliteScripts = new SqliteEventTableScripts();
```

### Use the Domain Store

```csharp
using System.Events.Domain;
using System.Events.Data;

public class OrderEventService(IDomainStore domainStore)
{
    // Append domain events to a stream
    public async Task CreateOrderAsync(
        Guid orderId, CancellationToken ct)
    {
        var request = new AppendRequest
        {
            StreamId = orderId,
            Events = [new OrderCreated { StreamId = orderId }],
            ExpectedVersion = null // null = no concurrency check
        };

        AppendResult result = await domainStore
            .AppendToStreamAsync(request, ct);

        Console.WriteLine(
            $"Appended {result.EventIds.Count} events, " +
            $"versions {result.FirstAssignedStreamVersion}..{result.LastAssignedStreamVersion}");
    }

    // Read all events from a stream
    public async Task<List<EnvelopeResult>> GetEventsAsync(
        Guid streamId, CancellationToken ct)
    {
        var request = new ReadStreamRequest
        {
            StreamId = streamId,
            FromVersion = 0,
            MaxCount = 0 // 0 = no limit
        };

        var events = new List<EnvelopeResult>();
        await foreach (EnvelopeResult envelope in domainStore
            .ReadStreamAsync(request, ct))
        {
            events.Add(envelope);
        }
        return events;
    }

    // Check stream existence
    public async Task<(bool Exists, long Version)> CheckStreamAsync(
        Guid streamId, CancellationToken ct)
    {
        bool exists = await domainStore.StreamExistsAsync(streamId, ct);
        long version = await domainStore.GetStreamVersionAsync(streamId, ct);
        return (exists, version);
    }
}
```

### Use the Outbox Store

```csharp
using System.Events.Integration;

public class IntegrationEventService(IOutboxStore outboxStore)
{
    // Enqueue integration events for reliable delivery
    public async Task EnqueueOrderPlacedAsync(
        Guid orderId, Guid customerId, decimal total,
        CancellationToken ct)
    {
        IIntegrationEvent[] events =
        [
            new OrderPlacedIntegrationEvent
            {
                OrderId = orderId,
                CustomerId = customerId,
                Total = total
            }
        ];

        await outboxStore.EnqueueAsync(events, ct);
    }

    // Dequeue pending events for processing
    public async Task ProcessPendingAsync(CancellationToken ct)
    {
        IReadOnlyList<IIntegrationEvent> pending =
            await outboxStore.DequeueAsync(
                maxEvents: 10,
                visibilityTimeout: TimeSpan.FromMinutes(5),
                ct);

        foreach (IIntegrationEvent @event in pending)
        {
            try
            {
                await PublishToExternalBusAsync(@event, ct);
                await outboxStore.CompleteAsync(ct, @event.EventId);
            }
            catch (Exception ex)
            {
                await outboxStore.FailAsync(ct, @event.EventId);
            }
        }
    }

    private Task PublishToExternalBusAsync(
        IIntegrationEvent @event, CancellationToken ct) => Task.CompletedTask;
}
```

### Use the Inbox Store (Exactly-Once Processing)

```csharp
using System.Events.Integration;

public class InboxProcessingService(IInboxStore inboxStore)
{
    // Receive and deduplicate an incoming integration event
    public async Task HandleIncomingEventAsync(
        IIntegrationEvent @event,
        string consumer,
        CancellationToken ct)
    {
        // Check if already processed
        InboxReceiveResult result = await inboxStore.ReceiveAsync(
            @event,
            consumer,
            visibilityTimeout: TimeSpan.FromMinutes(5),
            ct);

        if (result.ShouldProcess)
        {
            try
            {
                // Process the event...
                await ProcessEventAsync(@event, ct);
                await inboxStore.CompleteAsync(ct);
            }
            catch
            {
                await inboxStore.FailAsync(ct);
                throw;
            }
        }
        // else: already processed, skip
    }

    private Task ProcessEventAsync(
        IIntegrationEvent @event, CancellationToken ct) => Task.CompletedTask;
}
```

### Event Converters

```csharp
using System.Events.Data;

// Custom converter context (optional)
public sealed class CustomEventConverterContext : IEventConverterContext
{
    // Custom serialization settings, encryption, etc.
}

// Register with DI
services.AddXEventConverterFactory();
services.AddXEventConverterContext<CustomEventConverterContext>();

// Or use defaults
services.AddXEventConverterContext();
```
```

---

## Core Types

| Type | Description |
|------|-------------|
| `DomainStore<TDomain, TSnapshot>` | ADO.NET domain store |
| `OutboxStore<TOutbox>` | ADO.NET outbox store |
| `InboxStore<TInbox>` | ADO.NET inbox store |
| `DataEventDomain` | Domain event data entity |
| `DataEventSnapshot` | Snapshot data entity |
| `DataEventOutbox` | Outbox data entity |
| `DataEventInbox` | Inbox data entity |
| `IEventConverterFactory` | Converter factory |
| `IEventTableScriptProvider` | SQL script provider |

## DI Extension Methods

| Method | Description |
|--------|-------------|
| `AddXEventStores()` | Register all stores (DomainStore, Outbox, Inbox) |
| `AddXDomainStore()` | Register DomainStore only |
| `AddXOutboxStore()` | Register OutboxStore only |
| `AddXInboxStore()` | Register InboxStore only |
| `AddXEventConverterFactory()` | Register event converter factory |
| `AddXEventConverterContext()` | Register default converter context |

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Events** | Core event sourcing abstractions |
| **Xpandables.Data** | ADO.NET repository and unit of work |

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
