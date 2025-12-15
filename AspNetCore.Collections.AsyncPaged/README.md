# 🌐 AspNetCore.AsyncPaged

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/AspNetCore.Collections.AsyncPaged)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **ASP.NET Core Integration for Async Paged Enumerables** — Stream paged JSON responses with automatic pagination metadata, minimal API filters, MVC output formatters, and efficient serialization.

---

## 📋 Overview

`AspNetCore.Collections.AsyncPaged` provides seamless ASP.NET Core integration for `IAsyncPagedEnumerable<T>`, enabling efficient streaming of paginated data directly to HTTP responses. The library automatically wraps paged data with pagination metadata, supports both minimal APIs and MVC controllers, and leverages `System.Text.Json` for high-performance JSON serialization.

Built for .NET 10 with C# 14 extension members, this package bridges the gap between your data layer and HTTP responses with zero boilerplate.

### ✨ Key Features

- 🔄 **`IAsyncPagedEnumerable<T>` to `IResult`** — Direct conversion from paged enumerables to HTTP results
- 🛡️ **Endpoint Filters** — Automatic transformation of paged responses via `WithXAsyncPagedFilter()`
- 📝 **MVC Output Formatter** — Custom `TextOutputFormatter` for controller-based APIs
- ⚡ **Streaming JSON Serialization** — Memory-efficient serialization via `PipeWriter` and `Stream`
- 🎯 **Structured Response Format** — Consistent `{ "pagination": {...}, "items": [...] }` output
- 🚀 **AOT Compatible** — Source-generated JSON serialization with `PaginationJsonContext`
- 🔧 **Flexible Configuration** — Custom `JsonSerializerOptions` and `JsonTypeInfo<T>` support

---

## 📦 Installation

```bash
dotnet add package AspNetCore.Collections.AsyncPaged
```

Or via NuGet Package Manager:

```powershell
Install-Package AspNetCore.Collections.AsyncPaged
```

---

## 🚀 Quick Start

### Minimal API with Endpoint Filter

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Apply the async paged filter to automatically transform IAsyncPagedEnumerable responses
app.MapGet("/api/products", async (ProductService productService, CancellationToken ct) =>
{
    IAsyncPagedEnumerable<Product> products = productService.GetProductsAsync(pageNumber: 1, pageSize: 20);
    return products; // Filter transforms this to structured JSON response
})
.WithXAsyncPagedFilter();

app.Run();
```

**Response Output:**
```json
{
  "pagination": {
    "totalCount": 150,
    "pageSize": 20,
    "currentPage": 1,
    "continuationToken": null
  },
  "items": [
    { "id": 1, "name": "Product A", "price": 29.99 },
    { "id": 2, "name": "Product B", "price": 49.99 }
  ]
}
```

### Manual Result Conversion

```csharp
app.MapGet("/api/orders", async (OrderService orderService, CancellationToken ct) =>
{
    IAsyncPagedEnumerable<Order> orders = orderService.GetOrdersAsync(pageNumber: 2, pageSize: 50);
    
    // Explicitly convert to IResult for full control
    return orders.ToResult();
});
```

### With Custom JSON Options

```csharp
app.MapGet("/api/customers", async (CustomerService customerService, CancellationToken ct) =>
{
    IAsyncPagedEnumerable<Customer> customers = customerService.GetCustomersAsync();
    
    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    return customers.ToResult(jsonOptions);
});
```

---

## 🏗️ MVC Controller Support

### Register MVC Options

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add MVC with async paged output formatter
builder.Services.AddControllers();
builder.Services.AddXControllerAsyncPagedMvcOptions();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Controller Implementation

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public IAsyncPagedEnumerable<Product> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Return IAsyncPagedEnumerable directly - formatter handles serialization
        return productService.GetProductsAsync(page, pageSize);
    }

    [HttpGet("category/{categoryId}")]
    public IAsyncPagedEnumerable<Product> GetByCategory(
        int categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return productService.GetByCategoryAsync(categoryId, page, pageSize);
    }
}
```

The `ControllerResultAsyncPagedOutputFormatter` automatically:
- Detects `IAsyncPagedEnumerable<T>` return types
- Streams JSON with pagination metadata wrapper
- Supports UTF-8 and transcoding for other encodings
- Handles cancellation gracefully

---

## 🔧 Extension Methods

### `IAsyncPagedEnumerable<T>` Extensions

```csharp
using Microsoft.AspNetCore.Http;

IAsyncPagedEnumerable<Product> products = GetProductsAsync();

// Convert to IResult (uses default JSON options from DI)
IResult result1 = products.ToResult();

// Convert with custom JsonSerializerOptions
IResult result2 = products.ToResult(new JsonSerializerOptions { WriteIndented = true });

// Convert with source-generated JsonTypeInfo for AOT
IResult result3 = products.ToResult(ProductJsonContext.Default.Product);
```

### Endpoint Convention Builder Extensions

```csharp
// Apply async paged filter to transform responses
app.MapGet("/api/items", GetItems)
    .WithXAsyncPagedFilter();

// Works with route groups
var apiGroup = app.MapGroup("/api")
    .WithXAsyncPagedFilter(); // Applied to all endpoints in group

apiGroup.MapGet("/products", GetProducts);
apiGroup.MapGet("/orders", GetOrders);
```

---

## 📊 JSON Response Structure

All paged responses follow a consistent structure:

```json
{
  "pagination": {
    "totalCount": 500,
    "pageSize": 25,
    "currentPage": 2,
    "continuationToken": "offset:50"
  },
  "items": [
    // Array of serialized items
  ]
}
```

### Pagination Properties

| Property | Type | Description |
|----------|------|-------------|
| `totalCount` | `int?` | Total items across all pages (null if unknown) |
| `pageSize` | `int` | Number of items per page |
| `currentPage` | `int` | Current page number (1-based) |
| `continuationToken` | `string?` | Token for cursor-based pagination |

---

## ⚡ Streaming Serialization

The library uses efficient streaming serialization that:

1. **Computes pagination metadata first** via `GetPaginationAsync()`
2. **Writes the opening structure** (`{ "pagination": {...}, "items": [`)
3. **Streams items incrementally** as they're enumerated
4. **Adaptive flushing** based on dataset size and memory pressure

### Flush Strategy

| Dataset Size | Batch Size | Description |
|--------------|------------|-------------|
| Unknown | 100 items | Default for streaming sources |
| < 1,000 | 200 items | Small datasets - less frequent flushing |
| < 10,000 | 100 items | Medium datasets |
| < 100,000 | 50 items | Large datasets - more frequent flushing |
| ≥ 100,000 | 25 items | Very large datasets - maximum responsiveness |

Additionally, flushing occurs when pending bytes exceed 32KB regardless of item count.

---

## 🔄 Integration with Data Layer

### Entity Framework Core Example

```csharp
public class ProductService(AppDbContext context)
{
    public IAsyncPagedEnumerable<Product> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 20)
    {
        return context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToAsyncPagedEnumerable(); // Automatically extracts pagination from Skip/Take
    }

    public IAsyncPagedEnumerable<Product> GetByCategoryAsync(
        int categoryId,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = context.Products
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt);

        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToAsyncPagedEnumerable(async ct =>
            {
                // Custom pagination factory for accurate total count
                int total = await context.Products
                    .CountAsync(p => p.CategoryId == categoryId && p.IsActive, ct);
                
                return Pagination.Create(
                    pageSize: pageSize,
                    currentPage: pageNumber,
                    totalCount: total);
            });
    }
}
```

### Cursor-Based Pagination Example

```csharp
public class ActivityService(AppDbContext context)
{
    public IAsyncPagedEnumerable<Activity> GetActivitiesAsync(
        string? continuationToken = null,
        int pageSize = 50)
    {
        var query = context.Activities.AsQueryable();

        if (TryDecodeCursor(continuationToken, out DateTime cursor))
        {
            query = query.Where(a => a.Timestamp < cursor);
        }

        var items = query
            .OrderByDescending(a => a.Timestamp)
            .Take(pageSize);

        return items.ToAsyncPagedEnumerable(async ct =>
        {
            var activities = await items.ToListAsync(ct);
            string? nextToken = activities.Count == pageSize
                ? EncodeCursor(activities[^1].Timestamp)
                : null;

            return Pagination.Create(
                pageSize: pageSize,
                currentPage: 1,
                continuationToken: nextToken,
                totalCount: null); // Unknown for cursor-based
        });
    }

    private static bool TryDecodeCursor(string? token, out DateTime cursor)
    {
        cursor = default;
        if (string.IsNullOrEmpty(token)) return false;
        return DateTime.TryParse(token, out cursor);
    }

    private static string EncodeCursor(DateTime timestamp) => 
        timestamp.ToString("O");
}
```

---

## 🛡️ Endpoint Filter Details

The `AsyncPagedEnpointFilter` automatically:

1. **Intercepts endpoint results** after handler execution
2. **Detects `IAsyncPagedEnumerable` types** (including wrapped in `ObjectResult`)
3. **Creates `ResultAsyncPaged<T>`** with proper generic type resolution
4. **Returns the structured result** for serialization

```csharp
// The filter transforms this:
app.MapGet("/api/data", () => GetDataAsync().ToAsyncPagedEnumerable());

// Into this effective behavior:
app.MapGet("/api/data", () => 
{
    var paged = GetDataAsync().ToAsyncPagedEnumerable();
    return new ResultAsyncPaged<DataItem>(paged);
});
```

---

## 📝 MVC Output Formatter Configuration

The `ControllerAsyncPagedMvcOptions` configures MVC with:

```csharp
public void Configure(MvcOptions options)
{
    options.EnableEndpointRouting = false;
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;

    // Insert formatter at position 0 for priority
    options.OutputFormatters.Insert(0, 
        new ControllerResultAsyncPagedOutputFormatter(jsonSerializerOptions));
}
```

### Supported Media Types

- `application/json`
- `text/json`
- `application/*+json`

### Supported Encodings

- UTF-8 (optimized path via `PipeWriter`)
- Unicode (transcoding stream fallback)

---

## ✅ Best Practices

### ✅ Do

- **Use `WithXAsyncPagedFilter()`** for minimal APIs returning `IAsyncPagedEnumerable<T>`
- **Register `AddXControllerAsyncPagedMvcOptions()`** for MVC controller support
- **Provide custom pagination factories** for accurate total counts with complex queries
- **Use source-generated `JsonTypeInfo<T>`** for AOT-compatible serialization
- **Return `IAsyncPagedEnumerable<T>` directly** from handlers - let the framework handle conversion

### ❌ Don't

- **Materialize entire collections** before returning - leverage streaming
- **Ignore cancellation tokens** - pass them through to data layer operations
- **Mix manual `ToResult()` with endpoint filter** - choose one approach per endpoint
- **Forget to compute pagination** - call `GetPaginationAsync()` before serialization

---

## 🔧 Advanced: Custom Result Implementation

For full control over serialization:

```csharp
app.MapGet("/api/custom", async (DataService service, HttpContext context) =>
{
    var paged = service.GetDataAsync();
    var pagination = await paged.GetPaginationAsync(context.RequestAborted);
    
    // Access pagination for custom headers
    context.Response.Headers["X-Total-Count"] = pagination.TotalCount?.ToString();
    context.Response.Headers["X-Page-Size"] = pagination.PageSize.ToString();
    
    return paged.ToResult();
});
```

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Collections.AsyncPaged** | Core `IAsyncPagedEnumerable<T>` and `Pagination` types |
| **System.Text.Json.AsyncPaged** | JSON serialization extensions for paged enumerables |
| **System.Linq.AsyncPaged** | LINQ operators (`SelectManyPaged`, transformations) |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

