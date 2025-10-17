# ?? Xpandables.Net.Async

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Async Utilities & Pagination** - Powerful extensions for `IAsyncEnumerable<T>` with built-in pagination support for efficient data streaming.

---

## ?? Overview

Comprehensive async enumerable extensions providing LINQ-like operations, pagination, grouping, and transformations for asynchronous data streams.

### ?? Key Features

- ?? **Async LINQ** - Full async enumerable support
- ?? **Pagination** - Built-in paging with `IAsyncPagedEnumerable<T>`
- ? **Streaming** - Memory-efficient data processing
- ?? **Type-Safe** - Strongly-typed operations
- ?? **Rich Extensions** - Map, Filter, GroupBy, and more

---

## ?? Quick Start

```csharp
// Paginated queries
IAsyncPagedEnumerable<User> users = _repository
    .GetUsersAsync()
    .ToPaged(pageSize: 20);

await foreach (var user in users.WithCancellation(cancellationToken))
{
    Console.WriteLine(user.Name);
}

// Get pagination info
int totalCount = await users.CountAsync();
int pageCount = await users.PageCountAsync();
```

---

## ?? Advanced Operations

```csharp
// Transform and filter
var activeUsers = source
    .WhereAsync(u => u.IsActive)
    .SelectAsync(u => new UserDto(u.Id, u.Name))
    .OrderByAsync(u => u.Name);

// Grouping
var usersByCountry = await source
    .GroupByAsync(u => u.Country)
    .ToListAsync();

// Aggregation
decimal total = await orders
    .SumAsync(o => o.Total);
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
