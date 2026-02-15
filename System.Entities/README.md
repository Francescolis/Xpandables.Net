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
- **`IEntity<TKey>`** — Typed entity with strongly-typed key identifier (`TKey : notnull, IComparable`)
- **`EntityStatus`** — Strongly-typed status primitive (`ACTIVE`, `PENDING`, `DELETED`, `SUSPENDED`, `PROCESSING`, `PUBLISHED`, `ACCEPTED`, `DUPLICATE`, `ONERROR`)

### Repository Pattern
- **`IEntityRepository`** — Base repository marker interface (implements `IDisposable`, `IAsyncDisposable`)
- **`IEntityRepository<TEntity>`** — Generic repository with async CRUD operations
- **`IAmbientContextReceiver<TContext>`** — For receiving ambient context data
- **`IEntityRequiresUnitOfWork`** — Marker for handlers requiring transactional scope

### Query Specifications
- **`QuerySpecification`** — Static factory with `For<TEntity>()` method
- **`QuerySpecificationBuilder<TEntity>`** — Fluent builder with `Where`, `OrderBy`, `Include`, `Skip`, `Take`, `Page`, `Distinct`, `WithTracking`, `Select`
- **`IQuerySpecification<TEntity, TResult>`** — Type-safe query specification
- **`IIncludeSpecification<TEntity>`** — Eager loading specification
- **`IOrderSpecification<TEntity>`** — Ordering specification

### Unit of Work
- **`IEntityUnitOfWork`** — Transaction coordination with `GetRepository<T>()`, `BeginTransactionAsync()`, `SaveChangesAsync()`
- **`IEntityUnitOfWorkTransaction`** — Transaction abstraction with `CommitAsync()`, `RollbackAsync()`
- **`EntityUnitOfWorkDbTransaction`** — DbTransaction wrapper

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

    // IEntity lifecycle properties
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
    .Skip(0)
    .Take(20)
    .Select(u => new UserDto(u.KeyId, u.Name, u.Email));

// With paging helper
var pagedSpec = QuerySpecification
    .For<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .Page(pageIndex: 0, pageSize: 20)
    .Select(u => new UserDto(u.KeyId, u.Name, u.Email));

// With includes and tracking
var detailedSpec = QuerySpecification
    .For<Order>()
    .Where(o => o.Status == EntityStatus.PENDING)
    .Include(o => o.Customer)
    .WithTracking()
    .Select(o => o);
```

### Use the Repository

```csharp
using System.Entities;

public class UserService(
    IEntityRepository<User> repository,
    IEntityUnitOfWork unitOfWork)
{
    public IAsyncPagedEnumerable<UserDto> GetActiveUsers()
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

// Via UnitOfWork (tracked, can rollback)
int updated = await repository.UpdateAsync(spec, updater, ct);
await unitOfWork.SaveChangesAsync(ct);

// Bulk (immediate, bypasses UoW)
int bulkUpdated = await repository.UpdateBulkAsync(spec, updater, ct);
```

### Unit of Work with Transactions

```csharp
using System.Entities;

public class OrderService(IEntityUnitOfWork unitOfWork)
{
    public async Task PlaceOrderAsync(
        Order order, CancellationToken ct)
    {
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
    }
}
```

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register unit of work
services.AddXEntityUnitOfWork<MyEntityUnitOfWork>();

// Or with interface and implementation
services.AddXEntityUnitOfWork<IEntityUnitOfWork, MyEntityUnitOfWork>();

// Keyed registration for multiple contexts
services.AddXEntityUnitOfWorkKeyed<MyEntityUnitOfWork>("orders");
```

---

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
| `UpdateAsync(spec, expression)` | Bulk update with expression (via UoW) |
| `UpdateAsync(spec, action)` | Bulk update with action (via UoW) |
| `UpdateAsync(spec, updater)` | Bulk update with EntityUpdater (via UoW) |
| `UpdateBulkAsync(spec, updater)` | Immediate bulk update (bypasses UoW) |
| `DeleteAsync(spec)` | Delete matching entities (via UoW) |
| `DeleteBulkAsync(spec)` | Immediate bulk delete (bypasses UoW) |

## Core Types

| Type | Description |
|------|-------------|
| `IEntity` | Entity with lifecycle metadata |
| `IEntity<TKey>` | Typed entity with key (`TKey : notnull, IComparable`) |
| `EntityStatus` | Strongly-typed status primitive |
| `IEntityRepository<TEntity>` | Generic async repository |
| `IEntityUnitOfWork` | Transaction coordinator |
| `IEntityUnitOfWorkTransaction` | Transaction with commit/rollback |
| `IQuerySpecification<TEntity, TResult>` | Query specification |
| `QuerySpecification` | Fluent query builder factory |
| `EntityUpdater` | Bulk update builder factory |
| `IEntityRequiresUnitOfWork` | Marker for pipeline UoW integration |

---

## 📚 Related Packages

- **Xpandables.Entities.EntityFramework** — EF Core implementation of repository and unit of work
- **Xpandables.Events** — Domain event contracts
- **Xpandables.Results** — Result types for handler responses

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
