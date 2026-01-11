# 📖 Xpandables.AsyncPaged

[![NuGet](https://img.shields.io/badge/NuGet-10.0.1-blue.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **Efficient Async Pagination for Modern .NET** — Stream large datasets with pagination metadata, minimal allocations, and AOT-friendly design.

---

## 🎯 Overview

`Xpandables.AsyncPaged` provides a production-ready solution for implementing pagination in asynchronous scenarios. It extends `IAsyncEnumerable<T>` with a lightweight `IAsyncPagedEnumerable<T>` interface that carries pagination metadata, enabling efficient data streaming from databases, APIs, and cloud services without reinventing the wheel.

Built for .NET 10 with AOT (Ahead-of-Time) compatibility and full support for modern async patterns, this library eliminates boilerplate while maintaining ergonomic, allocation-conscious APIs.

### ✨ Key Features

- **`IAsyncPagedEnumerable<T>` Interface** — Composable async enumerable with built-in pagination metadata (page size, current page, total count, continuation token)
- **Lazy Pagination Computation** — Pagination metadata is computed on-demand, avoiding unnecessary overhead for simple scenarios
- **Automatic Pagination Extraction** — Intelligently infers pagination details from `IQueryable<T>` Skip/Take patterns or materializes from `IAsyncEnumerable<T>`
- **Cursor-Based & Offset Pagination** — Support for both offset-based and continuation token–based pagination strategies
- **Immutable `Pagination` Struct** — Efficient, thread-safe pagination state with helper methods (NextPage, PreviousPage, TotalPages, etc.)
- **AOT Compatible** — Full native AOT support with JSON source generation for serialization
- **System.Text.Json Integration** — Built-in `PaginationJsonContext` for high-performance JSON serialization
- **Zero Framework Baggage** — No Entity Framework, ASP.NET, or other dependencies; works with any async source

---

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged
```

Or via NuGet Package Manager:

```powershell
Install-Package Xpandables.AsyncPaged
```

---

## 🚀 Quick Start

### 📋 Basic Enumeration with Pagination

```csharp
using System.Collections.Generic;

// Get a paged enumerable from your async source
IAsyncPagedEnumerable<User> users = GetUsersAsync();

// Enumerate items asynchronously
await foreach (var user in users)
{
    Console.WriteLine($"{user.Name} ({user.Email})");
}

// Access pagination metadata
var pagination = await users.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages} pages");
Console.WriteLine($"Total items: {pagination.TotalCount}");
```

### 🗄️ With Entity Framework Core

```csharp
public IAsyncPagedEnumerable<Product> GetProductsAsync(int pageNumber = 1, int pageSize = 20)
{
    return _context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToAsyncPagedEnumerable(PaginationStrategy.None);  // Automatically extracts pagination from Skip/Take
}

// Usage
var products = GetProductsAsync(pageNumber: 2, pageSize: 50);
await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Showing {pagination.PageSize} items, page {pagination.CurrentPage} of {pagination.TotalPages}");
```

### 🎮 Manual Pagination Control

```csharp
var pagination = Pagination.Create(pageSize: 25, currentPage: 1, totalCount: 500);
var items = GetItemsAsync();
var paged = items.ToAsyncPagedEnumerable(pagination);

// Navigate pages
var nextPagePagination = pagination.NextPage();
var prevPagePagination = pagination.PreviousPage();

Console.WriteLine($"Current: {pagination.CurrentPage}");
Console.WriteLine($"Has next: {pagination.HasNextPage}");
Console.WriteLine($"Skip {pagination.Skip}, Take {pagination.Take}");

// Apply pagination strategy
var pagedWithStrategy = paged.WithStrategy(PaginationStrategy.PerPage);
```

---

## 📚 Core Concepts

### 🧩 The `Pagination` Struct

The `Pagination` struct is an immutable value type that encapsulates all pagination state:

```csharp
var pagination = Pagination.Create(
    pageSize: 20,
    currentPage: 3,
    totalCount: 250,
    continuationToken: null
);

// Computed properties
Console.WriteLine(pagination.TotalPages);      // 13 (computed)
Console.WriteLine(pagination.Skip);            // 40 (for SQL: (page-1) * size)
Console.WriteLine(pagination.Take);            // 20
Console.WriteLine(pagination.IsFirstPage);     // false
Console.WriteLine(pagination.IsLastPage);      // false
Console.WriteLine(pagination.HasNextPage);     // true
Console.WriteLine(pagination.HasPreviousPage); // true

// Navigation
var next = pagination.NextPage("continuation_token_xyz");
var prev = pagination.PreviousPage();

```

Key properties:
- **`PageSize`** — Items per page
- **`CurrentPage`** — Current page number (1-based)
- **`TotalCount`** — Total items across all pages (nullable if unknown)
- **`ContinuationToken`** — Token for cursor-based pagination (nullable)
- **`Skip`** — Calculated skip offset for database queries
- **`Take`** — Calculated take count (same as PageSize)
- **`TotalPages`** — Computed total number of pages (null if TotalCount is unknown)

### 🏗️ Creating `IAsyncPagedEnumerable<T>`

**From `IAsyncEnumerable<T>` with known count:**
```csharp
IAsyncEnumerable<User> users = GetUsersAsync();
var paged = users.ToAsyncPagedEnumerable(totalCount: 1000);
```

**From `IAsyncEnumerable<T>` with lazy total count:**
```csharp
var paged = users.ToAsyncPagedEnumerable(async cancellationToken =>
{
    int total = await _userRepository.CountAsync(cancellationToken);
    return Pagination.Create(pageSize: 20, currentPage: 1, totalCount: total);
});
```

**From `IAsyncEnumerable<T>` with pagination strategy:**
```csharp
var paged = users.ToAsyncPagedEnumerable(PaginationStrategy.PerPage);
```

**From `IQueryable<T>` (automatic pagination extraction):**
```csharp
var paged = _context.Products
    .Where(p => p.IsActive)
    .Skip(40)
    .Take(20)
    .ToAsyncPagedEnumerable();  // Automatically detects Skip/Take and total count
```

**From `IQueryable<T>` with explicit pagination factory:**
```csharp
var paged = _context.Products
    .OrderBy(p => p.Name)
    .ToAsyncPagedEnumerable(async ct => 
    {
        int total = await _context.Products.CountAsync(ct);
        return Pagination.Create(pageSize: 20, currentPage: 2, totalCount: total);
    });
```

---

## 💡 Design Philosophy

### ⚡️ **Async-First, Streaming-Focused**
- Designed for `await foreach` consumption without blocking threads
- Pagination metadata computation is lazy; only fetched when explicitly requested via `GetPaginationAsync()`
- Optimal for database cursors, API pagination, and cloud storage enumeration

### 💨 **Minimal Allocations**
- `Pagination` is a readonly struct; zero heap allocations for pagination state
- `AsyncPagedEnumerable<T>` is sealed to enable better JIT optimizations
- Lazy materialization; data is not loaded until enumerated

### 🚀 **AOT Compatible**
- No reflection; source-generated JSON serialization via `PaginationJsonContext`
- Safe for native AOT deployment and trimming
- Thread-safe lazy initialization of pagination state using atomic operations

### 📦 **Zero Dependencies**
- No Entity Framework, ASP.NET, or external package dependencies
- Pure `System.Collections.Generic` and `System.Text.Json` integration
- Works with any async enumerable source: EF Core, Dapper, custom queries, REST APIs, etc.

### 🔐 **Immutable & Thread-Safe**
- `Pagination` is a `readonly record struct`; safe to share across threads
- `AsyncPagedEnumerable<T>` uses thread-safe lazy initialization for pagination computation
- All navigation methods (`NextPage()`, `PreviousPage()`, `WithTotalCount()`) return new instances

---

## 🎯 Advanced Examples

### 🔄 Cursor-Based Pagination

For efficient pagination over time-series or large datasets without expensive total count queries:

```csharp
public IAsyncPagedEnumerable<Activity> GetActivitiesAsync(
    string? continuationToken,
    int pageSize = 50)
{
    var query = _context.Activities.AsQueryable();
    
    if (!string.IsNullOrEmpty(continuationToken))
    {
        var timestamp = DecodeToken(continuationToken);
        query = query.Where(a => a.Timestamp < timestamp);
    }
    
    var items = query
        .OrderByDescending(a => a.Timestamp)
        .Take(pageSize);
    
    var pagination = Pagination.Create(
        pageSize: pageSize,
        currentPage: 1,
        continuationToken: continuationToken,
        totalCount: null  // Unknown total count for cursor-based
    );
    
    return items.ToAsyncPagedEnumerable(async ct => 
    {
        var activities = await items.ToListAsync(ct);
        string? nextToken = activities.Count == pageSize
            ? EncodeToken(activities.Last().Timestamp)
            : null;
        
        return pagination with { ContinuationToken = nextToken };
    });
}

// Usage
var activities = GetActivitiesAsync(continuationToken: null);
await foreach (var activity in activities)
{
    Console.WriteLine(activity.Description);
}

var meta = await activities.GetPaginationAsync();
if (meta.HasContinuation)
{
    var nextPage = GetActivitiesAsync(meta.ContinuationToken);
}
```

### 📊 Streaming with Aggregation

```csharp
public async Task<PagedAggregation> GetOrderStatsAsync(
    int year,
    CancellationToken cancellationToken = default)
{
    var orders = _context.Orders
        .Where(o => o.CreatedAt.Year == year)
        .OrderByDescending(o => o.CreatedAt)
        .ToAsyncPagedEnumerable();
    
    decimal totalAmount = 0m;
    int itemCount = 0;
    
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
