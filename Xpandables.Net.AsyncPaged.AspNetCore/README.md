# ?? Xpandables.Net.AsyncPaged.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Integration** - Seamless pagination support for web APIs with automatic HTTP response formatting.

---

## ?? Overview

`Xpandables.Net.AsyncPaged.AspNetCore` provides ASP.NET Core integration for `IAsyncPagedEnumerable<T>`, enabling automatic JSON streaming responses with pagination headers. Perfect for building efficient, scalable web APIs.

### ?? Key Features

- ?? **HTTP Integration** - Automatic pagination header injection
- ?? **JSON Streaming** - Stream large datasets directly to HTTP response
- ?? **Controller Support** - Works with both MVC controllers and minimal APIs
- ?? **Automatic Metadata** - Pagination info in response headers
- ? **High Performance** - Zero-copy streaming where possible
- ?? **Configurable** - Customize JSON serialization options

---

## ?? Quick Start

### Installation

```bash
dotnet add package Xpandables.Net.AsyncPaged.AspNetCore
```

### Configuration

```csharp
using Xpandables.Net.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register MVC with async paged enumerable support
builder.Services
    .AddControllers()
    .ConfigureIAsyncPagedEnumerableMvcOptions();

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## ?? Controller Examples

### Basic API Endpoint

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public ProductsController(AppDbContext context) => _context = context;
    
    [HttpGet]
    public IAsyncPagedEnumerable<ProductDto> GetProducts(
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        return _context.Products
            .Where(p => p.IsActive)
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
}

// Response automatically includes pagination headers:
// X-Pagination-TotalCount: 1000
// X-Pagination-PageSize: 20
// X-Pagination-CurrentPage: 1
// X-Pagination-TotalPages: 50
```

### Advanced Search Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public ProductsController(AppDbContext context) => _context = context;
    
    [HttpGet("search")]
    public IAsyncPagedEnumerable<ProductDto> SearchProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy = "name",
        [FromQuery] bool descending = false,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive);
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => 
                p.Name.Contains(search) || 
                p.Description.Contains(search));
        }
        
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Name == category);
        }
        
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice);
        }
        
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }
        
        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "price" => descending 
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "date" => descending
                ? query.OrderByDescending(p => p.CreatedDate)
                : query.OrderBy(p => p.CreatedDate),
            _ => descending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };
        
        // Apply pagination
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Category = p.Category.Name,
                CreatedDate = p.CreatedDate,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock
            })
            .ToAsyncPagedEnumerable();
    }
}
```

---

## ?? Minimal API Examples

### Basic Minimal API

```csharp
using Microsoft.EntityFrameworkCore;
using Xpandables.Net.AsyncPaged;
using Xpandables.Net.AsyncPaged.Extensions;
using Xpandables.Net.AsyncPaged.Minimals;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ProductDtoContext.Default);
});

var app = builder.Build();

app.MapGet("/api/products", async (
    AppDbContext context,
    int pageSize = 20,
    int pageNumber = 1) =>
{
    var products = context.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        })
        .ToAsyncPagedEnumerable();
    
    return products.ToResult(ProductDtoContext.Default.ProductDto);
});

app.Run();

[JsonSerializable(typeof(ProductDto))]
public partial class ProductDtoContext : JsonSerializerContext { }
```

### Advanced Minimal API with Filters

```csharp
app.MapGet("/api/orders", async (
    AppDbContext context,
    Guid? userId,
    DateTime? fromDate,
    DateTime? toDate,
    OrderStatus? status,
    int pageSize = 10,
    int pageNumber = 1) =>
{
    var query = context.Orders
        .Include(o => o.Items)
        .AsQueryable();
    
    if (userId.HasValue)
        query = query.Where(o => o.UserId == userId);
    
    if (fromDate.HasValue)
        query = query.Where(o => o.CreatedDate >= fromDate);
    
    if (toDate.HasValue)
        query = query.Where(o => o.CreatedDate <= toDate);
    
    if (status.HasValue)
        query = query.Where(o => o.Status == status);
    
    var orders = query
        .OrderByDescending(o => o.CreatedDate)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CreatedDate = o.CreatedDate,
            Status = o.Status,
            TotalAmount = o.Items.Sum(i => i.Price * i.Quantity)
        })
        .ToAsyncPagedEnumerable();
    
    return orders.ToResult(OrderDtoContext.Default.OrderDto);
})
.WithName("GetOrders")
.WithOpenApi();
```

---

## ?? Configuration Options

### Custom JSON Serialization

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Xpandables.Net.AsyncPaged;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.WriteIndented = false;
});

// For MVC
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .ConfigureIAsyncPagedEnumerableMvcOptions();
```

### Custom Result Formatting

```csharp
app.MapGet("/api/products", async (
    AppDbContext context,
    HttpContext httpContext,
    int pageSize = 20,
    int pageNumber = 1) =>
{
    var products = context.Products
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToAsyncPagedEnumerable();
    
    // Use custom serialization options
    var options = httpContext.GetJsonSerializerOptions();
    return products.ToResult(options);
});
```

---

## ?? Real-World API Examples

### E-Commerce Product API

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(
        AppDbContext context,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Search products with filtering and pagination
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Paginated product list</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IAsyncPagedEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public IAsyncPagedEnumerable<ProductDto> SearchProducts(
        [FromQuery] ProductSearchRequest request)
    {
        _logger.LogInformation(
            "Searching products: Search={Search}, Category={Category}, Page={Page}",
            request.Search, request.Category, request.PageNumber);
        
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => 
                EF.Functions.Like(p.Name, $"%{request.Search}%") ||
                EF.Functions.Like(p.Description, $"%{request.Search}%"));
        }
        
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category.Name == request.Category);
        }
        
        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice);
        }
        
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice);
        }
        
        if (request.InStock)
        {
            query = query.Where(p => p.Stock > 0);
        }
        
        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.Descending 
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "rating" => request.Descending
                ? query.OrderByDescending(p => p.AverageRating)
                : query.OrderBy(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedDate),
            _ => query.OrderBy(p => p.Name)
        };
        
        return query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Category = p.Category.Name,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock,
                AverageRating = p.AverageRating,
                ReviewCount = p.Reviews.Count
            })
            .ToAsyncPagedEnumerable();
    }
    
    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IAsyncPagedEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IAsyncPagedEnumerable<ProductDto>>> GetByCategory(
        string category,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Name == category);
        
        if (!categoryExists)
        {
            return NotFound(new { message = $"Category '{category}' not found" });
        }
        
        var products = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Category.Name == category)
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
        
        return Ok(products);
    }
}

public record ProductSearchRequest
{
    public string? Search { get; init; }
    public string? Category { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool InStock { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }
    public int PageSize { get; init; } = 20;
    public int PageNumber { get; init; } = 1;
}

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int Stock { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
```

### Analytics API with Aggregation

```csharp
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public AnalyticsController(AppDbContext context) => _context = context;
    
    [HttpGet("sales-by-product")]
    public IAsyncPagedEnumerable<ProductSalesDto> GetSalesReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int pageSize = 50,
        [FromQuery] int pageNumber = 1)
    {
        return _context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedDate >= fromDate 
                      && oi.Order.CreatedDate <= toDate
                      && oi.Order.Status == OrderStatus.Completed)
            .GroupBy(oi => new 
            { 
                oi.ProductId, 
                oi.Product.Name,
                oi.Product.Category.Name
            })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                Category = g.Key.Name,
                TotalUnits = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity),
                AveragePrice = g.Average(oi => oi.Price),
                OrderCount = g.Count()
            })
            .OrderByDescending(s => s.TotalRevenue)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToAsyncPagedEnumerable();
    }
}

public record ProductSalesDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int TotalUnits { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AveragePrice { get; init; }
    public int OrderCount { get; init; }
}
```

---

## ?? Response Format

### Automatic Headers

When returning `IAsyncPagedEnumerable<T>`, the following headers are automatically added:

```
HTTP/1.1 200 OK
Content-Type: application/json
X-Pagination-TotalCount: 1000
X-Pagination-PageSize: 20
X-Pagination-CurrentPage: 1
X-Pagination-TotalPages: 50
X-Pagination-HasNextPage: true
X-Pagination-HasPreviousPage: false
```

### JSON Response Body

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Product 1",
    "price": 29.99,
    "category": "Electronics"
  },
  {
    "id": "8b3c5e12-9f8d-4a5b-b2c1-7d6e4f8a9b0c",
    "name": "Product 2",
    "price": 49.99,
    "category": "Electronics"
  }
]
```

---

## ?? HttpContext Extensions

```csharp
using Xpandables.Net.AsyncPaged;

app.MapGet("/api/custom", async (HttpContext httpContext, AppDbContext context) =>
{
    // Get JSON options from context
    var jsonOptions = httpContext.GetJsonSerializerOptions();
    
    // Or MVC-specific options
    var mvcOptions = httpContext.GetMvcJsonSerializerOptions();
    
    // Get content type
    var contentType = httpContext.GetContentType("application/json");
    
    var products = context.Products
        .ToAsyncPagedEnumerable();
    
    return products.ToResult(jsonOptions);
});
```

---

## ?? Best Practices

1. **Use query parameters** - For pagination and filtering options
2. **Include pagination headers** - Automatic with this library
3. **Validate page numbers** - Handle invalid requests gracefully
4. **Set reasonable defaults** - PageSize = 20 is a good starting point
5. **Document your APIs** - Use OpenAPI/Swagger annotations
6. **Use DTOs** - Don't expose domain entities directly
7. **Apply filters before pagination** - For optimal database performance

---

## ?? Related Packages

- **Xpandables.Net.AsyncPaged** - Core pagination library
- **Xpandables.Net.ExecutionResults.AspNetCore** - Result pattern for APIs
- **Xpandables.Net.Repositories.EntityFramework** - Repository implementation

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
