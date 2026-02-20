# System.AsyncPaged.Linq

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.Linq.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Linq)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.Linq.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Linq)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Comprehensive LINQ operators for `IAsyncPagedEnumerable<T>` with projection, filtering, aggregation, and materialization support.

## Overview

`System.AsyncPaged.Linq` extends `IAsyncPagedEnumerable<T>` with a complete suite of LINQ operators specifically designed for async pagination scenarios. All operators preserve pagination metadata throughout the query chain.

Built for .NET 10 with AOT compatibility.

## Features

### Projection (ProjectionExtensions)
- `SelectPaged` — Synchronous projection with optional index
- `SelectPagedAsync` — Async projection
- `SelectManyPaged` — Flatten nested enumerables
- `SelectManyPagedAsync` — Async flatten

### Set Operations (SetExtensions)
- `DistinctPaged` — Remove duplicates
- `DistinctByPaged` — Remove duplicates by key
- `UnionPaged` — Combine sequences
- `IntersectPaged` — Common elements
- `ExceptPaged` — Exclude elements

### Ordering (OrderingExtensions)
- `OrderByPaged` / `OrderByPagedDescending` — Primary ordering
- `ThenByPaged` / `ThenByPagedDescending` — Secondary ordering
- `ReversePaged` — Reverse sequence order

### Grouping (GroupingExtensions)
- `GroupByPaged` — Partition by key with optional element and result selectors

### Joining (JoinExtensions)
- `JoinPaged` — Inner join sequences
- `GroupJoinPaged` — Group join for one-to-many relationships

### Windowing & Partitioning (WindowingExtensions)
- `TakePaged` / `TakeLastPaged` — Take elements from start/end
- `SkipPaged` / `SkipLastPaged` — Skip elements from start/end
- `TakeWhilePaged` / `SkipWhilePaged` — Conditional take/skip
- `ChunkPaged` — Split into fixed-size chunks
- `WindowPaged` — Sliding window of elements
- `WindowedSumPaged` / `WindowedAveragePaged` — Windowed aggregates
- `WindowedMinPaged` / `WindowedMaxPaged` — Windowed min/max
- `PairwisePaged` — Process consecutive pairs
- `ScanPaged` — Running accumulator

### Transformation (TransformationExtensions)
- `WherePaged` — Filter with predicate
- `CastPaged` — Type cast elements
- `OfTypePaged` — Filter by type

### Materialization (MaterializationExtensions)
- `ToListPagedAsync` — Materialize to list
- `ToArrayPagedAsync` — Materialize to array
- `MaterializeAsync` — Materialize with custom collection
- `PrecomputePaginationAsync` — Compute pagination upfront

### Pagination (PaginationExtensions)
- `ToAsyncPagedEnumerable` — Convert `IQueryable<T>` or `IAsyncEnumerable<T>`

## Installation

```bash
dotnet add package Xpandables.AsyncPaged.Linq
```

**Dependency:** `Xpandables.AsyncPaged` (installed automatically)

## Quick Start

### Basic LINQ Operations

```csharp
using System.Linq;

IAsyncPagedEnumerable<Product> products = GetProductsAsync();

// Projection
var productNames = products.SelectPaged(p => p.Name);

// Filtering
var activeProducts = products.WherePaged(p => p.IsActive);

// Ordering
var sorted = products.OrderByPaged(p => p.Name);

// Enumerate
await foreach (var product in activeProducts)
{
    Console.WriteLine(product.Name);
}

// Pagination metadata
var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

### Async Projection

```csharp
var enrichedProducts = products.SelectPagedAsync(async product =>
{
    var reviews = await _reviewService.GetReviewsAsync(product.Id);
    return new { product.Name, ReviewCount = reviews.Count };
});
```

### Windowed Analysis

```csharp
// 7-day moving average
var movingAverages = products
    .OrderByPaged(p => p.CreatedAt)
    .WindowedAveragePaged(windowSize: 7, p => p.Price);

// Pairwise comparison
var priceChanges = products
    .PairwisePaged((prev, curr) => curr.Price - prev.Price);
```

### Materialization

```csharp
// Materialize to list with pagination preserved
var list = await products.ToListPagedAsync();

// Precompute pagination for multiple enumerations
var precomputed = await products.PrecomputePaginationAsync();
```

## License

Apache License 2.0
        .Where(p => p.IsActive)
        .ToAsyncPagedEnumerable();

    // Apply filters
    if (!string.IsNullOrWhiteSpace(searchTerm))
        products = products.WherePaged(p => p.Name.Contains(searchTerm));

    if (minPrice.HasValue)
        products = products.WherePaged(p => p.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        products = products.WherePaged(p => p.Price <= maxPrice.Value);

    // Project to DTO
    var dtos = products.SelectPaged(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Category = p.Category.Name
    });

    // Sort and return
    return dtos.OrderByPagedDescending(p => p.Price);
}

// Usage
var results = await SearchAsync("laptop", minPrice: 500, maxPrice: 2000, pageSize: 10);
await foreach (var product in results)
{
    Console.WriteLine($"{product.Name} - ${product.Price}");
}

var pagination = await results.GetPaginationAsync();
```

### 🎯 Data Enrichment

```csharp
// Enrich items from paged source with additional async data
var enrichedUsers = _context.Users
    .Where(u => u.IsActive)
    .ToAsyncPagedEnumerable()
    .SelectPagedAsync(async user =>
    {
        var orderCount = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .CountAsync();

        var totalSpent = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .SumAsync(o => o.Total);

        return new UserSummary
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            OrderCount = orderCount,
            TotalSpent = totalSpent
        };
    });

await foreach (var summary in enrichedUsers)
{
    Console.WriteLine($"{summary.Name}: {summary.OrderCount} orders, ${summary.TotalSpent}");
}
```

### 💾 Materialization for Multiple Enumerations

```csharp
// When you need to enumerate multiple times, materialize to avoid re-querying
var paged = _context.Products.Where(p => p.IsActive).ToAsyncPagedEnumerable();

// Materialize with all items in memory
var materialized = await paged.MaterializeAsync();

// Now you can enumerate multiple times efficiently
int count = await materialized.CountAsync();
var maxPrice = await materialized.MaxAsync(p => p.Price);
var minPrice = await materialized.MinAsync(p => p.Price);

// Enumerate items
await foreach (var product in materialized)
{
    Console.WriteLine(product.Name);
}
```

### 📦 Flattening Nested Collections

```csharp
// Flatten orders and their items while preserving pagination
var orderItems = _context.Orders
    .ToAsyncPagedEnumerable()
    .SelectManyPagedAsync(order => 
        _context.OrderItems
            .Where(oi => oi.OrderId == order.Id)
            .ToAsyncEnumerable(),
        (order, item) => new
        {
            OrderId = order.Id,
            ItemId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price
        });

await foreach (var item in orderItems)
{
    Console.WriteLine($"Order {item.OrderId}: {item.Quantity}x at ${item.Price}");
}
```

---

## 🎯 Advanced Examples

### 📈 Aggregation Pipeline

```csharp
// Complex aggregation with grouping
var salesByCategory = _context.Sales
    .Where(s => s.Date.Year == 2024)
    .ToAsyncPagedEnumerable()
    .GroupBy(s => s.Category);

var report = new List<CategoryReport>();

foreach (var group in salesByCategory)
{
    var pagedGroup = group.ToAsyncPagedEnumerable();
    var total = await pagedGroup.SumAsync(s => s.Amount);
    var count = await pagedGroup.CountAsync();
    var avg = await pagedGroup.AverageAsync(s => s.Amount);

    report.Add(new CategoryReport
    {
        Category = group.Key,
        TotalSales = total,
        SalesCount = count,
        AverageSale = avg
    });
}

foreach (var category in report)
{
    Console.WriteLine($"{category.Category}: ${category.TotalSales} ({category.SalesCount} sales)");
}
```

### 🔄 Async Transformation with Cancellation

```csharp
// Support cancellation in async projections
var processed = products.SelectPagedAsync(
    async (product, cancellationToken) =>
    {
        // Long-running operation with cancellation support
        var enhanced = await EnhanceProductAsync(product, cancellationToken);
        return enhanced;
    });

// Enumerate with cancellation
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await foreach (var item in processed.WithCancellation(cts.Token))
{
    Console.WriteLine(item.Name);
}
```

### 🪟 Windowing & Chunking Operations

```csharp
// Skip first 20, take next 20 items
var paged = products
    .SkipPaged(20)
    .TakePaged(20);

await foreach (var product in paged)
{
    Console.WriteLine(product.Name);
}

// Chunk into batches
var batches = products.ChunkPaged(size: 50);
await foreach (var batch in batches)
{
    Console.WriteLine($"Processing batch of {batch.Length} items");
    // Process batch
}

// Sliding window analysis
var windows = timeSeries
    .WindowPaged(windowSize: 7);

await foreach (var window in windows)
{
    var avg = window.Average();
    Console.WriteLine($"7-day window average: {avg}");
}
```

### 🎯 Finding Specific Elements

```csharp
// Get first item matching criteria
var firstExpensive = await products
    .WherePaged(p => p.Price > 1000)
    .FirstPagedAsync();

// Get single item or throw if not found
var singleProduct = await products
    .WherePaged(p => p.Id == productId)
    .SinglePagedAsync();

// Get element at specific index
var fifthProduct = await products.ElementAtPagedAsync(4);
```

---

## ✅ Best Practices

### ✅ Do

- **Chain operators fluently** — Take advantage of method chaining for readable queries
- **Use `*PagedAsync` methods for terminal operations** — They provide proper async/await semantics
- **Apply filtering early** — Filter with `WherePaged` before projection to reduce data processed
- **Materialize when needed** — Use `MaterializeAsync()` for small datasets requiring multiple enumerations
- **Precompute pagination when appropriate** — Use `PrecomputePaginationAsync()` to eagerly compute pagination metadata once
- **Support cancellation** — Pass `CancellationToken` through async operations
- **Preserve pagination context** — Let operators maintain pagination metadata automatically
- **Use analytical operations efficiently** — Leverage `WindowPaged`, `PairwisePaged`, `ScanPaged` for time-series and trend analysis
- **Use MinBy/MaxBy** — Find items with extreme key values without manual comparison

### ❌ Don't

- **Materialize large datasets unnecessarily** — Avoid loading entire result sets into memory with `MaterializeAsync()`
- **Mix filtering patterns inconsistently** — Use `WherePaged` + `SelectPaged` for clear separation of concerns
- **Forget pagination metadata** — Always call `GetPaginationAsync()` when displaying page info
- **Block on async operations** — Never use `.Result` or `.Wait()` on paged operations
- **Ignore cancellation tokens** — Support graceful cancellation in long-running queries
- **Compute pagination multiple times** — If accessing pagination repeatedly, use `PrecomputePaginationAsync()` first

---

## 📖 API Reference

### 🔄 Projection Extensions

```csharp
// Synchronous projection
IAsyncPagedEnumerable<TResult> SelectPaged<TResult>(
    Func<TSource, TResult> selector);

// Asynchronous projection (without cancellation)
IAsyncPagedEnumerable<TResult> SelectPagedAsync<TResult>(
    Func<TSource, ValueTask<TResult>> selectorAsync);

// Asynchronous projection (with cancellation)
IAsyncPagedEnumerable<TResult> SelectPagedAsync<TResult>(
    Func<TSource, CancellationToken, ValueTask<TResult>> selectorAsync);
```

### 🔍 Filtering & Partitioning Extensions

```csharp
// Synchronous predicate filtering
IAsyncPagedEnumerable<TSource> WherePaged(Func<TSource, bool> predicate);
IAsyncPagedEnumerable<TSource> WherePaged(Func<TSource, int, bool> predicate);

// Partitioning
IAsyncPagedEnumerable<TSource> TakePaged(int count);
IAsyncPagedEnumerable<TSource> SkipPaged(int count);
IAsyncPagedEnumerable<TSource> TakeLastPaged(int count);
IAsyncPagedEnumerable<TSource> SkipLastPaged(int count);

// Conditional partitioning
IAsyncPagedEnumerable<TSource> TakeWhilePaged(Func<TSource, bool> predicate);
IAsyncPagedEnumerable<TSource> TakeWhilePaged(Func<TSource, int, bool> predicate);
IAsyncPagedEnumerable<TSource> SkipWhilePaged(Func<TSource, bool> predicate);
IAsyncPagedEnumerable<TSource> SkipWhilePaged(Func<TSource, int, bool> predicate);

// Chunking & Distinctness
IAsyncPagedEnumerable<TSource[]> ChunkPaged(int size);
IAsyncPagedEnumerable<TSource> DistinctPaged();
IAsyncPagedEnumerable<TSource> DistinctPaged(IEqualityComparer<TSource>? comparer);
IAsyncPagedEnumerable<TSource> DistinctByPaged<TKey>(Func<TSource, TKey> keySelector);
IAsyncPagedEnumerable<TSource> DistinctByPaged<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer);
```

### 💾 Materialization Extensions

```csharp
// Fully materialize paged enumerable
ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync(
    this IAsyncPagedEnumerable<T> source,
    CancellationToken cancellationToken = default);

ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync(
    this IAsyncPagedEnumerable<T> source,
    int pageSize,
    int currentPage = 1,
    CancellationToken cancellationToken = default);

// Convert IAsyncEnumerable to materialized paged enumerable
ValueTask<IAsyncPagedEnumerable<T>> ToMaterializedAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    CancellationToken cancellationToken = default);

ValueTask<IAsyncPagedEnumerable<T>> ToMaterializedAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    int pageSize,
    int currentPage = 1,
    CancellationToken cancellationToken = default);

// Precompute pagination metadata (extension method on IAsyncPagedEnumerable<T>)
ValueTask<IAsyncPagedEnumerable<T>> PrecomputePaginationAsync(
    CancellationToken cancellationToken = default);
```

### 🔬 Analytical & Windowing Extensions

```csharp
// Sliding windows
IAsyncPagedEnumerable<TSource[]> WindowPaged(int windowSize);

// Windowed aggregations
IAsyncPagedEnumerable<int> WindowedSumPaged(int windowSize, Func<TSource, int> selector);
IAsyncPagedEnumerable<long> WindowedSumPaged(int windowSize, Func<TSource, long> selector);
IAsyncPagedEnumerable<double> WindowedSumPaged(int windowSize, Func<TSource, double> selector);
IAsyncPagedEnumerable<double> WindowedAveragePaged(int windowSize, Func<TSource, double> selector);
IAsyncPagedEnumerable<TValue> WindowedMinPaged<TValue>(int windowSize, Func<TSource, TValue> selector) where TValue : IComparable<TValue>;
IAsyncPagedEnumerable<TValue> WindowedMaxPaged<TValue>(int windowSize, Func<TSource, TValue> selector) where TValue : IComparable<TValue>;

// Pairwise operations
IAsyncPagedEnumerable<(TSource Previous, TSource Current)> PairwisePaged();
IAsyncPagedEnumerable<TResult> PairwisePaged<TResult>(Func<TSource, TSource, TResult> selector);

// Scan (running aggregation)
IAsyncPagedEnumerable<TAccumulate> ScanPaged<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func);
IAsyncPagedEnumerable<TSource> ScanPaged(Func<TSource, TSource, TSource> func);
```

---

## ⚙️ Performance Considerations

- **Lazy Evaluation** — Operators are lazily evaluated; items are only processed when enumerated
- **Pagination Preservation** — Pagination metadata flows through the operator chain without recomputation
- **Memory Efficiency** — Stream data without materializing entire collections unless needed
- **Cancellation Support** — All async operations support cancellation tokens for resource cleanup
- **ValueTask Usage** — Terminal async operations use `ValueTask<T>` for allocation efficiency
- **Materialization Trade-off** — Use `MaterializeAsync()` only for small datasets requiring multiple passes
- **Precomputation Strategy** — Use `PrecomputePaginationAsync()` when pagination metadata is accessed frequently without enumeration
- **Windowing Operations** — `WindowPaged` and related methods use `Queue<T>` for efficient sliding windows (O(1) enqueue/dequeue)
- **Distinct Operations** — `DistinctPaged` and `DistinctByPaged` use `HashSet<T>` for O(1) lookups
- **Analytical Operations** — `ScanPaged` produces running results without storing intermediate states; `PairwisePaged` has minimal memory footprint

---

## 🔗 Dependency

This library depends on:
- **System.AsyncPaged** — Core pagination types and interfaces

---

## 📚 Related Packages

- **[System.AsyncPaged](https://www.nuget.org/packages/System.Collections.AsyncPaged)** — Core async pagination library
- **[System.AsyncPaged.Json](https://www.nuget.org/packages/System.Text.Json.AsyncPaged)** — JSON serialization support
- **[System.Net.Http.AsyncPaged](https://www.nuget.org/packages/System.Net.Http.AsyncPaged)** — REST API pagination

---

## 📄 License & Contributing

Licensed under the **Apache License 2.0**. Copyright © Kamersoft 2025.

Contributions are welcome! Please visit [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net) to contribute, report issues, or request features.
