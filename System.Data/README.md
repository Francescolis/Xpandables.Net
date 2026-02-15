# System.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Data.svg)](https://www.nuget.org/packages/Xpandables.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

ADO.NET repository, unit of work, and SQL builder infrastructure for .NET.

## Overview

`System.Data` provides a lightweight ADO.NET data access layer with Repository and Unit of Work patterns, type-safe query specifications, SQL builders for multiple dialects, and connection management. All operations execute immediately against the database (no change tracking).

Built for .NET 10 with full async support.

## Features

### Repository Pattern
- **`IDataRepository`** — Base repository marker (implements `IDisposable`, `IAsyncDisposable`)
- **`IDataRepository<TData>`** — Generic repository with `QueryAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync`
- **`DataRepository<TData>`** — Default ADO.NET repository implementation
- **`IDataRequiresUnitOfWork`** — Marker for pipeline UoW integration

### Query Specifications
- **`DataSpecification`** — Static factory with `For<TData>()` method
- **`IDataSpecification<TData, TResult>`** — Type-safe query specification

### Unit of Work
- **`IDataUnitOfWork`** — Transaction management with `GetRepository<T>()`, `BeginTransactionAsync()`
- **`DataUnitOfWork`** — Default ADO.NET unit of work implementation
- **`IDataTransaction`** — Transaction abstraction with `CommitAsync()`, `RollbackAsync()`
- **`DataTransaction`** — Default transaction implementation

### SQL Builders
- **`IDataSqlBuilder`** — SQL builder interface
- **`MsDataSqlBuilder`** — SQL Server SQL builder
- **`PostgreDataSqlBuilder`** — PostgreSQL SQL builder
- **`MyDataSqlBuilder`** — MySQL SQL builder
- **`DataSqlBuilderFactory`** — Factory for SQL builders
- **`DataSqlBuilderBase`** — Base class for custom builders

### Connection Management
- **`IDbConnectionFactory`** — Database connection factory
- **`DbConnectionFactory`** — Default connection factory
- **`IDbConnectionScope`** — Scoped connection wrapper
- **`DbConnectionScope`** — Default scoped connection
- **`IDbConnectionFactoryProvider`** — Provider for connection factories
- **`DbConnectionFactoryProvider`** — Default provider
- **`DbProviders`** — Provider constants (`MsSqlServer`, `PostgreSql`, `MySql`)

### SQL Mapping
- **`IDataSqlMapper`** — Maps specifications to SQL
- **`DataSqlMapper`** — Default SQL mapper implementation

### Entity Updates
- **`DataUpdater`** — Fluent API for building ADO.NET update operations
- **`IDataPropertyUpdate`** — Property update abstraction

## Installation

```bash
dotnet add package Xpandables.Data
```

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register SQL Server connection
services.AddXDbConnectionMsSqlServer(connectionString);

// Or PostgreSQL
services.AddXDbConnectionPostgreSql(connectionString);

// Or MySQL
services.AddXDbConnectionMySql(connectionString);

// Register SQL builder
services.AddXMsSqlBuilder();       // SQL Server
services.AddXPostgreSqlBuilder();  // PostgreSQL
services.AddXMySqlBuilder();       // MySQL

// Register SQL mapper
services.AddXSqlMapper();

// Register connection scope
services.AddXDbConnectionScope();

// Register unit of work and repositories
services.AddXDataUnitOfWork();
services.AddXDataRepositories(typeof(Program).Assembly);
```

### Use the Repository

```csharp
using System.Data;

public class OrderService(IDataUnitOfWork unitOfWork)
{
    public async Task CreateOrderAsync(
        Order order, CancellationToken ct)
    {
        await using var transaction = await unitOfWork
            .BeginTransactionAsync(ct);

        try
        {
            var repo = unitOfWork
                .GetRepository<IDataRepository<Order>>();

            await repo.InsertAsync([order], ct);

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

### Query with Specifications

```csharp
using System.Data;

var spec = DataSpecification
    .For<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Skip(0)
    .Take(20)
    .Select(p => new ProductDto(p.Id, p.Name, p.Price));

await foreach (var product in repository.QueryAsync(spec, ct))
{
    Console.WriteLine(product.Name);
}
```

---

## Core Types

| Type | Description |
|------|-------------|
| `IDataRepository<TData>` | Generic ADO.NET repository |
| `IDataUnitOfWork` | ADO.NET transaction coordinator |
| `IDataTransaction` | Transaction with commit/rollback |
| `DataSpecification` | Query specification factory |
| `IDataSqlBuilder` | SQL builder interface |
| `IDbConnectionFactory` | Connection factory |
| `DataUpdater` | Fluent update builder |
| `IDataRequiresUnitOfWork` | Pipeline UoW marker |

## DI Extension Methods

| Method | Description |
|--------|-------------|
| `AddXDbConnectionMsSqlServer(cs)` | Register SQL Server connection |
| `AddXDbConnectionPostgreSql(cs)` | Register PostgreSQL connection |
| `AddXDbConnectionMySql(cs)` | Register MySQL connection |
| `AddXMsSqlBuilder()` | Register SQL Server SQL builder |
| `AddXPostgreSqlBuilder()` | Register PostgreSQL SQL builder |
| `AddXMySqlBuilder()` | Register MySQL SQL builder |
| `AddXSqlMapper()` | Register SQL mapper |
| `AddXDataUnitOfWork()` | Register unit of work |
| `AddXDataRepositories(assemblies)` | Register repositories from assemblies |
| `AddXDbConnectionScope()` | Register scoped connection |

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Entities** | Entity abstractions with lifecycle tracking |
| **Xpandables.Events.Data** | Event store built on System.Data |

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
