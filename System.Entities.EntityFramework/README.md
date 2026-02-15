# System.Entities.EntityFramework

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Entities.EntityFramework.svg)](https://www.nuget.org/packages/Xpandables.Entities.EntityFramework)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Entity Framework Core implementation of Repository and Unit of Work patterns with automatic entity lifecycle tracking.

## Overview

`System.Entities.EntityFramework` provides EF Core implementations for the repository and unit of work patterns defined in `System.Entities`. It includes `DataContext` with automatic entity lifecycle tracking, `EntityRepository<TEntity>` for data access, and `EntityUnitOfWork<TDataContext>` for transaction management.

Built for .NET 10 with Entity Framework Core.

## Features

### DataContext
- **`DataContext`** — Extended `DbContext` with automatic entity lifecycle tracking
- Automatically sets `CreatedOn` when entities are added
- Automatically sets `UpdatedOn` when entities are modified
- Automatically sets `DeletedOn` and `Status = DELETED` when entities are deleted

### Repository
- **`EntityRepository<TEntity>`** — Generic EF Core repository implementing `IEntityRepository<TEntity>`
- Implements `IAmbientContextReceiver<DataContext>` for context injection
- Full async CRUD with `IAsyncPagedEnumerable<T>` results

### Unit of Work
- **`EntityUnitOfWork<TDataContext>`** — Transaction management implementing `IEntityUnitOfWork<TDataContext>`
- **`EntityUnitOfWorkTransaction`** — Transaction wrapper with commit/rollback

### Value Converters
- **`JsonDocumentValueConverter`** — Convert `JsonDocument` to/from string
- **`ReadOnlyMemoryToByteArrayConverter`** — Convert `ReadOnlyMemory<byte>` to/from byte array
- **`ConverterExtensions`** — Extension methods: `HasJsonDocumentConversion()`, `HasJsonDocumentComparer()`, `HasReadOnlyMemoryToByteArrayConversion()`

### Extensions
- **`EntityUpdaterExtensions`** — Build EF Core `ExecuteUpdate` expressions from `EntityUpdater<T>`
- **`IEntityPropertyUpdateExtensions`** — Convert property updates to EF Core setters

## Installation

```bash
dotnet add package Xpandables.Entities.EntityFramework
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**Dependencies:** `System.Entities`, `Microsoft.EntityFrameworkCore`

## Quick Start

### Define Your DataContext

```csharp
using System.Entities.EntityFramework;
using Microsoft.EntityFrameworkCore;

public class AppDataContext : DataContext
{
    public AppDataContext(DbContextOptions<AppDataContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.KeyId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });
    }
}
```

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register DataContext
services.AddXDataContext<AppDataContext>(options =>
    options.UseSqlServer(connectionString));

// Register EntityUnitOfWork for the DataContext
services.AddXEntityUnitOfWork<AppDataContext>();

// Register generic EntityRepository
services.AddXEntityRepository();
```

### Use the Repository

```csharp
using System.Entities;

public class UserService(
    IEntityRepository<User> repository,
    IEntityUnitOfWork unitOfWork)
{
    public IAsyncPagedEnumerable<UserDto> GetUsers()
    {
        var spec = QuerySpecification
            .For<User>()
            .Where(u => u.IsActive)
            .Select(u => new UserDto(u.KeyId, u.Name));

        return repository.FetchAsync(spec);
    }

    public async Task<User?> GetByIdAsync(
        Guid id, CancellationToken ct)
    {
        var spec = QuerySpecification
            .For<User>()
            .Where(u => u.KeyId == id)
            .Select(u => u);

        return await repository
            .FetchSingleOrDefaultAsync(spec, ct);
    }

    public async Task CreateUserAsync(
        User user, CancellationToken ct)
    {
        // CreatedOn is set automatically by DataContext
        await repository.AddAsync([user], ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
```

### Bulk Updates with EntityUpdater

```csharp
using System.Entities;

var spec = QuerySpecification
    .For<Product>()
    .Where(p => p.IsActive)
    .Select(p => p);

var updater = EntityUpdater
    .For<Product>()
    .SetProperty(e => e.Price, e => e.Price * 1.1m)
    .SetProperty(e => e.LastUpdated, DateTime.UtcNow);

// Via UnitOfWork (tracked, can rollback)
int updated = await repository.UpdateAsync(spec, updater, ct);
await unitOfWork.SaveChangesAsync(ct);

// Bulk (immediate SQL, bypasses UoW)
int bulkUpdated = await repository.UpdateBulkAsync(spec, updater, ct);
```

### Transactions

```csharp
await using var transaction = await unitOfWork
    .BeginTransactionAsync(ct);

try
{
    var orderRepo = unitOfWork
        .GetRepository<IEntityRepository<Order>>();

    await orderRepo.AddAsync([order], ct);
    await unitOfWork.SaveChangesAsync(ct);

    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

---

## 📅 Automatic Entity Lifecycle Tracking

`DataContext` tracks entity lifecycle events via `ChangeTracker`:

| Event | Property Set | Value |
|-------|-------------|-------|
| Entity added | `CreatedOn` | `DateTime.UtcNow` |
| Entity modified | `UpdatedOn` | `DateTime.UtcNow` |
| Entity deleted | `DeletedOn` | `DateTime.UtcNow` |
| Entity deleted | `Status` | `EntityStatus.DELETED` |

```csharp
// CreatedOn set automatically when added
var user = new User { Name = "John", Email = "john@example.com" };
await repository.AddAsync([user], ct);
await unitOfWork.SaveChangesAsync(ct);
// user.CreatedOn is now DateTime.UtcNow
```

---

## 🔧 Value Converters

### JsonDocument Converter

```csharp
using System.Entities.EntityFramework.Converters;
using System.Text.Json;

// In DataContext.OnModelCreating
modelBuilder.Entity<AuditLog>(entity =>
{
    entity.Property(e => e.Data)
        .HasJsonDocumentConversion()
        .HasJsonDocumentComparer();
});
```

### ReadOnlyMemory&lt;byte&gt; Converter

```csharp
using System.Entities.EntityFramework.Converters;

// In DataContext.OnModelCreating
modelBuilder.Entity<BinaryData>(entity =>
{
    entity.Property(e => e.Content)
        .HasReadOnlyMemoryToByteArrayConversion();
});
```

---

## Core Types

| Type | Description |
|------|-------------|
| `DataContext` | Extended DbContext with lifecycle tracking |
| `EntityRepository<TEntity>` | EF Core repository implementation |
| `EntityUnitOfWork<TDataContext>` | Transaction coordinator |
| `EntityUnitOfWorkTransaction` | Transaction wrapper |
| `JsonDocumentValueConverter` | JsonDocument EF converter |
| `ReadOnlyMemoryToByteArrayConverter` | Memory EF converter |

## DI Extension Methods

| Method | Description |
|--------|-------------|
| `AddXDataContext<T>()` | Register DataContext with DbContextOptions |
| `AddXDataContextFactory<T>()` | Register IDbContextFactory for factory pattern |
| `AddXEntityUnitOfWork<T>()` | Register EntityUnitOfWork for DataContext |
| `AddXEntityRepository()` | Register generic EntityRepository |

---

## ✅ Best Practices

- **Inherit from `DataContext`** — Get automatic entity lifecycle tracking
- **Implement `IEntity`** — Enable automatic `CreatedOn`/`UpdatedOn`/`DeletedOn`
- **Use `QuerySpecification` with projections** — Select only needed columns via `Select`
- **Use `EntityUpdater`** — For efficient bulk updates via `ExecuteUpdate`
- **Use `IEntityUnitOfWork` for transactions** — Coordinate multiple operations
- **Use `UpdateBulkAsync`/`DeleteBulkAsync`** — For immediate SQL execution bypassing UoW

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Entities** | Core entity abstractions (`IEntity`, `IEntityRepository`, `IEntityUnitOfWork`) |
| **Xpandables.Events.Data** | Event Store and Outbox implementations |

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
