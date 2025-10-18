# 🗄️ Xpandables.Net.Events.EntityFramework

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Entity Framework Event Store** - Production-ready event sourcing implementation using EF Core for persistent event storage with snapshot support.

---

## 📋 Overview

Provides a complete Entity Framework Core implementation of event store interfaces, enabling event sourcing patterns with persistent storage, snapshots, and optimistic concurrency control.

### 🎯 Key Features

- 🗄️ **EF Core Storage** - Persistent event storage
- 📸 **Snapshot Support** - Performance optimization
- 🔒 **Optimistic Concurrency** - Stream version management
- 🔔 **Event Subscriptions** - Real-time event notifications
- 📤 **Outbox Pattern** - Reliable integration event publishing

---

## 🚀 Quick Start

```csharp
// DbContext setup
public sealed class EventStoreDbContext : DbContext
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<SnapshotEntity> Snapshots { get; set; }
    public DbSet<OutboxEntity> Outbox { get; set; }
}

// Registration
services.AddDbContext<EventStoreDbContext>(options =>
    options.UseSqlServer(connectionString));

services.AddXEventStore<EventStoreDbContext>();

// Usage
var order = Order.Create(customerId);
order.AddItem(productId, quantity, price);

await _aggregateStore.AppendAsync(order);

// Load from events
var loadedOrder = await _aggregateStore.ReadAsync<Order>(orderId);
```

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
