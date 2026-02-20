# System.AsyncPaged

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Efficient async pagination for modern .NET applications. Stream large datasets with pagination metadata, minimal allocations, and AOT-friendly design.

## Overview

`System.AsyncPaged` provides a production-ready solution for implementing pagination in asynchronous scenarios. It extends `IAsyncEnumerable<T>` with a lightweight `IAsyncPagedEnumerable<T>` interface that carries pagination metadata, enabling efficient data streaming from databases, APIs, and cloud services.

Built for .NET 10 with AOT (Ahead-of-Time) compatibility and full support for modern async patterns.

## Features

- **`IAsyncPagedEnumerable<T>`** — Async enumerable with built-in pagination metadata (page size, current page, total count, continuation token)
- **Lazy Pagination** — Pagination metadata computed on-demand
- **Automatic Extraction** — Infers pagination from `IQueryable<T>` Skip/Take patterns
- **Cursor-Based & Offset Pagination** — Support for both pagination strategies via `CursorOptions<T>` and `CursorDirection`
- **Immutable `Pagination` Struct** — Thread-safe pagination state with navigation helpers
- **`PaginationStrategy` Enum** — Control pagination updates (None, PerPage, PerItem)
- **AOT Compatible** — Full native AOT support with JSON source generation
- **Zero Dependencies** — No Entity Framework, ASP.NET, or other framework dependencies

## Installation

```bash
dotnet add package Xpandables.AsyncPaged
```

## Quick Start

### Basic Enumeration

```csharp
using System.Collections.Generic;

IAsyncPagedEnumerable<User> users = GetUsersAsync();

// Enumerate items
await foreach (var user in users)
{
    Console.WriteLine($"{user.Name} ({user.Email})");
}

// Access pagination metadata
Pagination pagination = await users.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
Console.WriteLine($"Total items: {pagination.TotalCount}");
```

### With Entity Framework Core

```csharp
public IAsyncPagedEnumerable<Product> GetProductsAsync(int page = 1, int pageSize = 20)
{
    return _context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToAsyncPagedEnumerable(PaginationStrategy.None);
}
```

### Manual Pagination Control

```csharp
var pagination = Pagination.Create(pageSize: 25, currentPage: 1, totalCount: 500);
var items = GetItemsAsync();
var paged = items.ToAsyncPagedEnumerable(pagination);

// Navigate pages
var nextPage = pagination.NextPage();
var prevPage = pagination.PreviousPage();
```

### Cursor-Based Pagination

```csharp
var cursorOptions = new CursorOptions<Product>
{
    KeySelector = p => p.Id,
    Direction = CursorDirection.Forward
};

var paged = products.ToAsyncPagedEnumerable(cursorOptions);
```

## Core Types

| Type | Description |
|------|-------------|
| `IAsyncPagedEnumerable<T>` | Async enumerable with pagination support |
| `IAsyncPagedEnumerator<T>` | Enumerator with pagination awareness |
| `Pagination` | Immutable pagination metadata struct |
| `PaginationStrategy` | Enum controlling pagination update behavior |
| `CursorOptions<T>` | Configuration for cursor-based pagination |
| `CursorDirection` | Forward or backward cursor movement |

## Extension Methods

- `ToAsyncPagedEnumerable()` — Convert `IAsyncEnumerable<T>` to paged enumerable
- `GetPaginationAsync()` — Retrieve computed pagination metadata
- `GetArgumentType()` — Get the element type via reflection

## License

Apache License 2.0
    await foreach (var order in orders.WithCancellation(cancellationToken))
    {
        totalAmount += order.Amount;
        itemCount++;
    }
    
    var pagination = await orders.GetPaginationAsync(cancellationToken);
    
    return new PagedAggregation
    {
        TotalAmount = totalAmount,
        ItemCount = itemCount,
        Pagination = pagination
    };
}
```

### 📝 JSON Serialization

```csharp
using System.Text.Json;

var pagination = Pagination.Create(pageSize: 20, currentPage: 2, totalCount: 500);

// Serialize using the provided source-generated context
var json = JsonSerializer.Serialize(pagination, PaginationJsonContext.Default.Pagination);
Console.WriteLine(json);
// Output: {"totalCount":500,"pageSize":20,"currentPage":2,"continuationToken":null}

// Deserialize
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var deserialized = JsonSerializer.Deserialize<Pagination>(json, options);
```

---

## ✅ Best Practices

### ✅ Do

- **Use `await foreach`** for consumption — leverage `IAsyncEnumerable<T>` semantics naturally
- **Call `GetPaginationAsync()` explicitly** — ensures pagination metadata is fully computed before responding
- **Apply filters before pagination** — better query performance and smaller result sets
- **Stream large datasets** — avoid materializing entire collections; process items as they arrive
- **Use `CancellationToken` throughout** — especially with `GetPaginationAsync()` and enumeration
- **Cache total counts** — for expensive operations; avoid re-querying on every page request
- **Use continuation tokens** — for time-series or large ordered datasets to avoid expensive total count queries

### ❌ Don't

- **Block on async operations** — avoid `.Result` or `.Wait()`; use `await` or async all the way
- **Materialize unnecessarily** — don't call `ToList()` unless required
- **Ignore `CancellationToken`** — support graceful cancellation for long-running operations
- **Assume pagination is computed** — always call `GetPaginationAsync()` if you need up-to-date metadata
- **Share mutable state across multiple enumerations** — create new enumerables per usage

---

## 📖 API Reference

### 🔗 `IAsyncPagedEnumerable<T>` Interface

```csharp
public interface IAsyncPagedEnumerable<out T> : IAsyncEnumerable<T>, IAsyncPagedEnumerable
    where T : allows ref struct
{
    // Inherited from IAsyncPagedEnumerable:
    Pagination Pagination { get; }
    Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default);
    
    // Returns a paged enumerator:
    IAsyncPagedEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    
    // Configure pagination strategy:
    IAsyncPagedEnumerable<T> WithStrategy(PaginationStrategy strategy);
}
```

### 🔄 `IAsyncPagedEnumerator<T>` Interface

```csharp
public interface IAsyncPagedEnumerator<out T> : IAsyncEnumerator<T>
    where T : allows ref struct
{
    // Gets current pagination metadata by reference:
    ref readonly Pagination Pagination { get; }
    
    // Gets the active pagination strategy:
    PaginationStrategy Strategy { get; }
}
```

### 📊 `PaginationStrategy` Enum

```csharp
public enum PaginationStrategy
{
    // No pagination updates during enumeration
    None = 0,
    
    // Update pagination per page
    PerPage = 1,
    
    // Update pagination per item
    PerItem = 2
}
```

### 🧩 `Pagination` Struct

```csharp
public readonly record struct Pagination
{
    // Properties
    public required int? TotalCount { get; init; }
    public required int PageSize { get; init; }
    public required int CurrentPage { get; init; }
    public required string? ContinuationToken { get; init; }
    
    // Computed Properties
    public int Skip { get; }                    // (CurrentPage - 1) * PageSize
    public int Take { get; }                    // PageSize
    public int? TotalPages { get; }             // Ceiling(TotalCount / PageSize)
    public bool IsFirstPage { get; }            // CurrentPage <= 1
    public bool IsLastPage { get; }             // CurrentPage * PageSize >= TotalCount
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
    public bool HasContinuation { get; }        // ContinuationToken is not null/empty
    public bool IsUnknown { get; }              // TotalCount is null or < 0
    public bool IsPaginated { get; }            // Skip > 0 || Take > 0
    
    // Factory Methods
    public static Pagination Create(int pageSize, int currentPage, 
        string? continuationToken = null, int? totalCount = null);
    public static Pagination FromTotalCount(int totalCount);
    public static Pagination Empty { get; }
    
    // Navigation Methods
    public Pagination NextPage(string? continuationToken = null);
    public Pagination PreviousPage();
    public Pagination WithTotalCount(int totalCount);
}
```

### 🔧 Extension Methods

```csharp
// From IAsyncEnumerable<T>
public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    PaginationStrategy strategy = PaginationStrategy.None);

public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    int totalCount);

public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    Pagination pagination);

public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
    PaginationStrategy strategy = PaginationStrategy.None);

// From IQueryable<T>
public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IQueryable<T> source,
    PaginationStrategy strategy = PaginationStrategy.None);

public static IAsyncPagedEnumerable<T> ToAsyncPagedEnumerable<T>(
    this IQueryable<T> source,
    Func<CancellationToken, ValueTask<Pagination>> paginationFactory,
    PaginationStrategy strategy = PaginationStrategy.None);
```

---

## 🔧️ Extensibility

### 🏗️ Implementing a Custom Pagination Source

You can create custom pagination sources using the factory methods:

```csharp
public IAsyncPagedEnumerable<User> GetCustomPaginatedUsers(
    int pageNumber,
    int pageSize = 25)
{
    var items = FetchUsersAsync(pageNumber, pageSize);
    
    return items.ToAsyncPagedEnumerable(async ct =>
    {
        int totalCount = await GetTotalUserCountAsync(ct);
        return Pagination.Create(
            pageSize: pageSize,
            currentPage: pageNumber,
            totalCount: totalCount);
    }, PaginationStrategy.None);
}

// Or using AsyncPagedEnumerable.Create directly
public IAsyncPagedEnumerable<User> GetUsersWithFactory()
{
    var source = GetUsersAsyncEnumerable();
    
    return AsyncPagedEnumerable.Create(
        source,
        async ct =>
        {
            int total = await CountUsersAsync(ct);
            return Pagination.Create(pageSize: 20, currentPage: 1, totalCount: total);
        },
        PaginationStrategy.PerPage);
}

// Usage
var users = GetCustomPaginatedUsers(pageNumber: 2);
await foreach (var user in users)
{
    Console.WriteLine(user.Name);
}

var pagination = await users.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

---

## ⚙️ Performance Considerations

- **Lazy Pagination Computation** — Pagination metadata is only computed when `GetPaginationAsync()` is called; no overhead if not needed
- **Thread-Safe Initialization** — Uses lock-free atomic operations for lazy pagination computation
- **Struct Allocations** — `Pagination` is a readonly struct; all copies are stack-allocated
- **IQueryable Optimization** — Automatically extracts Skip/Take patterns without materializing data
- **Memory Streaming** — Process large datasets without holding entire collections in memory

---

## 📚 Related Packages

- **[System.Linq.AsyncPaged](https://www.nuget.org/packages/System.Linq.AsyncPaged)** — LINQ operators for async paged enumerables
- **[System.Net.Http.AsyncPaged](https://www.nuget.org/packages/System.Net.Http.AsyncPaged)** — REST API pagination support

---

## 📄 License & Contributing

Licensed under the **Apache License 2.0**. Copyright © Kamersoft 2025.

Contributions are welcome! Please visit [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net) to contribute, report issues, or request features.
