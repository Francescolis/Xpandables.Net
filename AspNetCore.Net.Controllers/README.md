# ?? Xpandables.Net.AspNetCore.Controllers

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **MVC Controller Features** - Base controllers, action filters, model binding, and ExecutionResult integration for ASP.NET Core MVC/API controllers.

---

## ?? Overview

`Xpandables.Net.AsyncPaged` provides a comprehensive solution for implementing pagination in asynchronous scenarios. It extends `IAsyncEnumerable<T>` with pagination metadata, enabling efficient data streaming with full pagination support.

### ?? Key Features

- ?? **IAsyncPagedEnumerable** - Async enumerable with built-in pagination metadata
- ?? **Pagination Metadata** - Total count, page size, current page, and continuation tokens
- ?? **LINQ Integration** - Seamless integration with IQueryable and IAsyncEnumerable
- ?? **Memory Efficient** - Stream large datasets without loading all data into memory
- ? **High Performance** - Optimized for minimal allocations and maximum throughput

---

## ?? Quick Start

### Installation

```bash
dotnet add package Xpandables.Net.AsyncPaged
```

### Basic Usage

```csharp
using Microsoft.EntityFrameworkCore;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

// Simple pagination
IAsyncEnumerable<Product> products = GetProductsAsync();
IAsyncPagedEnumerable<Product> pagedProducts = products
    .ToAsyncPagedEnumerable(totalCount: 1000);

await foreach (var product in pagedProducts)
{
    Console.WriteLine($"{product.Name} - ${product.Price}");
}

// Access pagination metadata
var pagination = await pagedProducts.GetPaginationAsync();
Console.WriteLine($"Total items: {pagination.TotalCount}");
```

### Database Pagination

```csharp
public IAsyncPagedEnumerable<Product> GetProductsAsync(
    int pageSize = 20, 
    int pageNumber = 1)
{
    return _context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToAsyncPagedEnumerable(); // Automatically calculates total count
}

// Usage
var products = productService.GetProductsAsync(pageSize: 10, pageNumber: 1);

await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Page {pagination.CurrentPage} of {pagination.TotalPages}");
```

---

## ?? Real-World Examples

### E-Commerce Product Search

```csharp
public async Task<IAsyncPagedEnumerable<ProductDto>> SearchProductsAsync(
    string? searchTerm,
    decimal? minPrice,
    decimal? maxPrice,
    int pageSize = 24,
    int pageNumber = 1)
{
    var query = _context.Products.Where(p => p.IsActive);
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
        query = query.Where(p => p.Name.Contains(searchTerm));
    
    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice);
    
    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice);
    
    return query
        .OrderBy(p => p.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Category = p.Category.Name
        })
        .ToAsyncPagedEnumerable();
}
```

### Cursor-Based Pagination

```csharp
public async Task<IAsyncPagedEnumerable<Activity>> GetActivitiesAsync(
    string? continuationToken,
    int pageSize = 50)
{
    var query = _context.Activities.AsQueryable();
    
    if (!string.IsNullOrEmpty(continuationToken))
    {
        var cursor = DecodeCursor(continuationToken);
        query = query.Where(a => a.Timestamp < cursor.Timestamp);
    }
    
    var items = query
        .OrderByDescending(a => a.Timestamp)
        .Take(pageSize);
    
    int totalCount = await _context.Activities.CountAsync();
    
    return items.ToAsyncPagedEnumerable(
        Pagination.Create(
            pageSize: pageSize,
            currentPage: 1,
            continuationToken: GenerateNextToken(),
            totalCount: totalCount));
}
```

---

## ?? Core Concepts

### Pagination Metadata

```csharp
var pagination = Pagination.Create(
    pageSize: 20,
    currentPage: 3,
    totalCount: 250);

Console.WriteLine($"Skip: {pagination.Skip}");           // 40
Console.WriteLine($"Take: {pagination.Take}");           // 20
Console.WriteLine($"Total Pages: {pagination.TotalPages}"); // 13
Console.WriteLine($"Has Next: {pagination.HasNextPage}");   // true

// Navigate
var nextPage = pagination.NextPage();
var previousPage = pagination.PreviousPage();
```

### Creating Paged Enumerables

```csharp
// From IAsyncEnumerable
var paged1 = products.ToAsyncPagedEnumerable(totalCount: 100);

// With pagination object
var pagination = Pagination.Create(pageSize: 20, currentPage: 1, totalCount: 100);
var paged2 = products.ToAsyncPagedEnumerable(pagination);

// With factory
var paged3 = products.ToAsyncPagedEnumerable(async ct =>
{
    int total = await GetTotalCountAsync(ct);
    return Pagination.FromTotalCount(total);
});

// From IQueryable
var paged4 = _context.Products.ToAsyncPagedEnumerable();
```

---

## ?? Advanced Features

### LINQ Extensions

```csharp
using Xpandables.Net.AsyncPaged.Extensions;

IAsyncPagedEnumerable<Product> products = GetProductsAsync();

// Projection
var names = products.Select(p => p.Name);

// Filtering
var expensive = products.Where(p => p.Price > 100);

// Aggregation
int count = await products.CountAsync();
decimal total = await products.SumAsync(p => p.Price);
var max = await products.MaxAsync(p => p.Price);

// Materialization
List<Product> list = await products.ToListAsync();
Product[] array = await products.ToArrayAsync();

// Grouping
var grouped = products.GroupBy(p => p.Category);
```

---

## ?? Best Practices

1. **Use async all the way** - Don't block on async operations
2. **Stream large datasets** - Avoid materializing entire collections
3. **Apply filters before pagination** - Better query performance
4. **Consider caching total counts** - For expensive operations
5. **Use continuation tokens** - For cursor-based pagination

---

## ?? Related Packages

- **Xpandables.Net.AsyncPaged.AspNetCore** - ASP.NET Core integration
- **Xpandables.Net.Repositories.EntityFramework** - Repository pattern support

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
