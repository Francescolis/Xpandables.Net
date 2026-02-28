# System.Entities

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Entities.svg)](https://www.nuget.org/packages/Xpandables.Entities)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Entities.svg)](https://www.nuget.org/packages/Xpandables.Entities)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Entity abstractions, repository pattern, and unit-of-work contracts for domain-driven persistence.

## 📖 Overview

`System.Entities` (NuGet: **Xpandables.Entities**) defines `IEntity<TKey>` for domain entities with lifecycle metadata, `IEntityRepository<TEntity>` for async CRUD with query specifications, and `IEntityUnitOfWork` for transactional persistence. Namespace: `System.Entities`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

### 🏷️ Entity

| Type | File | Description |
|------|------|-------------|
| `IEntity` | `IEntity.cs` | Base — `KeyId`, `Status`, `CreatedOn`, `UpdatedOn`, `DeletedOn`, `IsDeleted` |
| `IEntity<TKey>` | `IEntity.cs` | Strongly-typed key (`TKey : notnull, IComparable`) |
| `Entity` | `Entity.cs` | Abstract base implementation |
| `EntityStatus` | `EntityStatus.cs` | Status constants |

### 📦 Repository

| Type | File | Description |
|------|------|-------------|
| `IEntityRepository` | `IEntityRepository.cs` | Marker (`IDisposable`, `IAsyncDisposable`) |
| `IEntityRepository<TEntity>` | `IEntityRepository.cs` | `FetchAsync`, `FetchSingleAsync`, `FetchSingleOrDefaultAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` |

### 🔍 Query Specification

| Type | File | Description |
|------|------|-------------|
| `IQuerySpecification<TEntity, TResult>` | `IQuerySpecification.cs` | Defines filtering, projection, ordering, paging |
| `QuerySpecification` | `QuerySpecification.cs` | Fluent builder via `QuerySpecification.For<TEntity>()` |

### 🔄 Unit of Work

| Type | File | Description |
|------|------|-------------|
| `IEntityUnitOfWork` | `IEntityUnitOfWork.cs` | `SaveChangesAsync` contract |
| `IEntityUnitOfWorkTransaction` | `IEntityUnitOfWorkTransaction.cs` | Transaction abstraction |
| `EntityUnitOfWorkDbTransaction` | `EntityUnitOfWorkDbTransaction.cs` | Database transaction wrapper |
| `IEntityRequiresUnitOfWork` | `IEntityRequiresUnitOfWork.cs` | Marker for requests needing EF transactions |

### 🛠️ Utilities

| Type | File | Description |
|------|------|-------------|
| `EntityUpdater` | `EntityUpdater.cs` | Bulk update builder |
| `IEntityPropertyUpdate` | `IEntityPropertyUpdate.cs` | Property update abstraction |
| `IAmbientContextReceiver` | `IAmbientContextReceiver.cs` | Ambient context injection |

## 📦 Installation

```bash
dotnet add package Xpandables.Entities
```

**Project References:** `Xpandables.AsyncPaged`

## 🚀 Quick Start

```csharp
using System.Entities;

// Fetch with specification
var spec = QuerySpecification.For<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .Take(10)
    .Select(u => new UserDto(u.Id, u.Name));

var users = repository.FetchAsync(spec, ct);
await foreach (var user in users) { /* ... */ }
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
