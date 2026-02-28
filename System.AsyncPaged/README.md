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

```csharp
using System.Collections.Generic;

// Create from IAsyncEnumerable
IAsyncPagedEnumerable<Product> paged = AsyncPagedEnumerable.Create(
    productsAsyncEnumerable,
    ct => new ValueTask<Pagination>(new Pagination
    {
        TotalCount = 100,
        PageSize = 10,
        CurrentPage = 1
    }));

// Iterate with pagination
await foreach (var product in paged)
{
    Console.WriteLine(product.Name);
}

// Access pagination metadata
Pagination pagination = await paged.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
