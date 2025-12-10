# 🌐 AspNetCore.Results

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/AspNetCore.Results)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **ASP.NET Core Integration for Result Pattern** — Convert Results to HTTP responses, ProblemDetails, and ModelState with endpoint filters, MVC filters, middleware, and validation support.

---

## 📋 Overview

`AspNetCore.Results` provides comprehensive ASP.NET Core integration for the `System.Results` Result pattern. The library bridges the gap between your domain's `Result` type and ASP.NET Core's HTTP infrastructure, enabling automatic result-to-response conversion, validation filters, exception handling middleware, and consistent ProblemDetails responses.

Built for .NET 10 with C# 14 extension members, this package ensures your APIs return consistent, well-structured responses whether using minimal APIs or MVC controllers.

### ✨ Key Features

- 🔄 **Result Extensions** — Convert `Result` to `IActionResult`, `ProblemDetails`, and `ModelStateDictionary`
- 🛡️ **Endpoint Filters** — `WithXResultSupport()`, `WithXResultFilter()`, `WithXResultValidation()` for minimal APIs
- 🏗️ **MVC Filters** — `ControllerResultFilter` and `ControllerResultValidationFilterAttribute` for controllers
- 📝 **Header Writing** — `IResultHeaderWriter` for custom response header handling
- ⚡ **Exception Middleware** — `ResultMiddleware` for global exception-to-ProblemDetails conversion
- ✅ **Validation Integration** — Automatic validation via `IResultEndpointValidator` and `IRuleValidatorProvider`
- 🔐 **Authentication Support** — Automatic `WWW-Authenticate` header for 401 responses

---

## 📦 Installation

```bash
dotnet add package AspNetCore.Results
```

Or via NuGet Package Manager:

```powershell
Install-Package AspNetCore.Results
```

---

## 🚀 Quick Start

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register Result middleware for exception handling
builder.Services.AddXResultMiddleware();

// Register endpoint validator with default implementation
builder.Services.AddXResultEndpointValidator();

// Register MVC options for controller Result support
builder.Services.AddXControllerResultMvcOptions();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Use Result middleware for global exception handling
app.UseXResultMiddleware();

app.MapControllers();
app.Run();
```

---

## 🛡️ Minimal API Endpoint Filters

### Apply Filters to Endpoints

```csharp
using Microsoft.AspNetCore.Http;
using System.Results;

var app = builder.Build();

// Apply both validation and result filters
app.MapPost("/api/orders", async (CreateOrderRequest request, IOrderService orderService) =>
{
    Result<Order> result = await orderService.CreateAsync(request);
    return result; // Filter converts Result to HTTP response
})
.WithXResultSupport();

// Apply only result filter (converts Result to response)
app.MapGet("/api/orders/{id}", async (Guid id, IOrderService orderService) =>
{
    Result<Order> result = await orderService.GetByIdAsync(id);
    return result;
})
.WithXResultFilter();

// Apply only validation filter
app.MapPost("/api/products", async (CreateProductRequest request, IProductService productService) =>
{
    Result<Product> result = await productService.CreateAsync(request);
    return result;
})
.WithXResultValidation();

app.Run();
```

### How Filters Work

The `ResultEndpointFilter` automatically:
1. **Intercepts `Result` responses** from endpoint handlers
2. **Writes response headers** via `IResultHeaderWriter` (status code, Location, custom headers)
3. **Converts failures to ProblemDetails** using the configured `IProblemDetailsService`
4. **Extracts success values** and returns them as the response body
5. **Handles exceptions** and converts them to structured error responses

```csharp
// Example: Handler returns Result<T>
app.MapGet("/api/users/{id}", async (Guid id, IUserService userService) =>
{
    // Service returns Result<User>
    return await userService.GetByIdAsync(id);
})
.WithXResultFilter();

// On success: Returns User with 200 OK
// On failure: Returns ProblemDetails with appropriate status code
```

---

## 🔄 Result Extensions

### Convert Result to IActionResult (MVC)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Results;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        Result<User> result = await userService.GetByIdAsync(id, ct);
        
        // Converts Result to ObjectResult with correct status code
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken ct)
    {
        Result<User> result = await userService.CreateAsync(request, ct);
        return result.ToActionResult();
    }
}
```

### Convert Result to ProblemDetails

```csharp
using Microsoft.AspNetCore.Http;
using System.Results;

// Manual conversion to ProblemDetails
app.MapGet("/api/custom/{id}", async (Guid id, HttpContext context, IDataService dataService) =>
{
    Result<Data> result = await dataService.GetAsync(id);
    
    if (result.IsFailure)
    {
        ProblemDetails problem = result.ToProblemDetails(context);
        return Results.Problem(problem);
    }
    
    return Results.Ok(result.Value);
});
```

### Convert Result to ModelStateDictionary

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Results;

[HttpPost]
public async Task<IActionResult> CreateWithView(CreateItemRequest request, CancellationToken ct)
{
    Result<Item> result = await _itemService.CreateAsync(request, ct);

    if (result.IsFailure)
    {
        // Convert Result errors to ModelState for view rendering
        ModelStateDictionary modelState = result.ToModelStateDictionary();
        
        foreach (var error in modelState)
        {
            foreach (var stateError in error.Value.Errors)
            {
                ModelState.AddModelError(error.Key, stateError.ErrorMessage);
            }
        }
        
        return View(request);
    }

    return RedirectToAction("Details", new { id = result.Value!.Id });
}
```

### Convert ModelState to Result

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Results;
using System.Net;

[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken ct)
{
    if (!ModelState.IsValid)
    {
        // Convert ModelState errors to Result
        Result validationResult = ModelState.ToResult(HttpStatusCode.BadRequest);
        return validationResult.ToActionResult();
    }

    Result<User> result = await _userService.CreateAsync(request, ct);
    return result.ToActionResult();
}
```

### Convert BadHttpRequestException to Result

```csharp
using Microsoft.AspNetCore.Http;
using System.Results;

// The library automatically handles this in ResultMiddleware
// But you can also use it manually:
try
{
    // Some operation that might throw
}
catch (BadHttpRequestException ex)
{
    Result result = ex.ToResult();
    // result.StatusCode = exception's status code
    // result.Errors contains the parameter name and error message
}
```

---

## 🏗️ MVC Controller Support

### Automatic Result Filter Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register MVC options with Result filters
builder.Services.AddXControllerResultMvcOptions();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

This registers:
- `ControllerResultValidationFilterAttribute` — Validates ModelState before action execution
- `ControllerResultFilter` — Processes `Result` return types and writes headers

### Controller with Result Support

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Results;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<Result<IEnumerable<Product>>> GetProducts(CancellationToken ct)
    {
        // Return Result directly - filter handles conversion
        return await productService.GetAllAsync(ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<Result<Product>> GetProduct(Guid id, CancellationToken ct)
    {
        return await productService.GetByIdAsync(id, ct);
    }

    [HttpPost]
    public async Task<Result<Product>> CreateProduct(CreateProductRequest request, CancellationToken ct)
    {
        // ControllerResultValidationFilterAttribute validates ModelState
        // ControllerResultFilter handles Result response
        return await productService.CreateAsync(request, ct);
    }
}
```

### Manual Filter Application

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Results;

[ApiController]
[Route("api/[controller]")]
[ControllerResultValidationFilterAttribute] // Validates ModelState
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request, CancellationToken ct)
    {
        // ModelState already validated by filter
        Result<Order> result = await orderService.CreateAsync(request, ct);
        return result.ToActionResult();
    }
}
```

---

## ⚡ Exception Handling Middleware

The `ResultMiddleware` provides global exception handling:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register middleware
builder.Services.AddXResultMiddleware();

var app = builder.Build();

// Use middleware - catches unhandled exceptions
app.UseXResultMiddleware();

app.MapGet("/api/risky", () =>
{
    throw new InvalidOperationException("Something went wrong!");
    // Middleware converts to ProblemDetails response
});

app.Run();
```

### Exception Handling Behavior

The middleware handles:
- `BadHttpRequestException` → Converts using `ToResult()` extension
- `ResultException` → Extracts the embedded `Result`
- Other exceptions → Converts using `exception.ToResult()`

All exceptions are converted to `ProblemDetails` responses when the response hasn't started.

---

## 📝 Custom Header Writing

### Default Header Writer

The `ResultHeaderWriter` automatically:
- Sets `Content-Type` header
- Sets response status code from `Result.StatusCode`
- Adds `Location` header if `Result.Location` is set
- Appends custom headers from `Result.Headers`
- Adds `WWW-Authenticate` header for 401 responses

### Custom Header Writer Implementation

```csharp
using Microsoft.AspNetCore.Http;
using System.Results;

public sealed class CustomResultHeaderWriter : IResultHeaderWriter
{
    public async Task WriteAsync(HttpContext context, Result result)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)result.StatusCode;

        // Add custom correlation header
        if (result.Extensions.TryGetValue("CorrelationId", out var correlationId))
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId?.ToString();
        }

        // Add timing header
        context.Response.Headers["X-Response-Time"] = DateTime.UtcNow.ToString("O");

        // Handle Location header for created resources
        if (result.Location is not null)
        {
            context.Response.Headers.Location = result.Location.ToString();
        }

        await Task.CompletedTask;
    }
}

// Register custom writer
builder.Services.AddXResultHeaderWriter<CustomResultHeaderWriter>();
```

---

## ✅ Validation Integration

### Automatic Validation with IRequiresValidation

The `ResultEndpointValidator` automatically validates endpoint arguments that implement `IRequiresValidation`:

```csharp
using System.Results;

// Request implements IRequiresValidation
public record CreateOrderRequest(string CustomerId, List<OrderItem> Items) : IRequiresValidation;

// Validator for the request
public sealed class CreateOrderRequestValidator : IRuleValidator<CreateOrderRequest>
{
    public async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(
        CreateOrderRequest instance,
        CancellationToken cancellationToken = default)
    {
        List<ValidationResult> results = [];

        if (string.IsNullOrWhiteSpace(instance.CustomerId))
        {
            results.Add(new ValidationResult("CustomerId is required", ["CustomerId"]));
        }

        if (instance.Items is null || instance.Items.Count == 0)
        {
            results.Add(new ValidationResult("At least one item is required", ["Items"]));
        }

        return results;
    }
}

// Register validator
builder.Services.AddXResultEndpointValidator();
builder.Services.AddScoped<IRuleValidator<CreateOrderRequest>, CreateOrderRequestValidator>();

// Endpoint with automatic validation
app.MapPost("/api/orders", async (CreateOrderRequest request, IOrderService orderService) =>
{
    // Validator runs before handler if request implements IRequiresValidation
    return await orderService.CreateAsync(request);
})
.WithXResultValidation()
.WithXResultFilter();
```

---

## 🔧 Extension Methods Summary

### IServiceCollection Extensions

| Method | Description |
|--------|-------------|
| `AddXResultMiddleware()` | Registers `ResultMiddleware` for exception handling |
| `AddXResultEndpointValidator()` | Registers default endpoint validator with dependencies |
| `AddXResultEndpointValidator<T>()` | Registers custom endpoint validator |
| `AddXResultHeaderWriter()` | Registers default header writer |
| `AddXResultHeaderWriter<T>()` | Registers custom header writer |
| `AddXControllerResultMvcOptions()` | Registers MVC options with Result filters |

### IApplicationBuilder Extensions

| Method | Description |
|--------|-------------|
| `UseXResultMiddleware()` | Adds exception handling middleware to pipeline |

### IEndpointConventionBuilder Extensions

| Method | Description |
|--------|-------------|
| `WithXResultSupport()` | Adds both validation and result filters |
| `WithXResultFilter()` | Adds result-to-response conversion filter |
| `WithXResultValidation()` | Adds automatic validation filter |

### Result Extensions

| Method | Description |
|--------|-------------|
| `ToActionResult()` | Converts Result to `IActionResult` |
| `ToProblemDetails(context)` | Converts Result to `ProblemDetails` |
| `ToModelStateDictionary()` | Converts Result errors to `ModelStateDictionary` |

### ModelStateDictionary Extensions

| Method | Description |
|--------|-------------|
| `ToResult(statusCode)` | Converts ModelState errors to `Result` |

### BadHttpRequestException Extensions

| Method | Description |
|--------|-------------|
| `ToResult()` | Converts exception to `Result` with error details |

---

## ✅ Best Practices

### ✅ Do

- **Use `WithXResultSupport()`** for endpoints that need both validation and result conversion
- **Register `AddXResultMiddleware()`** for global exception handling
- **Return `Result<T>` from services** and let filters handle HTTP conversion
- **Implement `IRequiresValidation`** on request types for automatic validation
- **Use `AddXControllerResultMvcOptions()`** for MVC controller support

### ❌ Don't

- **Mix manual and automatic Result handling** — choose one approach per endpoint
- **Catch exceptions in handlers** when using `ResultMiddleware` — let middleware handle them
- **Return raw exceptions** — wrap them in `Result.Failure()` or let middleware convert them
- **Forget to register `IResultHeaderWriter`** — required for endpoint filters

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Results** | Core `Result` and `Result<T>` types |
| **System.Primitives.Validation** | `IRuleValidator` and validation infrastructure |
| **AspNetCore.Net** | Minimal API infrastructure and endpoint routing |
| **AspNetCore.Collections.AsyncPaged** | Async paged enumerable HTTP streaming |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

