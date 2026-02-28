# System.Entities.EntityFramework

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Entities.EntityFramework.svg)](https://www.nuget.org/packages/Xpandables.Entities.EntityFramework)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Entities.EntityFramework.svg)](https://www.nuget.org/packages/Xpandables.Entities.EntityFramework)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Entity Framework Core implementation of repository and unit-of-work abstractions from `System.Entities`.

## 📖 Overview

`System.Entities.EntityFramework` (NuGet: **Xpandables.Entities.EntityFramework**) provides `DataContext` (a `DbContext` subclass with entity lifecycle tracking), `EntityRepository<TEntity>` backed by EF Core, `EntityUnitOfWork` with `SaveChangesAsync`, and value converters for `JsonDocument` and `ReadOnlyMemory<byte>`. Namespace: `System.Entities.EntityFramework`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `DataContext` | `DataContext.cs` | `DbContext` subclass — auto-sets `CreatedOn`/`UpdatedOn` on tracked entities |
| `EntityRepository<TEntity>` | `EntityRepository.cs` | `IEntityRepository<TEntity>` backed by `DbSet<TEntity>` |
| `EntityUnitOfWork` | `EntityUnitOfWork.cs` | `IEntityUnitOfWork` — wraps `DataContext.SaveChangesAsync` |
| `EntityUnitOfWorkTransaction` | `EntityUnitOfWorkTransaction.cs` | EF Core database transaction wrapper |
| `EntityUpdaterExtensions` | `EntityUpdaterExtensions.cs` | EF Core–specific update helpers |
| `IEntityPropertyUpdateExtensions` | `IEntityPropertyUpdateExtensions.cs` | Property update extensions for EF |

### 🔄 Value Converters

| Type | File | Description |
|------|------|-------------|
| `JsonDocumentValueConverter` | `Converters/JsonDocumentValueConverter.cs` | `JsonDocument` ↔ string |
| `ReadOnlyMemoryToByteArrayConverter` | `Converters/ReadOnlyMemoryToByteArrayConverter.cs` | `ReadOnlyMemory<byte>` ↔ `byte[]` |
| `ConverterExtensions` | `Converters/ConverterExtensions.cs` | Registration helpers |

### ⚙️ Dependency Injection

```csharp
services.AddXEntityRepository();               // IEntityRepository<T> → EntityRepository<T>
services.AddXEntityUnitOfWork<MyDataContext>();  // IEntityUnitOfWork → EntityUnitOfWork
```

## 📦 Installation

```bash
dotnet add package Xpandables.Entities.EntityFramework
```

**Dependencies:** `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Relational`  
**Project References:** `Xpandables.Entities`

## 🚀 Quick Start

```csharp
using System.Entities.EntityFramework;

public class AppDbContext : DataContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}

// Register
services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
services.AddXEntityUnitOfWork<AppDbContext>();
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
