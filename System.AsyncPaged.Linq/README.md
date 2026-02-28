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

```csharp
using System.Linq;

var result = await pagedProducts
    .WherePaged(p => p.Price > 10m)
    .SelectPaged(p => new ProductDto(p.Name, p.Price))
    .OrderByPaged(p => p.Name)
    .TakePaged(20)
    .ToListAsync(cancellationToken);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
