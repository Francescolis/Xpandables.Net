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

```csharp
using System.Data;

await using var unitOfWork = await unitOfWorkFactory.CreateAsync(ct);
await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

var orderRepo = unitOfWork.GetRepository<Order>();
await orderRepo.InsertAsync(order, ct);
await transaction.CommitAsync(ct);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
