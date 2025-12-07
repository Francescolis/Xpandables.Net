# ?? System.Events.Data

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Store with EF Core** - Entity Framework Core implementation of event sourcing with EventStoreDataContext, OutboxStoreDataContext, and aggregate persistence.

---

## ?? Overview

`System.Events.Data` provides EF Core-based implementations for event sourcing patterns including event store, outbox pattern, and aggregate reconstruction.

### ? Key Features

- ??? **EventStoreDataContext** - EF Core context for event persistence
- ?? **OutboxStoreDataContext** - Outbox pattern implementation
- ?? **EventStore** - Store and retrieve domain events
- ?? **OutboxStore** - Reliable event publishing with transactional outbox
- ??? **AggregateStore** - Save and reconstruct aggregates from events
- ?? **Snapshot Support** - Aggregate snapshots for performance

---

## ?? Quick Start

### Installation

```bash
dotnet add package System.Events.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Event Store Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Event Store
builder.Services.AddXEventStoreDataContext(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStore"),
        sql => sql.MigrationsHistoryTable("__EventStoreMigrations")));

builder.Services.AddXEventStore();
builder.Services.AddXAggregateStore();
```

### Outbox Pattern Setup

```csharp
// Register Outbox Store
builder.Services.AddXOutboxStoreDataContext(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EventStore"),
        sql => sql.MigrationsHistoryTable("__OutboxStoreMigrations")));

builder.Services.AddXOutboxStore();
```

### Using Event Store

```csharp
public class OrderService
{
    private readonly IAggregateStore _aggregateStore;
    
    public OrderService(IAggregateStore aggregateStore) =>
        _aggregateStore = aggregateStore;
    
    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        var order = OrderAggregate.Create(command.CustomerId, command.Items);
        await _aggregateStore.AppendAsync(order);
    }
    
    public async Task<OrderAggregate> GetOrderAsync(Guid orderId)
    {
        return await _aggregateStore.ReadAsync<OrderAggregate>(orderId);
    }
}
```

---

## ?? Related Packages

- **System.Events** - Event abstractions
- **System.Data.EntityFramework** - EF Core repository
- **System.ExecutionResults** - Result types

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
