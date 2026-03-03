# System.AsyncPaged

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Asynchronous paged enumerable abstraction for streaming large datasets with pagination metadata.

## 📖 Overview

`System.AsyncPaged` (NuGet: **Xpandables.AsyncPaged**) provides `IAsyncPagedEnumerable<T>` — an `IAsyncEnumerable<T>` enriched with `Pagination` metadata (total count, page size, current page, continuation token). It supports cursor-based and offset-based pagination strategies. Namespace: `System.Collections.Generic`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `IAsyncPagedEnumerable<T>` | `IAsyncPagedEnumerable.cs` | `IAsyncEnumerable<T>` + `Pagination` property + `GetPaginationAsync` |
| `IAsyncPagedEnumerator<T>` | `IAsyncPagedEnumerator.cs` | Enumerator with pagination awareness |
| `AsyncPagedEnumerable` | `AsyncPagedEnumerable.cs` | Default implementation |
| `AsyncPagedEnumerableFactory` | `AsyncPagedEnumerableFactory.cs` | Static factory methods (`Create`) |
| `AsyncPagedEnumerator` | `AsyncPagedEnumerator.cs` | Default enumerator implementation |
| `AsyncPagedEnumeratorFactory` | `AsyncPagedEnumeratorFactory.cs` | Enumerator factory |
| `Pagination` | `Pagination.cs` | Record struct — `TotalCount`, `PageSize`, `CurrentPage`, `TotalPages`, `ContinuationToken` |
| `PaginationStrategy` | `Pagination.cs` | Enum — `None`, `PerPage`, `PerItem` |
| `CursorOptions<TSource>` | `CursorOptions.cs` | Cursor-based pagination configuration — `KeySelector`, `CursorType`, `CursorDirection` |
| `CursorDirection` | `CursorOptions.cs` | Enum — `Forward`, `Backward` |
| `QueryPaginationNormalizer` | `QueryPaginationNormalizer.cs` | Normalizes pagination parameters |
| `IAsyncEnumerableExtensions` | `IAsyncEnumerableExtensions.cs` | Extension methods on `IAsyncEnumerable<T>` |
| `IAsyncPagedEnumerableExtensions` | `IAsyncPagedEnumerableExtensions.cs` | Extension methods on `IAsyncPagedEnumerable<T>` |

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged
```

**Project References:** `Xpandables.Primitives`

## 🚀 Quick Start

### Create from an IAsyncEnumerable with Explicit Pagination

```csharp
using System.Collections.Generic;

// Simulate a database query that returns products as an async stream
async IAsyncEnumerable<Product> FetchProductsAsync(int page, int pageSize)
{
    // ... yield items from database
}

// Wrap with pagination metadata
IAsyncPagedEnumerable<Product> paged = AsyncPagedEnumerable.Create(
    FetchProductsAsync(page: 2, pageSize: 25),
    ct => new ValueTask<Pagination>(Pagination.Create(
        pageSize: 25,
        currentPage: 2,
        totalCount: 250)));

// Iterate items
await foreach (Product product in paged)
{
    Console.WriteLine(product.Name);
}

// Access pagination metadata (lazily computed)
Pagination pagination = await paged.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
// Output: Page 2 of 10

Console.WriteLine($"Has next: {pagination.HasNextPage}");
// Output: Has next: True
```

### Create from an IQueryable (EF Core / LINQ)

```csharp
// IQueryable-based — pagination is auto-extracted from Skip/Take
IQueryable<Order> query = dbContext.Orders
    .Where(o => o.Status == "Active")
    .OrderBy(o => o.CreatedOn)
    .Skip(20)
    .Take(10);

IAsyncPagedEnumerable<Order> pagedOrders = query.ToAsyncPagedEnumerable();

await foreach (Order order in pagedOrders)
{
    Console.WriteLine($"{order.Id}: {order.TotalAmount}");
}

Pagination p = await pagedOrders.GetPaginationAsync();
Console.WriteLine($"Total: {p.TotalCount}, Skip: {p.Skip}, Take: {p.Take}");
```

### Pagination Strategies

```csharp
// PerPage — pagination metadata updates at page boundaries
IAsyncPagedEnumerable<Product> perPage = AsyncPagedEnumerable
    .Create(source, _ => ValueTask.FromResult(
        Pagination.Create(pageSize: 50, currentPage: 1, totalCount: 500)))
    .WithPerPageStrategy();

// PerItem — pagination metadata updates after each item
IAsyncPagedEnumerable<Product> perItem = AsyncPagedEnumerable
    .Create(source, _ => ValueTask.FromResult(
        Pagination.Create(pageSize: 50, currentPage: 1, totalCount: 500)))
    .WithPerItemStrategy();
```

### Convert IAsyncEnumerable to IAsyncPagedEnumerable

```csharp
// From total count only
IAsyncPagedEnumerable<User> users = usersStream.ToAsyncPagedEnumerable(totalCount: 1000);

// From explicit Pagination struct
Pagination meta = Pagination.Create(pageSize: 10, currentPage: 3, totalCount: 100);
IAsyncPagedEnumerable<User> usersWithMeta = usersStream.ToAsyncPagedEnumerable(meta);

// From async factory
IAsyncPagedEnumerable<User> usersFromFactory = usersStream.ToAsyncPagedEnumerable(
    async ct => Pagination.Create(
        pageSize: 10,
        currentPage: 1,
        totalCount: await dbContext.Users.CountAsync(ct)));
```

### Pagination Navigation

```csharp
Pagination page = Pagination.Create(pageSize: 20, currentPage: 1, totalCount: 200);

page.IsFirstPage;      // true
page.HasNextPage;       // true
page.HasPreviousPage;   // false
page.TotalPages;        // 10
page.Skip;              // 0
page.Take;              // 20

// Advance to next page
Pagination next = page.NextPage();
// next.CurrentPage = 2, next.Skip = 20

// Update total count
Pagination updated = page.WithTotalCount(300);

// Use continuation tokens (cursor-based)
Pagination cursor = Pagination.Create(
    pageSize: 50,
    currentPage: 1,
    continuationToken: "eyJpZCI6IDQyfQ==");
```

### Empty Paged Enumerable

```csharp
// Return an empty result with proper pagination
IAsyncPagedEnumerable<Product> empty = AsyncPagedEnumerable.Empty<Product>(
    Pagination.Create(pageSize: 10, currentPage: 1, totalCount: 0));
```

---

## 📁 Project Structure

```
System.AsyncPaged/
├── IAsyncPagedEnumerable.cs           # Core interface
├── IAsyncPagedEnumerator.cs           # Enumerator interface
├── AsyncPagedEnumerable.cs            # Default implementation
├── AsyncPagedEnumerableFactory.cs     # Static factory (Create, Empty)
├── AsyncPagedEnumerator.cs            # Default enumerator
├── AsyncPagedEnumeratorFactory.cs     # Enumerator creation
├── Pagination.cs                      # Pagination record struct + PaginationStrategy
├── CursorOptions.cs                   # Cursor-based pagination config
├── QueryPaginationNormalizer.cs       # Skip/Take extraction from IQueryable
├── IAsyncEnumerableExtensions.cs      # ToAsyncPagedEnumerable extensions
├── IAsyncPagedEnumerableExtensions.cs # ArgumentType, GetArgumentType
└── IAsyncPagedEnumerableStrategyExtensions.cs  # WithPerPageStrategy, WithPerItemStrategy
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
