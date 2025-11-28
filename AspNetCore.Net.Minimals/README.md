# ? AspNetCore.Net.Minimals

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Minimal API Extensions** - OperationResult integration, automatic validation, endpoint filters, middleware, and async paged responses for ASP.NET Core Minimal APIs.

---

## ?? Overview

`AspNetCore.Net.Minimals` provides comprehensive support for Minimal APIs with OperationResult integration, automatic validation, endpoint filters, exception handling middleware, and async paged response formatting.

### ? Key Features

- ?? **OperationResult Integration** - Automatic conversion of OperationResult to HTTP responses
- ? **Endpoint Validation Filter** - Automatic request validation for minimal API endpoints
- ?? **Result Endpoint Filter** - Consistent result transformation for all endpoints
- ?? **Exception Middleware** - Automatic exception to ProblemDetails conversion
- ?? **Async Paged Support** - Automatic formatting of IAsyncPagedEnumerable responses
- ?? **Custom Headers** - OperationResult metadata in HTTP response headers

---

## ?? Quick Start

### Installation

```bash
dotnet add package AspNetCore.Net.Minimals
```

### Basic Setup

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register minimal support services
builder.Services.AddXMinimalSupport();

var app = builder.Build();

// Use minimal support middleware and filters
app.UseXMinimalSupport(options =>
{
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
});

app.MapGet("/api/users/{id}", GetUser)
    .WithXMinimalApi();  // Applies validation and result filters

app.Run();

async Task<OperationResult<User>> GetUser(Guid id, IUserService service)
{
    return await service.GetUserAsync(id);
}
```

---

## ?? Core Features

### Automatic OperationResult Handling

```csharp
using System.ExecutionResults;

// Endpoint returns OperationResult - automatically converted to HTTP response
app.MapGet("/api/users/{id}", async (Guid id, IUserService service) =>
{
    OperationResult<User> result = await service.GetUserAsync(id);
    // Automatically returns appropriate HTTP status with ProblemDetails if failed
    return result;
})
.WithXMinimalFilter();

// Success responses
app.MapPost("/api/users", async (CreateUserRequest request, IUserService service) =>
{
    OperationResult<User> result = await service.CreateUserAsync(request);
    // Returns 201 Created with Location header if successful
    return result;
})
.WithXMinimalFilter();

// Error responses automatically formatted as ProblemDetails
app.MapPut("/api/users/{id}", async (Guid id, UpdateUserRequest request, IUserService service) =>
{
    OperationResult result = await service.UpdateUserAsync(id, request);
    // Returns 400, 404, etc. with ProblemDetails based on OperationResult status
    return result;
})
.WithXMinimalFilter();
```

### Endpoint Validation

```csharp
// Automatic validation for requests
app.MapPost("/api/products", async (CreateProductRequest request, IProductService service) =>
{
    // Validation happens automatically before this handler
    var result = await service.CreateProductAsync(request);
    return result;
})
.WithXMinimalValidation();  // Adds validation filter

// Request validation model
public record CreateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }
    
    [Required]
    public string Category { get; init; } = string.Empty;
}

// Invalid requests automatically return 400 Bad Request with validation errors
```

### Combined Minimal API Support

```csharp
// Apply both validation and result filtering
app.MapPost("/api/orders", async (CreateOrderRequest request, IOrderService service) =>
{
    var result = await service.CreateOrderAsync(request);
    return result;
})
.WithXMinimalApi();  // Applies both WithXMinimalValidation() and WithXMinimalFilter()

// Configure all endpoints
app.UseXMinimalSupport(options =>
{
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
    
    // Optionally apply filters selectively
    options.ConfigureEndpoint = (builder, endpoint) =>
    {
        var methods = endpoint.Metadata
            .OfType<HttpMethodMetadata>()
            .FirstOrDefault()?.HttpMethods ?? [];
        
        // Only apply to POST/PUT endpoints
        if (methods.Contains("POST") || methods.Contains("PUT"))
        {
            builder.WithXMinimalApi();
        }
    };
});
```

### Exception Handling Middleware

```csharp
using AspNetCore.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXMinimalSupport();

var app = builder.Build();

// Middleware catches unhandled exceptions and converts to ProblemDetails
app.UseXMinimalSupport();

app.MapGet("/api/data", () =>
{
    // Any exception is caught and converted to appropriate HTTP response
    throw new InvalidOperationException("Something went wrong");
    // Returns 500 Internal Server Error with ProblemDetails
});

app.MapGet("/api/bad-request", () =>
{
    throw new BadHttpRequestException("Invalid request parameter");
    // Returns 400 Bad Request with detailed error information
});

app.Run();
```

### Async Paged Responses

```csharp
using System.Collections.AsyncPaged;

// Return IAsyncPagedEnumerable - automatically formatted with pagination metadata
app.MapGet("/api/products", async (
    int pageSize,
    int pageIndex,
    IProductService service) =>
{
    IAsyncPagedEnumerable<Product> products = 
        await service.GetProductsAsync(pageSize, pageIndex);
    
    // Automatically includes pagination headers and formatted response
    return products;
})
.WithXMinimalFilter();

// Response includes pagination information:
// - X-Pagination-TotalCount
// - X-Pagination-PageSize
// - X-Pagination-CurrentPage
// - X-Pagination-TotalPages
```

---

## ?? Advanced Scenarios

### Custom Endpoint Predicate

```csharp
app.UseXMinimalSupport(options =>
{
    // Only apply to specific routes
    options.EndpointPredicate = endpoint =>
    {
        return endpoint.RoutePattern.RawText?.StartsWith("/api/") ?? false;
    };
    
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
});
```

### Selective Filter Application

```csharp
app.UseXMinimalSupport(options =>
{
    options.ConfigureEndpoint = (builder, endpoint) =>
    {
        var route = endpoint.RoutePattern.RawText;
        var methods = endpoint.Metadata
            .OfType<HttpMethodMetadata>()
            .FirstOrDefault()?.HttpMethods ?? [];
        
        // Apply validation only to mutation endpoints
        if (methods.Contains("POST") || 
            methods.Contains("PUT") || 
            methods.Contains("DELETE"))
        {
            builder.WithXMinimalValidation();
        }
        
        // Apply result filter to all API endpoints
        if (route?.StartsWith("/api/") ?? false)
        {
            builder.WithXMinimalFilter();
        }
        
        // Apply both to specific routes
        if (route?.Contains("/admin/") ?? false)
        {
            builder.WithXMinimalApi();
        }
    };
});
```

### Custom Validation

```csharp
// Custom validation with IMinimalResultEndpointValidator
public class CustomUserValidator : IMinimalResultEndpointValidator
{
    public Task<OperationResult> ValidateAsync(
        EndpointFilterInvocationContext context,
        CancellationToken cancellationToken)
    {
        var request = context.Arguments
            .OfType<CreateUserRequest>()
            .FirstOrDefault();
        
        if (request is null)
            return Task.FromResult(OperationResult.Ok().Build());
        
        if (request.Email.EndsWith("@test.com"))
        {
            return Task.FromResult(
                OperationResult
                    .BadRequest()
                    .WithError("Email", "Test emails not allowed")
                    .Build());
        }
        
        return Task.FromResult(OperationResult.Ok().Build());
    }
}

// Register validator
builder.Services.AddSingleton<IMinimalResultEndpointValidator, CustomUserValidator>();
```

### Custom Headers Configuration

```csharp
// Configure custom header writer
builder.Services.AddSingleton<IExecutionResultHeaderWriter, CustomHeaderWriter>();

public class CustomHeaderWriter : IExecutionResultHeaderWriter
{
    public void WriteHeaders(HttpContext context, OperationResult result)
    {
        context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
        context.Response.Headers["X-Status-Code"] = ((int)result.StatusCode).ToString();
        
        if (result.Extensions.Count > 0)
        {
            context.Response.Headers["X-Extensions"] = 
                JsonSerializer.Serialize(result.Extensions);
        }
    }
}
```

---

## ??? Complete Example

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.ExecutionResults;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddXMinimalSupport();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure minimal support
app.UseXMinimalSupport(options =>
{
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
});

// User endpoints
var users = app.MapGroup("/api/users").WithTags("Users");

users.MapGet("/", async (IUserService service) =>
{
    return await service.GetAllUsersAsync();
})
.WithXMinimalFilter();

users.MapGet("/{id}", async (Guid id, IUserService service) =>
{
    return await service.GetUserAsync(id);
})
.WithXMinimalFilter();

users.MapPost("/", async (CreateUserRequest request, IUserService service) =>
{
    return await service.CreateUserAsync(request);
})
.WithXMinimalApi();  // Validation + Result filter

users.MapPut("/{id}", async (Guid id, UpdateUserRequest request, IUserService service) =>
{
    return await service.UpdateUserAsync(id, request);
})
.WithXMinimalApi();

users.MapDelete("/{id}", async (Guid id, IUserService service) =>
{
    return await service.DeleteUserAsync(id);
})
.WithXMinimalFilter();

app.Run();

// Request models
public record CreateUserRequest
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;
    
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public record UpdateUserRequest
{
    [StringLength(100)]
    public string? Name { get; init; }
    
    [EmailAddress]
    public string? Email { get; init; }
}
```

---

## ?? Best Practices

1. **Use WithXMinimalApi()** - For mutation endpoints (POST/PUT/DELETE)
2. **Use WithXMinimalFilter()** - For query endpoints (GET)
3. **Register AddXMinimalSupport()** - Always register services before using middleware
4. **Configure selectively** - Apply filters based on endpoint requirements
5. **Return OperationResult** - Let filters handle HTTP response conversion
6. **Use validation attributes** - Leverage built-in validation on request models

---

## ?? Related Packages

- **AspNetCore.Net** - Core ASP.NET integration
- **System.ExecutionResults** - OperationResult types
- **System.Collections.AsyncPaged** - Async pagination support

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
