# ?? System.Linq.AsyncPaged

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/System.Linq.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **Comprehensive LINQ Operators for Async Pagination** — Full-featured, composable LINQ extensions for `IAsyncPagedEnumerable<T>` with projection, filtering, aggregation, and materialization support.

---

## ?? Overview

`System.Linq.AsyncPaged` extends `IAsyncPagedEnumerable<T>` with a complete suite of LINQ operators specifically designed for async pagination scenarios. It provides projection, filtering, aggregation, ordering, grouping, and materialization methods that preserve pagination metadata throughout the query chain.

Built for .NET 10 with AOT compatibility, this library seamlessly integrates with LINQ queries while maintaining the performance and memory efficiency benefits of async enumeration.

### ? Key Features

- **?? Projection Operators** — `SelectPaged`, `SelectPagedAsync` with sync and async selectors
- **?? Filtering Operations** — `WherePaged` with predicate-based filtering  
- **?? Aggregation Methods** — `CountPaged`, `SumPaged`, `MinPaged`, `MaxPaged`, `AggregatePagedAsync`, and more
- **??? Element Access** — `FirstPaged`, `LastPaged`, `SinglePaged`, `ElementAtPaged`
- **?? Ordering** — `OrderByPaged`, `ThenByPaged`, `ReversePaged` with ascending/descending support
- **?? Set Operations** — `DistinctPaged`, `UnionPaged`, `IntersectPaged`, `ExceptPaged`
- **?? Joining** — `JoinPaged`, `GroupJoinPaged` for cross-sequence operations
- **?? Grouping** — `GroupByPaged` for partitioning sequences while preserving pagination
- **?? Numerical** — `SumPaged`, `AveragePaged`, `MinPaged`, `MaxPaged` with custom projections
- **?? Materialization** — `ToListPagedAsync`, `ToArrayPagedAsync`, `MaterializeAsync` with memory efficiency
- **?? Windowing** — `TakePaged`, `SkipPaged`, `TakeWhilePaged`, `SkipWhilePaged` for partial enumeration
- **?? Flattening** — `SelectManyPaged`, `SelectManyPagedAsync` for nested enumerable composition
- **Pagination Preservation** — All operators preserve pagination metadata for accurate page tracking

---

## ?? Installation

```bash
dotnet add package System.Linq.AsyncPaged
```

Or via NuGet Package Manager:

```powershell
Install-Package System.Linq.AsyncPaged
```

### Prerequisites

- `System.Collections.AsyncPaged` (automatically installed as a dependency)

---

## ?? Quick Start

### ?? Basic LINQ Operations

```csharp
using System.Linq;

// Get paged products from database
IAsyncPagedEnumerable<Product> products = GetProductsAsync();

// Projection
var productNames = products.SelectPaged(p => p.Name);

// Filtering
var activeProducts = products.WherePaged(p => p.IsActive);

// Aggregation
int totalProducts = await products.CountPagedAsync();
decimal avgPrice = await products.AveragePaged(p => p.Price);

// Enumeration
await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

// Get pagination info
var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

### ?? Async Projection

```csharp
// Project with async operations (e.g., fetching related data)
var enrichedProducts = products.SelectPagedAsync(async product =>
{
    var reviews = await _reviewService.GetReviewsAsync(product.Id);
    return new EnrichedProduct
    {
        Product = product,
        ReviewCount = reviews.Count,
        AverageRating = reviews.Average(r => r.Rating)
    };
});

await foreach (var item in enrichedProducts)
{
    Console.WriteLine($"{item.Product.Name} - {item.AverageRating:F1} stars");
}
```

### ?? Composition Chain

```csharp
// Build complex queries while preserving pagination
var results = _context.Orders
    .ToAsyncPagedEnumerable()
    .WherePaged(o => o.CreatedAt.Year == DateTime.Now.Year)
    .SelectPaged(o => new OrderSummary
    {
        Id = o.Id,
        Total = o.Items.Sum(i => i.Price * i.Quantity),
        ItemCount = o.Items.Count
    })
    .OrderByPagedDescending(o => o.Total);

// Enumerate and access pagination
await foreach (var order in results)
{
    Console.WriteLine($"Order {order.Id}: ${order.Total} ({order.ItemCount} items)");
}

var pagination = await results.GetPaginationAsync();
Console.WriteLine($"Total orders: {pagination.TotalCount}");
```

---

## ?? Core Concepts

### ?? Extension Method Naming Convention

All LINQ extensions for `IAsyncPagedEnumerable<T>` follow a consistent naming pattern:
- **`XxxPaged`** — Async methods returning `IAsyncPagedEnumerable<T>` (e.g., `SelectPaged`, `WherePaged`)
- **`XxxPagedAsync`** — Async methods returning `ValueTask<T>` or `Task<T>` (e.g., `CountPagedAsync`, `FirstPagedAsync`)

This naming distinguishes paged operations from standard LINQ and makes intent clear.

### ?? Pagination Preservation

All operator implementations preserve the source's pagination metadata:

```csharp
var pagination = Pagination.Create(pageSize: 20, currentPage: 2, totalCount: 500);
var paged = source.ToAsyncPagedEnumerable(pagination);

// Projection preserves pagination
var projected = paged.SelectPaged(x => x.Name);
var projectedPagination = await projected.GetPaginationAsync();
// projectedPagination == pagination (same metadata)

// Filtering preserves pagination
var filtered = paged.WherePaged(x => x.IsActive);
var filteredPagination = await filtered.GetPaginationAsync();
// filteredPagination == pagination (same metadata, but note: item count may differ)
```

### ?? Aggregation Without Enumeration

Aggregation operations count or compute without loading all items:

```csharp
var paged = GetProductsAsync();

// These operations enumerate the sequence and compute the result
int count = await paged.CountPagedAsync();              // Total items enumerated
decimal sum = await paged.SumPagedAsync(p => p.Price); // Sum of prices
decimal avg = await paged.AveragePaged(p => p.Price);  // Average price
var min = await paged.MinPagedAsync();                  // Minimum value
var max = await paged.MaxPagedAsync();                  // Maximum value
```

---

## ?? Common Patterns

### ??? Product Search with Pagination

```csharp
public async Task<IAsyncPagedEnumerable<ProductDto>> SearchAsync(
    string? searchTerm,
    decimal? minPrice,
    decimal? maxPrice,
    int pageNumber = 1,
    int pageSize = 20)
{
    var products = _context.Products
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

### ?? Data Enrichment

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

### ?? Materialization for Multiple Enumerations

```csharp
// When you need to enumerate multiple times, materialize to avoid re-querying
var paged = _context.Products.Where(p => p.IsActive).ToAsyncPagedEnumerable();

// Materialize with all items in memory
var materialized = await paged.MaterializeAsync();

// Now you can enumerate multiple times efficiently
int count = await materialized.CountPagedAsync();
var maxPrice = await materialized.MaxPagedAsync(p => p.Price);
var minPrice = await materialized.MinPagedAsync(p => p.Price);

// Enumerate items
await foreach (var product in materialized)
{
    Console.WriteLine(product.Name);
}
```

### ?? Flattening Nested Collections

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

## ?? Advanced Examples

### ?? Aggregation Pipeline

```csharp
// Complex aggregation with grouping
var report = await _context.Sales
    .Where(s => s.Date.Year == 2024)
    .ToAsyncPagedEnumerable()
    .GroupByPagedAsync(s => s.Category)
    .SelectPagedAsync(async group =>
    {
        var total = await group.SumPagedAsync(s => s.Amount);
        var count = await group.CountPagedAsync();
        var avg = await group.AveragePagedAsync(s => s.Amount);

        return new CategoryReport
        {
            Category = group.Key,
            TotalSales = total,
            SalesCount = count,
            AverageSale = avg
        };
    })
    .ToListPagedAsync();

foreach (var category in report)
{
    Console.WriteLine($"{category.Category}: ${category.TotalSales} ({category.SalesCount} sales)");
}
```

### ?? Async Transformation with Cancellation

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

### ?? Windowing Operations

```csharp
// Skip first page, take next 20 items
var paged = products.SkipPagedWhile((item, index) => index < 20)
                    .TakePagedWhile((item, index) => index < 40);

await foreach (var product in paged)
{
    Console.WriteLine(product.Name);
}
```

### ?? Finding Specific Elements

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

## ? Best Practices

### ? Do

- **Chain operators fluently** — Take advantage of method chaining for readable queries
- **Use `*PagedAsync` methods for terminal operations** — They provide proper async/await semantics
- **Apply filtering early** — Filter before projection to reduce data processed
- **Materialize when needed** — Use `MaterializeAsync` for small datasets requiring multiple enumerations
- **Support cancellation** — Pass `CancellationToken` through async operations
- **Preserve pagination context** — Let operators maintain pagination metadata automatically

### ? Don't

- **Materialize large datasets unnecessarily** — Avoid loading entire result sets into memory
- **Combine filtering and projection inconsistently** — Use `WherePaged` + `SelectPaged` together
- **Forget pagination metadata** — Always call `GetPaginationAsync()` when displaying page info
- **Block on async operations** — Never use `.Result` or `.Wait()` on paged operations
- **Ignore cancellation tokens** — Support graceful cancellation in long-running queries

---

## ?? API Reference

### ?? Projection Extensions

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

### ?? Filtering Extensions

```csharp
// Synchronous predicate filtering
IAsyncPagedEnumerable<TSource> WherePaged(
    Func<TSource, bool> predicate);

// Conditional windowing
IAsyncPagedEnumerable<TSource> TakeWhilePaged(
    Func<TSource, bool> predicate);

IAsyncPagedEnumerable<TSource> SkipWhilePaged(
    Func<TSource, bool> predicate);
```

### ?? Aggregation Extensions

```csharp
// Counting
ValueTask<int> CountPagedAsync(CancellationToken cancellationToken = default);
ValueTask<int> CountPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default);
ValueTask<long> LongCountPagedAsync(CancellationToken cancellationToken = default);

// Existence checks
ValueTask<bool> AnyPagedAsync(CancellationToken cancellationToken = default);
ValueTask<bool> AnyPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default);
ValueTask<bool> AllPagedAsync(Func<TSource, bool> predicate, CancellationToken cancellationToken = default);
ValueTask<bool> ContainsPagedAsync(TSource value, CancellationToken cancellationToken = default);

// Min/Max operations
ValueTask<TSource> MinPagedAsync(CancellationToken cancellationToken = default);
ValueTask<TSource> MaxPagedAsync(CancellationToken cancellationToken = default);
ValueTask<TResult> MinPagedAsync<TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken = default);
ValueTask<TResult> MaxPagedAsync<TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken = default);

// Aggregation
ValueTask<TSource> AggregatePagedAsync(Func<TSource, TSource, TSource> func, CancellationToken cancellationToken = default);
ValueTask<TAccumulate> AggregatePagedAsync<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, CancellationToken cancellationToken = default);
```

### ?? Materialization Extensions

```csharp
// Fully materialize paged enumerable
ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync(
    CancellationToken cancellationToken = default);

ValueTask<IAsyncPagedEnumerable<T>> MaterializeAsync(
    int pageSize,
    int currentPage = 1,
    CancellationToken cancellationToken = default);

// Convert IAsyncEnumerable to materialized paged enumerable
ValueTask<IAsyncPagedEnumerable<T>> ToMaterializedAsyncPagedEnumerable<T>(
    this IAsyncEnumerable<T> source,
    CancellationToken cancellationToken = default);

// Precompute pagination metadata
ValueTask<IAsyncPagedEnumerable<T>> PrecomputePaginationAsync(
    CancellationToken cancellationToken = default);
```

---

## ?? Performance Considerations

- **Lazy Evaluation** — Operators are lazily evaluated; items are only processed when enumerated
- **Pagination Preservation** — Pagination metadata flows through the operator chain without recomputation
- **Memory Efficiency** — Stream data without materializing entire collections unless needed
- **Cancellation Support** — All async operations support cancellation tokens for resource cleanup
- **ValueTask Usage** — Terminal async operations use `ValueTask<T>` for allocation efficiency
- **Materialization Trade-off** — Use `MaterializeAsync()` only for small datasets requiring multiple passes

---

## ?? Dependency

This library depends on:
- **System.Collections.AsyncPaged** — Core pagination types and interfaces

---

## ?? Related Packages

- **[System.Collections.AsyncPaged](https://www.nuget.org/packages/System.Collections.AsyncPaged)** — Core async pagination library
- **[System.Text.Json.AsyncPaged](https://www.nuget.org/packages/System.Text.Json.AsyncPaged)** — JSON serialization support
- **[System.Net.Http.AsyncPaged](https://www.nuget.org/packages/System.Net.Http.AsyncPaged)** — REST API pagination

---

## ?? License & Contributing

Licensed under the **Apache License 2.0**. Copyright © Kamersoft 2025.

Contributions are welcome! Please visit [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net) to contribute, report issues, or request features.
