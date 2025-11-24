# ?? System.Data.Repositories

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Repository Pattern Abstractions** - Core interfaces and abstractions for Repository and Unit of Work patterns with support for LINQ queries, bulk operations, and transactions.

---

## ?? Overview

`System.Data.Repositories` provides the foundational abstractions for implementing the Repository and Unit of Work patterns. These interfaces are technology-agnostic and can be implemented by any data access framework (Entity Framework Core, Dapper, etc.).

### ? Key Features

- ?? **IRepository** - Generic repository interface with LINQ support
- ?? **IUnitOfWork** - Unit of Work abstraction for transaction management
- ? **Bulk Operations** - Interfaces for efficient batch operations
- ?? **EntityUpdater** - Fluent API for property updates
- ?? **IEntity** - Base entity interface with lifecycle tracking
- ?? **Async-First** - Full async/await support throughout
- ? **Technology Agnostic** - No specific ORM dependency

---

## ?? Quick Start

### Installation

```bash
dotnet add package System.Data.Repositories
```

### Core Interfaces

```csharp
using System.Data.Repositories;

// IRepository - Core repository interface
public interface IRepository : IDisposable, IAsyncDisposable
{
    // Control unit of work behavior
    bool IsUnitOfWorkEnabled { get; set; }
    
    // Query operations
    IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class;
    
    // Insert/Update/Delete operations
    Task AddAsync<TEntity>(CancellationToken cancellationToken, params TEntity[] entities) where TEntity : class;
    Task UpdateAsync<TEntity>(CancellationToken cancellationToken, params TEntity[] entities) where TEntity : class;
    Task DeleteAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, CancellationToken cancellationToken = default) where TEntity : class;
}

// IRepository<TDataContext> - Context-specific repository
public interface IRepository<TDataContext> : IRepository where TDataContext : class;
```

---

## ?? Interface Details

### IRepository

Full LINQ query support with async enumeration:

```csharp
IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
    Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
    CancellationToken cancellationToken = default)
    where TEntity : class;
```

Multiple update overloads for flexibility:

```csharp
// Update tracked entities
Task UpdateAsync<TEntity>(CancellationToken cancellationToken, params TEntity[] entities);

// Bulk update with expression
Task UpdateAsync<TEntity>(
    Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
    Expression<Func<TEntity, TEntity>> updateExpression,
    CancellationToken cancellationToken = default);

// Bulk update with action
Task UpdateAsync<TEntity>(
    Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
    Action<TEntity> updateAction,
    CancellationToken cancellationToken = default);

// Bulk update with EntityUpdater
Task UpdateAsync<TEntity>(
    Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
    EntityUpdater<TEntity> updater,
    CancellationToken cancellationToken = default);
```

### IUnitOfWork

Transaction and multi-repository management:

```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class, IRepository;
    
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### EntityUpdater

Fluent API for bulk property updates:

```csharp
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.IsActive, true)
    .SetProperty(u => u.UpdatedDate, DateTime.UtcNow)
    .SetProperty(u => u.LoginCount, u => u.LoginCount + 1);
```

### IEntity

Standard entity lifecycle interface:

```csharp
public interface IEntity
{
    DateTime CreatedOn { get; set; }
    DateTime? UpdatedOn { get; set; }
    DateTime? DeletedOn { get; set; }
    EntityStatus Status { get; set; }
}
```

---

## ?? Best Practices

1. **Use IRepository abstractions** - Keep business logic independent of data access technology
2. **Leverage async/await** - All operations support cancellation tokens
3. **Unit of Work mode** - Enable for batch operations, disable for immediate execution
4. **EntityUpdater for bulk updates** - More efficient than loading entities into memory
5. **IEntity for tracking** - Standardize entity lifecycle management
6. **Dispose properly** - Always dispose repositories and unit of work

---

## ?? Related Packages

- **System.Data.EntityFramework** - EF Core implementation
- **System.Linq.AsyncPaged** - Async pagination support
- **System.Collections.AsyncPaged** - Core async types

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
