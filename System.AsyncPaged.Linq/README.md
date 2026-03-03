# System.AsyncPaged.Linq

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.Linq.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Linq)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.Linq.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Linq)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

LINQ operators for `IAsyncPagedEnumerable<T>` — filter, project, order, group, join, and materialize paged async sequences.

## 📖 Overview

`System.AsyncPaged.Linq` (NuGet: **Xpandables.AsyncPaged.Linq**) provides C# 14 extension members that bring familiar LINQ operators to `IAsyncPagedEnumerable<T>`, preserving pagination metadata through transformations. Namespace: `System.Linq`.

Built for **.NET 10** and **C# 14**.

## ✨ Extension Categories

| File | Operators |
|------|-----------|
| `TransformationExtensions.cs` | `SelectManyPaged`, `SelectPaged`, `WherePaged`, `DistinctPaged` |
| `ProjectionExtensions.cs` | `SelectPaged<TResult>` — element projection |
| `OrderingExtensions.cs` | `OrderByPaged`, `OrderByDescendingPaged`, `ThenByPaged` |
| `GroupingExtensions.cs` | `GroupByPaged` |
| `JoinExtensions.cs` | `JoinPaged`, `GroupJoinPaged` |
| `SetExtensions.cs` | `UnionPaged`, `IntersectPaged`, `ExceptPaged`, `ConcatPaged` |
| `PaginationExtensions.cs` | `SkipPaged`, `TakePaged` |
| `WindowingExtensions.cs` | `ChunkPaged`, `BufferPaged` |
| `MaterializationExtensions.cs` | `ToListAsync`, `ToArrayAsync`, `ToDictionaryAsync`, `CountAsync`, `AnyAsync`, `FirstOrDefaultAsync` |

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged.Linq
```

**Project References:** `Xpandables.AsyncPaged`

## 🚀 Quick Start

### Projection — Transform Elements

```csharp
using System.Linq;

// Project entities to DTOs, preserving pagination metadata
IAsyncPagedEnumerable<ProductDto> productDtos = pagedProducts
    .SelectPaged(p => new ProductDto(p.Id, p.Name, p.Price));

// Project with index
IAsyncPagedEnumerable<RankedProduct> ranked = pagedProducts
    .SelectPaged((p, index) => new RankedProduct(index + 1, p.Name));

// Async projection (e.g., call an external service per item)
IAsyncPagedEnumerable<EnrichedProduct> enriched = pagedProducts
    .SelectPagedAsync(async p =>
    {
        decimal discount = await discountService.GetDiscountAsync(p.Id);
        return new EnrichedProduct(p.Name, p.Price, discount);
    });
```

### Flattening — SelectMany

```csharp
// Flatten order → order items
IAsyncPagedEnumerable<OrderItem> allItems = pagedOrders
    .SelectManyPaged(order => order.Items);

// Flatten with result selector
IAsyncPagedEnumerable<OrderItemDto> itemDtos = pagedOrders
    .SelectManyPaged(
        order => order.Items,
        (order, item) => new OrderItemDto(order.Id, item.ProductName, item.Qty));
```

### Ordering

```csharp
IAsyncPagedEnumerable<Product> sorted = pagedProducts
    .OrderByPaged(p => p.Category)
    .ThenByPaged(p => p.Name);

IAsyncPagedEnumerable<Product> sortedDesc = pagedProducts
    .OrderByDescendingPaged(p => p.Price);
```

### Grouping

```csharp
IAsyncPagedEnumerable<IGrouping<string, Product>> grouped = pagedProducts
    .GroupByPaged(p => p.Category);

await foreach (var group in grouped)
{
    Console.WriteLine($"Category: {group.Key}");
    foreach (Product product in group)
        Console.WriteLine($"  - {product.Name}");
}
```

### Set Operations

```csharp
IAsyncPagedEnumerable<Product> featured = GetFeaturedProducts();
IAsyncPagedEnumerable<Product> onSale = GetOnSaleProducts();

// Union — combine unique products
IAsyncPagedEnumerable<Product> combined = featured
    .UnionPaged(onSale);

// Intersect — products in both sets
IAsyncPagedEnumerable<Product> both = featured
    .IntersectPaged(onSale);

// Except — featured but NOT on sale
IAsyncPagedEnumerable<Product> exclusive = featured
    .ExceptPaged(onSale);

// Concat — combine with duplicates
IAsyncPagedEnumerable<Product> all = featured
    .ConcatPaged(onSale);
```

### Joins

```csharp
IAsyncPagedEnumerable<OrderSummary> joined = pagedOrders
    .JoinPaged(
        customers,                              // inner sequence
        order => order.CustomerId,              // outer key
        customer => customer.Id,                // inner key
        (order, customer) => new OrderSummary(  // result selector
            order.Id, customer.Name, order.Total));
```

### Pagination (Skip / Take)

```csharp
IAsyncPagedEnumerable<Product> page3 = pagedProducts
    .SkipPaged(20)
    .TakePaged(10);
```

### Windowing (Chunk / Buffer)

```csharp
// Process in batches of 100
IAsyncPagedEnumerable<Product[]> batches = pagedProducts
    .ChunkPaged(100);

await foreach (Product[] batch in batches)
{
    await bulkInsertService.InsertAsync(batch);
}
```

### Materialization — Collect Results

```csharp
// Collect into a List
List<Product> productList = await pagedProducts.ToListAsync(ct);

// Collect into an array
Product[] productArray = await pagedProducts.ToArrayAsync(ct);

// Collect into a dictionary
Dictionary<Guid, Product> productMap = await pagedProducts
    .ToDictionaryAsync(p => p.Id, ct);

// Count, Any, First
int count = await pagedProducts.CountAsync(ct);
bool hasAny = await pagedProducts.AnyAsync(ct);
Product? first = await pagedProducts.FirstOrDefaultAsync(ct);

// Pre-compute pagination to avoid double enumeration
IAsyncPagedEnumerable<Product> precomputed = await pagedProducts
    .PrecomputePaginationAsync(ct);

// Materialize everything into memory for multiple passes
IAsyncPagedEnumerable<Product> materialized = await pagedProducts
    .MaterializeAsync(ct);
```

### Full Pipeline Example

```csharp
// Build a complete query pipeline for an API endpoint
List<OrderDto> result = await dbContext.Orders
    .Where(o => o.Status == "Active")
    .OrderByDescending(o => o.CreatedOn)
    .Skip(0).Take(50)
    .ToAsyncPagedEnumerable()
    .SelectPaged(o => new OrderDto(o.Id, o.CustomerName, o.Total, o.CreatedOn))
    .ToListAsync(cancellationToken);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
