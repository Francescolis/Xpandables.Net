# System.Events.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Events.Data.svg)](https://www.nuget.org/packages/Xpandables.Events.Data)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Events.Data.svg)](https://www.nuget.org/packages/Xpandables.Events.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

ADO.NET persistence for event sourcing with `System.Events`.

## Overview

`System.Events.Data` provides ADO.NET implementations for the event store, outbox, and inbox abstractions defined in `System.Events`. It uses `System.Data` (IDataRepository, IDataUnitOfWork) for raw ADO.NET persistence with SQL scripts for table creation across SQL Server, PostgreSQL, and SQLite.

Built for .NET 10. **Does not use Entity Framework Core.**

## Features

### Event Store
- **`EventStore<TDomain, TSnapshot>`** — ADO.NET event store implementing `IEventStore`
- Supports append, read, subscribe, truncate, and delete stream operations

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

// Register all event stores (EventStore, OutboxStore, InboxStore)
services.AddXEventStores();

// Or register individually
services.AddXEventStore();
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

### Use the Event Store

```csharp
using System.Events.Domain;
using System.Events.Data;

public class OrderService(IEventStore eventStore)
{
    public async Task CreateOrderAsync(
        Guid orderId, CancellationToken ct)
    {
        var request = new AppendRequest
        {
            StreamId = orderId,
            Events = [new OrderCreated { StreamId = orderId }]
        };

        AppendResult result = await eventStore
            .AppendToStreamAsync(request, ct);
    }

    public async Task<List<IDomainEvent>> GetEventsAsync(
        Guid streamId, CancellationToken ct)
    {
        var request = new ReadStreamRequest
        {
            StreamId = streamId,
            FromVersion = 0
        };

        var events = new List<IDomainEvent>();
        await foreach (var envelope in eventStore
            .ReadStreamAsync(request, ct))
        {
            events.Add(envelope.Event);
        }
        return events;
    }
}
```

---

## Core Types

| Type | Description |
|------|-------------|
| `EventStore<TDomain, TSnapshot>` | ADO.NET event store |
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
| `AddXEventStores()` | Register all stores (EventStore, Outbox, Inbox) |
| `AddXEventStore()` | Register EventStore only |
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
