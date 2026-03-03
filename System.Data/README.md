# System.Data

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Data.svg)](https://www.nuget.org/packages/Xpandables.Data)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Data.svg)](https://www.nuget.org/packages/Xpandables.Data)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

ADO.NET repository, unit-of-work, and SQL builder abstractions for raw database access.

## 📖 Overview

`System.Data` (NuGet: **Xpandables.Data**) provides `IDataRepository<TData>` for CRUD operations, `IDataUnitOfWork` for transaction management, and `IDataSqlBuilder` for generating provider-specific SQL. Supports SQL Server, PostgreSQL, and MySQL dialects. Namespace: `System.Data`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

### 📊 Repository

| Type | File | Description |
|------|------|-------------|
| `IDataRepository` | `IDataRepository.cs` | Marker interface (`IDisposable`, `IAsyncDisposable`) |
| `IDataRepository<TData>` | `IDataRepository.cs` | Generic — `QueryAsync`, `QueryPagedAsync`, `QuerySingleAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` |
| `DataRepository` | `DataRepository.cs` | Default implementation |

### 🔄 Unit of Work

| Type | File | Description |
|------|------|-------------|
| `IDataUnitOfWork` | `IDataUnitOfWork.cs` | `ConnectionScope`, `GetRepository<T>`, `BeginTransactionAsync` |
| `DataUnitOfWork` | `DataUnitOfWork.cs` | Default implementation |
| `IDataUnitOfWorkFactory` | `IDataUnitOfWorkFactory.cs` | Factory for creating unit of work instances |
| `DataUnitOfWorkFactory` | `DataUnitOfWorkFactory.cs` | Default factory |
| `IDataRequiresUnitOfWork` | `IDataRequiresUnitOfWork.cs` | Marker for requests requiring transactions |

### 🔗 Connection Management

| Type | File | Description |
|------|------|-------------|
| `IDataDbConnectionFactory` | `IDataDbConnectionFactory.cs` | Creates `DbConnection` instances |
| `DataDbConnectionFactory` | `DataDbConnectionFactory.cs` | Default implementation |
| `IDataDbConnectionScope` | `IDataDbConnectionScope.cs` | Scoped connection lifecycle |
| `DataDbConnectionScope` | `DataDbConnectionScope.cs` | Default scope |
| `IDataTransaction` | `IDataTransaction.cs` | Transaction abstraction |
| `DataTransaction` | `DataTransaction.cs` | Default transaction |

### 🏗️ SQL Building

| Type | File | Description |
|------|------|-------------|
| `IDataSqlBuilder` | `IDataSqlBuilder.cs` | Contract — SELECT, INSERT, UPDATE, DELETE generation |
| `DataSqlBuilderBase` | `DataSqlBuilderBase.cs` | Abstract base |
| `DataSqlBuilderFactory` | `DataSqlBuilderFactory.cs` | Factory for SQL builder creation |
| `MsDataSqlBuilder` | `MsDataSqlBuilder.cs` | SQL Server (T-SQL) |
| `PostgreDataSqlBuilder` | `PostgreDataSqlBuilder.cs` | PostgreSQL |
| `MyDataSqlBuilder` | `MyDataSqlBuilder.cs` | MySQL |
| `SqlDialect` | `SqlDialect.cs` | Enum — `SqlServer`, `PostgreSql`, `MySql` |
| `SqlQueryResult` | `IDataSqlBuilder.cs` | Record struct — `Sql` + `Parameters` |

### 🔍 Specification & Interceptors

| Type | File | Description |
|------|------|-------------|
| `IDataSpecification<TData, TResult>` | `IDataSpecification.cs` | Query specification for filtering/paging |
| `DataSpecification` | `DataSpecification.cs` | Default implementation |
| `IDataCommandInterceptor` | `IDataCommandInterceptor.cs` | Command interception |
| `DataCommandInterceptor` | `DataCommandInterceptor.cs` | Base implementation |
| `DataLoggingCommandInterceptor` | `DataLoggingCommandInterceptor.cs` | Logging interceptor |
| `DataCommandInterceptorOptions` | `DataCommandInterceptorOptions.cs` | Interceptor configuration |

## 📦 Installation

```bash
dotnet add package Xpandables.Data
```

**Dependencies:** `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`  
**Project References:** `Xpandables.AsyncPaged`

## 🚀 Quick Start

### Basic CRUD with Unit of Work

```csharp
using System.Data;

// Create a unit of work (scoped connection)
await using IDataUnitOfWork unitOfWork = await unitOfWorkFactory.CreateAsync(ct);

// Start a transaction
await using IDataTransaction transaction = await unitOfWork.BeginTransactionAsync(ct);

// Get typed repositories
IDataRepository<Order> orderRepo = unitOfWork.GetRepository<Order>();
IDataRepository<OrderItem> itemRepo = unitOfWork.GetRepository<OrderItem>();

// Insert
var order = new Order { CustomerId = customerId, Total = 149.99m };
await orderRepo.InsertAsync(order, ct);

// Insert related items
var items = new[]
{
    new OrderItem { OrderId = order.Id, ProductName = "Widget", Qty = 2 },
    new OrderItem { OrderId = order.Id, ProductName = "Gadget", Qty = 1 }
};
await itemRepo.InsertAsync(items, ct);

// Commit transaction — rolls back automatically on failure
await transaction.CommitAsync(ct);
```

### Query with Specification

```csharp
// Define a specification for filtering and projection
var spec = new DataSpecification<Order, OrderSummary>
{
    Criteria = o => o.Status == "Active" && o.Total > 100,
    OrderBy = o => o.CreatedOn,
    PageSize = 20,
    CurrentPage = 1,
    Projection = o => new OrderSummary(o.Id, o.CustomerName, o.Total)
};

// Query — returns IAsyncEnumerable
await foreach (OrderSummary summary in orderRepo.QueryAsync(spec, ct))
{
    Console.WriteLine($"{summary.Id}: {summary.CustomerName} — {summary.Total:C}");
}

// Paged query — returns IAsyncPagedEnumerable with pagination metadata
IAsyncPagedEnumerable<OrderSummary> pagedResults = orderRepo.QueryPagedAsync(spec, ct);

await foreach (OrderSummary summary in pagedResults)
{
    Console.WriteLine(summary.CustomerName);
}

Pagination pagination = await pagedResults.GetPaginationAsync(ct);
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

### Single-Item Queries

```csharp
// Throws InvalidOperationException if not exactly one
OrderSummary single = await orderRepo.QuerySingleAsync(spec, ct);

// Returns default if none found
OrderSummary? maybeOrder = await orderRepo.QuerySingleOrDefaultAsync(spec, ct);
```

### Update with DataUpdater

```csharp
// Build a typed update
var updater = new DataUpdater<Order>()
    .Set(o => o.Status, "Completed")
    .Set(o => o.UpdatedOn, DateTime.UtcNow)
    .Where(o => o.Id == orderId);

await orderRepo.UpdateAsync(updater, ct);
```

### Delete

```csharp
await orderRepo.DeleteAsync(o => o.Id == orderId, ct);
```

### SQL Dialects

The `IDataSqlBuilder` generates provider-specific SQL. Built-in dialects:

```csharp
// SQL Server (T-SQL)   — MsDataSqlBuilder
// PostgreSQL            — PostgreDataSqlBuilder
// MySQL                 — MyDataSqlBuilder

SqlDialect dialect = SqlDialect.PostgreSql;
```

### Command Interceptor (Logging)

```csharp
// Register the logging interceptor to log all SQL commands
services.Configure<DataCommandInterceptorOptions>(options =>
{
    options.IsEnabled = true;
});
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
