# System.Entities

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Entities.svg)](https://www.nuget.org/packages/Xpandables.Entities)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Repository and Unit of Work abstractions for .NET. Provides entity interfaces, query specifications, and transaction management contracts.

## Overview

`System.Entities` defines the core abstractions for implementing the Repository and Unit of Work patterns. It provides interfaces for entities with lifecycle tracking, repositories with async operations, query specifications for type-safe queries, and transaction management.

Built for .NET 10 with AOT compatibility and full async support.

## Features

### Entity Interfaces
- **`IEntity`** — Base entity with lifecycle tracking (`KeyId`, `CreatedOn`, `UpdatedOn`, `DeletedOn`, `Status`, `IsDeleted`)
- **`IEntity<TKey>`** — Typed entity with strongly-typed key identifier
- **`EntityStatus`** — Status constants (`ACTIVE`, `INACTIVE`, `DELETED`, `SUSPENDED`)

### Repository Pattern
- **`IRepository`** — Base repository marker interface (implements `IDisposable`, `IAsyncDisposable`)
- **`IRepository<TEntity>`** — Generic repository with async CRUD operations
- **`IAmbientContextReceiver<TContext>`** — For receiving ambient context data
- **`IRequiresUnitOfWork`** — Marker for handlers requiring transactional scope

### Query Specifications
- **`IQuerySpecification<TEntity, TResult>`** — Type-safe query specification with predicate, selector, includes, ordering, skip/take
- **`QuerySpecification`** — Static factory with `For<TEntity>()` method
- **`QuerySpecificationBuilder<TEntity>`** — Fluent builder for specifications
- **`IIncludeSpecification<TEntity>`** — Eager loading specification
- **`IOrderSpecification<TEntity>`** — Ordering specification
- **`IThenIncludeSpecification`** — Nested include specification

### Unit of Work
- **`IUnitOfWork`** — Transaction coordination with `GetRepository<T>()`, `BeginTransactionAsync()`, `SaveChangesAsync()`
- **`IUnitOfWork<TContext>`** — Typed unit of work
- **`IUnitOfWorkTransaction`** — Transaction abstraction with `CommitAsync()`, `RollbackAsync()`
- **`UnitOfWorkDbTransaction`** — DbTransaction wrapper

### Entity Updates
- **`EntityUpdater`** — Static factory with `For<TSource>()` and `SetProperty()` methods
- **`EntityUpdater<TEntity>`** — Fluent API for building property update expressions
- **`IEntityPropertyUpdate<TEntity>`** — Property update abstraction

## Installation

```bash
dotnet add package Xpandables.Entities
```

## Quick Start

### Define an Entity

```csharp
using System.Entities;

public class User : IEntity<Guid>
{
    public Guid KeyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // IEntity properties
    public string Status { get; set; } = EntityStatus.ACTIVE;
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public bool IsDeleted => DeletedOn.HasValue;

    public void SetStatus(string status) => Status = status;
}
```

### Create a Query Specification

```csharp
using System.Entities;

// Using the fluent builder
var spec = QuerySpecification
    .For<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .Include(u => u.Orders)
    .Skip(0)
    .Take(20)
    .Select(u => new UserDto(u.KeyId, u.Name, u.Email));
```

### Use the Repository

```csharp
using System.Entities;

public class UserService(IRepository<User> repository, IUnitOfWork unitOfWork)
{
    public IAsyncPagedEnumerable<UserDto> GetActiveUsers()
    {
        var spec = QuerySpecification
            .For<User>()
            .Where(u => u.IsActive)
            .Select(u => new UserDto(u.KeyId, u.Name));

        return repository.FetchAsync(spec);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var spec = QuerySpecification
            .For<User>()
            .Where(u => u.KeyId == id)
            .Select(u => u);

        return await repository.FetchSingleOrDefaultAsync(spec, ct);
    }

    public async Task CreateUserAsync(User user, CancellationToken ct)
    {
        await repository.AddAsync([user], ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
```

### Bulk Updates with EntityUpdater

```csharp
using System.Entities;

// Using the fluent API
var updater = EntityUpdater
    .For<Product>()
    .SetProperty(p => p.Price, p => p.Price * 1.1m)
    .SetProperty(p => p.LastUpdated, DateTime.UtcNow);

var spec = QuerySpecification
    .For<Product>()
    .Where(p => p.IsActive)
    .Select(p => p);

int updated = await repository.UpdateAsync(spec, updater, ct);
```

### Unit of Work with Transactions

```csharp
using System.Entities;

public class OrderService(IUnitOfWork unitOfWork)
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var orderRepo = unitOfWork.GetRepository<IRepository<Order>>();

            await orderRepo.AddAsync([order], ct);
            await unitOfWork.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
```

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register unit of work
services.AddXUnitOfWork<MyUnitOfWork>();

// Or with interface and implementation
services.AddXUnitOfWork<IUnitOfWork, MyUnitOfWork>();
```

## Repository Methods

| Method | Description |
|--------|-------------|
| `FetchAsync<TResult>(spec)` | Returns `IAsyncPagedEnumerable<TResult>` |
| `FetchSingleAsync<TResult>(spec)` | Returns single result or throws |
| `FetchSingleOrDefaultAsync<TResult>(spec)` | Returns single or default |
| `FetchFirstAsync<TResult>(spec)` | Returns first or throws |
| `FetchFirstOrDefaultAsync<TResult>(spec)` | Returns first or default |
| `AddAsync(entities)` | Adds entities, returns count |
| `UpdateAsync(entities)` | Updates entities, returns count |
| `UpdateAsync(spec, expression)` | Bulk update with expression |
| `UpdateAsync(spec, action)` | Bulk update with action |
| `UpdateAsync(spec, updater)` | Bulk update with EntityUpdater |
| `DeleteAsync(spec)` | Bulk delete, returns count |

## Core Types

| Type | Description |
|------|-------------|
| `IEntity` | Entity with lifecycle metadata |
| `IEntity<TKey>` | Typed entity with key |
| `IRepository<TEntity>` | Generic async repository |
| `IUnitOfWork` | Transaction coordinator |
| `IQuerySpecification<TEntity, TResult>` | Query specification |
| `QuerySpecification` | Fluent query builder factory |
| `EntityUpdater` | Bulk update builder factory |
| `EntityUpdater<TEntity>` | Typed update builder |

## License

Apache License 2.0
        .HasJsonDocumentComparer();
});
```

### ReadOnlyMemory<byte> Converter

```csharp
using System.Entities.Data.Converters;

public class BinaryData : IEntity
{
    public Guid Id { get; set; }
    public ReadOnlyMemory<byte> Content { get; set; }
    
    // IEntity properties...
}

// In DataContext.OnModelCreating
modelBuilder.Entity<BinaryData>(entity =>
{
    entity.Property(e => e.Content)
        .HasReadOnlyMemoryToByteArrayConversion();
});
```

---

## 📊 Extension Methods Summary

### IServiceCollection Extensions

| Method | Description |
|--------|-------------|
| `AddXDataContext<T>()` | Registers DataContext with DbContextOptions |
| `AddXDataContextFactory<T>()` | Registers IDbContextFactory for factory pattern |
| `AddXEntityFrameworkRepositories()` | Registers base IRepository and IUnitOfWork |
| `AddXEntityFrameworkRepositories<T>()` | Registers typed IRepository<T> and IUnitOfWork<T> |

### PropertyBuilder Extensions

| Method | Description |
|--------|-------------|
| `HasJsonDocumentConversion()` | Configures JsonDocument value converter |
| `HasJsonDocumentComparer()` | Configures JsonDocument value comparer |
| `HasReadOnlyMemoryToByteArrayConversion()` | Configures ReadOnlyMemory<byte> converter |

---

## ✅ Best Practices

### ✅ Do

- **Inherit from `DataContext`** — Get automatic entity lifecycle tracking
- **Implement `IEntity`** — Enable automatic `CreatedOn`/`UpdatedOn`/`DeletedOn`
- **Use `FetchAsync` with projections** — Select only needed columns with DTOs
- **Set `IsUnitOfWorkEnabled = false`** — For immediate SQL execution (bulk operations)
- **Use `EntityUpdater`** — For efficient bulk updates via `ExecuteUpdate`
- **Use `IUnitOfWork` for transactions** — Coordinate multiple operations

### ❌ Don't

- **Mix repository and direct DbContext access** — Choose one approach
- **Forget `IsUnitOfWorkEnabled`** — Default is `true`, requiring explicit save
- **Use `ExecuteUpdate`/`ExecuteDelete` with tracking** — These bypass change tracking

---

## ⚡ Performance Tips

```csharp
// FetchAsync applies AsNoTracking by default for read-only queries
var users = await repository
    .FetchAsync<User, User>(q => q.Where(u => u.IsActive))
    .ToListAsync(ct);

// Projections reduce data transfer
var names = await repository
    .FetchAsync<User, string>(q => q
        .Where(u => u.IsActive)
        .Select(u => u.Name))
    .ToListAsync(ct);

// EntityUpdater with IsUnitOfWorkEnabled = false for single SQL statement
repository.IsUnitOfWorkEnabled = false;
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.LastUpdated, DateTime.UtcNow);

await repository.UpdateAsync(
    q => q.Where(u => u.IsActive),
    updater,
    ct);
// Single UPDATE statement, no entity loading
```

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Entities** | Core entity abstractions (`IEntity`, `IRepository`, `IUnitOfWork`) |
| **System.Events.Data** | Event Store and Outbox EF Core implementations |
| **Microsoft.EntityFrameworkCore** | EF Core framework |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

