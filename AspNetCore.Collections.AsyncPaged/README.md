# 🌐 AspNetCore.Net

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Integration** - Result to HTTP response mapping, minimal API filters, modular endpoint routing, MVC integration, and lazy service resolution.

---

## 📋 Overview

`AspNetCore.Net` provides comprehensive ASP.NET Core integration for the Result pattern, including automatic result-to-HTTP-response conversion, validation filters, modular endpoint routing via `IEndpointRoute`, MVC controller filters, and utility extensions for dependency injection.

### ✨ Key Features

- 🔄 **Result Extensions** - Convert Result to IActionResult, ProblemDetails, and ModelStateDictionary
- 🛣️ **IEndpointRoute** - Interface for modular minimal API endpoint registration
- 🔍 **Validation Filters** - Automatic request validation for minimal APIs and MVC
- ⚡ **Result Filters** - Transform Result responses to proper HTTP results
- ⏳ **Lazy Resolution** - Lazy<T> dependency injection support
- 📝 **JSON Configuration** - Easy JsonSerializerOptions service registration
- 🎯 **Route Metadata** - Fluent API for endpoint metadata (Produces200OK, Produces400BadRequest, etc.)
- 🏗️ **MVC Support** - Controller result filters and formatters

---

## 📥 Installation

```bash
dotnet add package AspNetCore.Net
```

---

## 🚀 Quick Start

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register Result support middleware
builder.Services.AddXResultSupport();

// Register JSON serializer options from ASP.NET Core configuration
builder.Services.AddXJsonSerializerOptions();

// Register lazy resolution support
builder.Services.AddXLazyResolved();

// Register endpoint routes from assemblies
builder.Services.AddXEndpointRoutes(typeof(Program).Assembly);
```

### Configure Pipeline

```csharp
var app = builder.Build();

// Use Result support middleware (handles exceptions, converts Results)
app.UseXResultSupport(options =>
{
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
});

// Use registered endpoint routes
app.UseXEndpointRoutes();

app.Run();
```

---

## 🛣️ Modular Endpoint Routing

### Define Endpoint Routes

```csharp
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

public sealed class UserEndpoints : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", GetAllUsers)
            .Produces200OK<IEnumerable<User>>()
            .Produces500InternalServerError();

        group.MapGet("/{id:guid}", GetUserById)
            .Produces200OK<User>()
            .Produces404NotFound();

        group.MapPost("/", CreateUser)
            .Accepts<CreateUserRequest>()
            .Produces201Created<User>()
            .Produces400BadRequest()
            .WithXMinimalApi(); // Adds validation + result filters
    }

    private static async Task<IResult> GetAllUsers(
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return result.ToMinimalResult(); // Converts Result to IResult
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(request, cancellationToken);
        return result.ToMinimalResult();
    }
}
```

### Register and Use

```csharp
// In Program.cs
builder.Services.AddXEndpointRoutes(typeof(Program).Assembly);

var app = builder.Build();
app.UseXEndpointRoutes(); // Automatically calls AddRoutes on all IEndpointRoute implementations
```

---

## 🔄 Result Extensions

### Convert Result to IActionResult (MVC)

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Results;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        Result<User> result = await _userService.GetUserAsync(id);
        
        // Converts Result to ObjectResult with correct status code
        return result.ToActionResult();
    }
}
```

### Convert to ProblemDetails

```csharp
using Microsoft.AspNetCore.Http;
using System.Results;

app.UseExceptionHandler(exceptionHandler =>
{
    exceptionHandler.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        
        if (exceptionFeature?.Error is BadHttpRequestException badRequest)
        {
            Result result = badRequest.ToResult();
            ProblemDetails problem = result.ToProblemDetails(context);
            
            context.Response.StatusCode = problem.Status ?? 400;
            await context.Response.WriteAsJsonAsync(problem);
        }
    });
});
```

### Convert ModelState to Result

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Results;

[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    if (!ModelState.IsValid)
    {
        // Convert ModelState errors to Result
        Result validationResult = ModelState.ToResult();
        return validationResult.ToActionResult();
    }

    var result = await _userService.CreateUserAsync(request);
    return result.ToActionResult();
}
```

### Convert Result to ModelStateDictionary

```csharp
Result result = await _service.ValidateAsync(request);

if (!result.IsSuccess)
{
    // Convert Result errors back to ModelState for view rendering
    ModelStateDictionary modelState = result.ToModelStateDictionary();
    return View(request);
}
```

---

## 🔍 Minimal API Filters

### Apply Filters to Endpoints

```csharp
// Apply both validation and result filters
app.MapPost("/api/orders", CreateOrder)
    .WithXMinimalApi();

// Apply only validation filter
app.MapPost("/api/products", CreateProduct)
    .WithXMinimalValidation();

// Apply only result filter
app.MapGet("/api/items", GetItems)
    .WithXMinimalFilter();
```

### Configure Filters Globally

```csharp
app.UseXResultSupport(options =>
{
    options.EnableValidationFilter = true;
    options.EnableResultFilter = true;
    
    // Custom endpoint predicate
    options.EndpointPredicate = endpoint => 
        endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;
    
    // Custom endpoint configuration
    options.ConfigureEndpoint = (builder, endpoint) =>
    {
        if (endpoint.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("POST")))
        {
            builder.WithXMinimalValidation();
            builder.WithXMinimalFilter();
        }
    };
});
```

---

## 🎯 Route Metadata Extensions

Fluent API for adding OpenAPI metadata to endpoints:

```csharp
app.MapGet("/api/users", GetUsers)
    .Produces200OK<IEnumerable<User>>()
    .Produces500InternalServerError();

app.MapGet("/api/users/{id}", GetUserById)
    .Produces200OK<User>()
    .Produces404NotFound();

app.MapPost("/api/users", CreateUser)
    .Accepts<CreateUserRequest>()
    .Produces201Created<User>()
    .Produces400BadRequest();

app.MapPut("/api/users/{id}", UpdateUser)
    .Produces200OK<User>()
    .Produces404NotFound()
    .Produces409Conflict();

app.MapDelete("/api/users/{id}", DeleteUser)
    .Produces200OK()
    .Produces404NotFound()
    .Produces401Unauthorized();
```

---

## ⏳ Lazy Service Resolution

```csharp
// Register lazy support
builder.Services.AddXLazyResolved();

// Use in services - services are only resolved when accessed
public class OrderService
{
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IPaymentService> _paymentService;

    public OrderService(
        Lazy<IEmailService> emailService,
        Lazy<IPaymentService> paymentService)
    {
        _emailService = emailService;
        _paymentService = paymentService;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Services resolved only when .Value is accessed
        if (order.RequiresPayment)
        {
            await _paymentService.Value.ProcessAsync(order);
        }

        await _emailService.Value.SendConfirmationAsync(order);
    }
}
```

---

## 📝 JSON Serializer Options

```csharp
// Register JsonSerializerOptions from ASP.NET Core configuration
builder.Services.AddXJsonSerializerOptions();

// Use in services
public class DataService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public DataService(JsonSerializerOptions jsonOptions)
        => _jsonOptions = jsonOptions;

    public string Serialize<T>(T data) =>
        JsonSerializer.Serialize(data, _jsonOptions);
}
```

---

## 🏗️ MVC Controller Support

### Result Filter Attribute

```csharp
using AspNetCore.Net.Mvc.Filters;

[ApiController]
[Route("api/[controller]")]
[ControllerResultFilter] // Automatically converts Result returns to proper responses
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<Result<IEnumerable<Product>>> GetProducts()
    {
        // Return Result directly - filter converts to IActionResult
        return await _productService.GetAllAsync();
    }

    [HttpPost]
    [ControllerResultValidationFilter] // Validates request before execution
    public async Task<Result<Product>> CreateProduct(CreateProductRequest request)
    {
        return await _productService.CreateAsync(request);
    }
}
```

---

## ✅ Best Practices

1. **Use IEndpointRoute** - Organize minimal API endpoints into separate classes
2. **Apply WithXMinimalApi()** - Enable both validation and result filters
3. **Use ToActionResult()** - Consistently convert Results to HTTP responses
4. **Use Lazy<T>** - For optional or expensive dependencies
5. **Register JsonSerializerOptions** - Ensure consistent JSON serialization
6. **Add route metadata** - Use Produces* extensions for OpenAPI documentation

---

## 📚 Related Packages

- **System.Results** - Core Result types and request/handler pattern
- **System.Primitives.Validation** - Validation and specification pattern
- **System.Results.Pipelines** - Pipeline decorators

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
