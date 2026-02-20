# System.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Data.svg)](https://www.nuget.org/packages/Xpandables.Data)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Data.svg)](https://www.nuget.org/packages/Xpandables.Data)
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
- **`IDataDbConnectionFactory`** — Database connection factory
- **`DataDbConnectionFactory`** — Default connection factory
- **`IDataDbConnectionScope`** — Scoped connection wrapper
- **`DataDbConnectionScope`** — Default scoped connection
- **`IDataDbConnectionFactoryProvider`** — Provider for connection factories
- **`DataDbConnectionFactoryProvider`** — Default provider
- **`DbProviders`** — Provider constants (`MsSqlServer`, `PostgreSql`, `MySql`)

### SQL Mapping
- **`IDataSqlMapper`** — Maps specifications to SQL
- **`DataSqlMapper`** — Default SQL mapper implementation

### Entity Updates
- **`DataUpdater`** — Fluent API for building ADO.NET update operations
- **`IDataPropertyUpdate`** — Property update abstraction

### Command Interceptor
- **`IDataCommandInterceptor`** — Intercept command execution for logging, telemetry, and diagnostics
- **`DataCommandInterceptor`** — Base no-op implementation with virtual methods for selective override
- **`DataLoggingCommandInterceptor`** — Default structured logging implementation using `[LoggerMessage]` source generation
- **`DataCommandInterceptorOptions`** — Options for sensitive data logging, custom category, and slow command threshold
- **`DataCommandContext`** — Context record with SQL text, parameters, operation type, and entity name
- **`DataCommandOperationType`** — Enum: `Reader`, `Scalar`, `NonQuery`

## Installation

```bash
dotnet add package Xpandables.Data
```

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register SQL Server connection
services.AddXDataDbConnectionMsSqlServer(connectionString);

// Or PostgreSQL
services.AddXDataDbConnectionPostgreSql(connectionString);

// Or MySQL
services.AddXDataDbConnectionMySql(connectionString);

// Register SQL builder
services.AddXDataMsSqlBuilder();       // SQL Server
services.AddXDataPostgreSqlBuilder();  // PostgreSQL
services.AddXDataMySqlBuilder();       // MySQL

// Register SQL mapper
services.AddXDataSqlMapper();

// Register connection scope
services.AddXDataDbConnectionScope();

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
                .GetRepository<Order>();

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

### Command Interceptor

A `DataLoggingCommandInterceptor` is registered automatically when you call `AddXDataUnitOfWork()`. It logs all command execution using structured `ILogger` output with `[LoggerMessage]` source generation:

| Event | Log Level | Content |
|---|---|---|
| Before execution | `Debug` | SQL text, parameter names (or values if enabled) |
| After execution | `Information` | Duration, rows affected, SQL text |
| Slow command | `Warning` | Duration exceeding threshold |
| Failed execution | `Error` | Duration, SQL text, exception |

#### Configure Options

```csharp
services.AddXDataCommandInterceptor(options =>
{
    // Log parameter values (default: false — only names are logged)
    options.EnableSensitiveDataLogging = true;

    // Custom log category (default: null — uses DataLoggingCommandInterceptor type name)
    options.CategoryName = "MyApp.Database";

    // Commands exceeding this threshold log at Warning (default: null — disabled)
    options.SlowCommandThreshold = TimeSpan.FromSeconds(2);
});

services.AddXDataUnitOfWork(); // safe — TryAdd won't overwrite
```

#### Custom Interceptor

To fully replace the logging interceptor with your own implementation:

```csharp
public sealed class MetricsCommandInterceptor : DataCommandInterceptor
{
    public override ValueTask CommandExecutedAsync(
        DataCommandContext context, TimeSpan duration, int? rowsAffected,
        CancellationToken cancellationToken = default)
    {
        // Push metrics to your observability system
        return ValueTask.CompletedTask;
    }
}

services.AddXDataCommandInterceptor<MetricsCommandInterceptor>();
services.AddXDataUnitOfWork();
```

**Registration behavior:**
- `AddXDataUnitOfWork()` auto-registers the `DataLoggingCommandInterceptor` via `TryAdd` (won't overwrite a previously registered custom interceptor).
- `AddXDataCommandInterceptor(Action?)` configures options via the `IOptions<T>` pipeline and registers the logging interceptor.
- `AddXDataCommandInterceptor<T>()` uses `Replace` to swap the current registration.
- If you register your custom interceptor **before** `AddXDataUnitOfWork()`, it is preserved.
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
| `IDataDbConnectionFactory` | Connection factory |
| `DataUpdater` | Fluent update builder |
| `IDataRequiresUnitOfWork` | Pipeline UoW marker |
| `IDataCommandInterceptor` | Command execution interceptor |
| `DataLoggingCommandInterceptor` | Default structured logging interceptor |
| `DataCommandInterceptorOptions` | Interceptor configuration options |

## DI Extension Methods

| Method | Description |
|--------|-------------|
| `AddXDataDbConnectionMsSqlServer(cs)` | Register SQL Server connection |
| `AddXDataDbConnectionPostgreSql(cs)` | Register PostgreSQL connection |
| `AddXDataDbConnectionMySql(cs)` | Register MySQL connection |
| `AddXDataMsSqlBuilder()` | Register SQL Server SQL builder |
| `AddXDataPostgreSqlBuilder()` | Register PostgreSQL SQL builder |
| `AddXDataMySqlBuilder()` | Register MySQL SQL builder |
| `AddXDataSqlMapper()` | Register SQL mapper |
| `AddXDataUnitOfWork()` | Register unit of work (auto-registers logging interceptor) |
| `AddXDataCommandInterceptor()` | Register logging interceptor with default options |
| `AddXDataCommandInterceptor(Action)` | Register logging interceptor with custom options |
| `AddXDataCommandInterceptor<T>()` | Replace interceptor with custom implementation |
| `AddXDataRepositories(assemblies)` | Register repositories from assemblies |
| `AddXDataDbConnectionScope()` | Register scoped connection |

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Entities** | Entity abstractions with lifecycle tracking |
| **Xpandables.Events.Data** | Event store built on System.Data |

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
