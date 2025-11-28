# ?? AspNetCore.Net.Controllers

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **MVC Controller Extensions** - OperationResult integration, automatic validation, async paged output formatting, and action filters for ASP.NET Core MVC controllers.

---

## ?? Overview

`AspNetCore.Net.Controllers` provides comprehensive MVC controller support with OperationResult integration, automatic model validation, async paged response formatting, and result filters for consistent API responses.

### ? Key Features

- ?? **ControllerResultFilter** - Automatic OperationResult to HTTP response conversion
- ? **Validation Filter** - Automatic ModelState validation before action execution
- ?? **AsyncPaged Output Formatter** - JSON serialization for IAsyncPagedEnumerable responses
- ?? **MVC Options Configuration** - Pre-configured MVC settings for OperationResult support
- ?? **Header Writing** - OperationResult metadata in response headers

---

## ?? Quick Start

### Installation

```bash
dotnet add package AspNetCore.Net.Controllers
```

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with OperationResult support
builder.Services
    .AddControllers()
    .AddXControllerMvcOptions();  // Adds validation, filters, and formatters

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## ?? Core Features

### OperationResult in Controllers

```csharp
using System.ExecutionResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService) => 
        _userService = userService;
    
    [HttpGet("{id}")]
    public async Task<OperationResult<User>> GetUser(Guid id)
    {
        // Return OperationResult - automatically converted to HTTP response
        return await _userService.GetUserAsync(id);
    }
    
    [HttpPost]
    public async Task<OperationResult<User>> CreateUser(CreateUserRequest request)
    {
        // Validation happens automatically
        // OperationResult is converted to appropriate HTTP status
        return await _userService.CreateUserAsync(request);
    }
    
    [HttpPut("{id}")]
    public async Task<OperationResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        return await _userService.UpdateUserAsync(id, request);
    }
    
    [HttpDelete("{id}")]
    public async Task<OperationResult> DeleteUser(Guid id)
    {
        return await _userService.DeleteUserAsync(id);
    }
}
```

### Async Paged Responses

```csharp
using System.Collections.AsyncPaged;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    [HttpGet]
    public IAsyncPagedEnumerable<Product> GetProducts(
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageIndex = 1)
    {
        // Automatically formatted with pagination metadata in response
        return _productService.GetProductsAsync(pageSize, pageIndex);
    }
}

// Response format:
// {
//   "pagination": {
//     "pageSize": 20,
//     "currentPage": 1,
//     "totalCount": 150,
//     "totalPages": 8
//   },
//   "items": [ {...}, {...} ]
// }
```

### Automatic Validation

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<OperationResult<Order>> CreateOrder(
        CreateOrderRequest request)
    {
        // ModelState is validated automatically
        // Invalid requests return 400 with validation errors
        return await _orderService.CreateOrderAsync(request);
    }
}

public record CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; init; }
    
    [Required]
    [MinLength(1)]
    public List<OrderItem> Items { get; init; } = [];
    
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; init; }
}
```

---

## ?? Advanced Scenarios

### Custom Result Filters

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: OperationResult execution })
        {
            // Custom processing
            context.HttpContext.Response.Headers["X-Custom"] = "Value";
        }
        
        await next();
    }
}

// Register
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomResultFilter>();
});
```

### Selective Validation

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // Skip automatic validation for this endpoint
    [HttpPost("bulk")]
    [SkipControllerResultValidation]
    public async Task<OperationResult> BulkImport([FromBody] string csvData)
    {
        // Custom validation logic
        return await _adminService.BulkImportAsync(csvData);
    }
}
```

### Custom Output Formatters

```csharp
builder.Services.AddControllers(options =>
{
    // AsyncPaged formatter is added automatically by AddXControllerMvcOptions
    // But you can add custom formatters
    options.OutputFormatters.Insert(0, new CustomFormatter());
});
```

---

## ?? Best Practices

1. **Return OperationResult** - Use OperationResult as return type for consistent responses
2. **Use validation attributes** - Leverage DataAnnotations for request validation
3. **Return IAsyncPagedEnumerable** - For paginated collections
4. **Register AddXControllerMvcOptions()** - Always register for full functionality
5. **Avoid manual status codes** - Let OperationResult determine HTTP status

---

## ?? Related Packages

- **AspNetCore.Net** - Core ASP.NET integration
- **System.ExecutionResults** - OperationResult types
- **System.Collections.AsyncPaged** - Async pagination

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
