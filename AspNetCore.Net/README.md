# ?? AspNetCore.Net

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Integration** - OperationResult to HTTP response mapping, endpoint routing, lazy service resolution, and JSON serialization configuration.

---

## ?? Overview

`AspNetCore.Net` provides core ASP.NET Core integration for OperationResult types, endpoint routing patterns, and service resolution utilities. It bridges domain-driven design patterns with ASP.NET Core's HTTP pipeline.

### ? Key Features

- ?? **OperationResult Extensions** - Convert OperationResult to IActionResult, ProblemDetails, and ModelStateDictionary
- ??? **IEndpointRoute** - Interface for modular endpoint route registration
- ? **Lazy Resolution** - Lazy<T> dependency injection support
- ?? **JSON Configuration** - Easy JsonSerializerOptions service registration
- ?? **ModelState Integration** - Seamless ModelStateDictionary to OperationResult conversion
- ?? **Exception Handling** - BadHttpRequestException to OperationResult conversion

---

## ?? Quick Start

### Installation

```bash
dotnet add package AspNetCore.Net
```

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register JSON serializer options
builder.Services.AddXJsonSerializerOptions();

// Register lazy resolution support
builder.Services.AddXLazyResolved();
```

---

## ?? Core Features

### OperationResult to HTTP Response

```csharp
using System.ExecutionResults;
using AspNetCore.Net;
using Microsoft.AspNetCore.Mvc;

public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        OperationResult<User> result = await _userService.GetUserAsync(id);
        
        // Convert to IActionResult
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            // Convert ModelState to OperationResult
            OperationResult validationResult = ModelState.ToExecutionResult();
            return validationResult.ToActionResult();
        }
        
        OperationResult<User> result = await _userService.CreateUserAsync(request);
        return result.ToActionResult();
    }
}
```

### ProblemDetails Generation

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

app.UseExceptionHandler(exceptionHandler =>
{
    exceptionHandler.Run(async context =>
    {
        var exceptionFeature = context.Features
            .Get<IExceptionHandlerFeature>();
        
        if (exceptionFeature?.Error is BadHttpRequestException badRequest)
        {
            OperationResult result = badRequest.ToExecutionResult();
            ProblemDetails problem = result.ToProblemDetails(context);
            
            context.Response.StatusCode = problem.Status ?? 400;
            await context.Response.WriteAsJsonAsync(problem);
        }
    });
});
```

### Modular Endpoint Routing

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

// Implement endpoint route
public class UserEndpoints : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");
        
        group.MapGet("/", GetAllUsers);
        group.MapGet("/{id}", GetUserById);
        group.MapPost("/", CreateUser);
        group.MapPut("/{id}", UpdateUser);
        group.MapDelete("/{id}", DeleteUser);
    }
    
    private async Task<IResult> GetAllUsers(
        [FromServices] IUserService service)
    {
        var result = await service.GetAllUsersAsync();
        return Results.Ok(result);
    }
    
    // Other handlers...
}

// Register and use in Program.cs
app.Services.AddSingleton<IEndpointRoute, UserEndpoints>();

var endpoints = app.Services.GetServices<IEndpointRoute>();
foreach (var endpoint in endpoints)
{
    endpoint.AddRoutes(app);
}
```

### Lazy Service Resolution

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register lazy support
builder.Services.AddXLazyResolved();

// Use in services
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
        // Services are only resolved when accessed
        if (order.RequiresPayment)
        {
            await _paymentService.Value.ProcessAsync(order);
        }
        
        await _emailService.Value.SendConfirmationAsync(order);
    }
}
```

### JSON Serializer Options

```csharp
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

// Register JsonSerializerOptions from ASP.NET Core configuration
builder.Services.AddXJsonSerializerOptions();

// Use in services
public class DataService
{
    private readonly JsonSerializerOptions _jsonOptions;
    
    public DataService(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }
    
    public string SerializeData<T>(T data)
    {
        return JsonSerializer.Serialize(data, _jsonOptions);
    }
}
```

---

## ?? Advanced Scenarios

### ModelState Validation

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        // Custom validation
        if (request.Price < 0)
        {
            ModelState.AddModelError(nameof(request.Price), "Price must be positive");
        }
        
        if (!ModelState.IsValid)
        {
            // Convert to OperationResult with BadRequest status
            OperationResult error = ModelState.ToExecutionResult();
            return error.ToActionResult();
        }
        
        var result = await _productService.CreateAsync(request);
        return result.ToActionResult();
    }
}
```

### Exception to OperationResult

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Http;

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (BadHttpRequestException ex)
    {
        OperationResult result = ex.ToExecutionResult();
        ProblemDetails problem = result.ToProblemDetails(context);
        
        context.Response.StatusCode = (int)result.StatusCode;
        await context.Response.WriteAsJsonAsync(problem);
    }
});
```

---

## ? Best Practices

1. **Use ToActionResult()** - Consistently convert OperationResult to HTTP responses
2. **Leverage IEndpointRoute** - Organize endpoints into separate classes
3. **Use Lazy<T>** - For optional or expensive dependencies
4. **Register JsonSerializerOptions** - Ensure consistent JSON serialization
5. **Convert exceptions early** - Transform BadHttpRequestException to OperationResult

---

## ?? Related Packages

- **System.ExecutionResults** - Core OperationResult types
- **AspNetCore.Net.Minimals** - Minimal API integration
- **AspNetCore.Net.Controllers** - Controller-specific extensions

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
