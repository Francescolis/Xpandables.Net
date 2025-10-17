# ?? Xpandables.Net.Events.Repositories

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Event Store Abstractions** - Core abstractions and interfaces for event store implementations.

---

## ?? Overview

Provides the foundational abstractions and interfaces for building event store implementations, including event storage, retrieval, and subscription contracts.

### ? Key Features

- ?? **Event Store Contracts** - Core interfaces
- ?? **Subscription Abstractions** - Event streaming
- ?? **Snapshot Support** - Performance optimization contracts
- ??? **Outbox Pattern** - Integration event abstractions

---

## ?? Core Interfaces

```csharp
public interface IEventStore
{
    Task<AppendResult> AppendAsync(AppendRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<EnvelopeResult> ReadAsync(ReadStreamRequest request, CancellationToken cancellationToken = default);
    Task SubscribeToStreamAsync(SubscribeToStreamRequest request, CancellationToken cancellationToken = default);
}

public interface ISnapshotEventStore
{
    Task SaveSnapshotAsync(Guid streamId, ISnapshotEvent snapshot, CancellationToken cancellationToken = default);
    Task<ISnapshotEvent?> GetSnapshotAsync(Guid streamId, CancellationToken cancellationToken = default);
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
