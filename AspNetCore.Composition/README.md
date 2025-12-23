# 🌐 AspNetCore.Net

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/AspNetCore.Net)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **ASP.NET Core Minimal API Infrastructure** — Modular endpoint routing, lazy service resolution, JSON configuration, and MEF-based service exports for building clean, organized minimal APIs.

---

## 📋 Overview

`AspNetCore.Net` provides infrastructure components for building well-organized ASP.NET Core minimal APIs. The library offers modular endpoint routing via `IMinimalEndpointRoute`, lazy dependency injection support, JSON serializer configuration helpers, and MEF (Managed Extensibility Framework) integration for plugin-based service registration.

Built for .NET 10 with C# 14 extension members, this package promotes separation of concerns and clean architecture in minimal API applications.

### ✨ Key Features

- 🛣️ **`IMinimalEndpointRoute`** — Interface for modular minimal API endpoint registration with service and middleware hooks
- 🔧 **`MinimalRouteBuilder`** — Wrapper around `IEndpointRouteBuilder` with automatic filter application
- ⏳ **Lazy Resolution** — `Lazy<T>` dependency injection support via `AddXLazyResolved()`
- 📝 **JSON Configuration** — Register `JsonSerializerOptions` as a singleton with `AddXJsonSerializerOptions()`
- 🎯 **Route Metadata** — Fluent API for endpoint metadata (`Produces200OK`, `Produces400BadRequest`, etc.)
- 🔌 **MEF Integration** — `IUseServiceExport` for plugin-based middleware configuration
- ⚙️ **Minimal Support Options** — Conditional endpoint filtering and configuration

---

## 📦 Installation

```bash
dotnet add package AspNetCore.Net
```

Or via NuGet Package Manager:

```powershell
Install-Package AspNetCore.Net
```

---

## 🚀 Quick Start

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register minimal support with optional configuration
builder.Services.AddXMinimalSupport(options =>
{
    // Optional: Configure endpoint predicate for conditional filter application
    options.EndpointPredicate = endpoint => 
        endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;
    
    // Optional: Configure all endpoints
    options.ConfigureEndpoint = builder => 
        builder.RequireAuthorization();
});

// Register endpoint routes from assemblies
builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);

// Register JSON serializer options as singleton
builder.Services.AddXJsonSerializerOptions();

// Register lazy resolution support
builder.Services.AddXLazyResolved();

var app = builder.Build();

// Use registered endpoint routes
app.UseXMinimalEndpointRoutes();

app.Run();
```

---

## 🛣️ Modular Endpoint Routing

### Define Endpoint Routes

The `IMinimalEndpointRoute` interface provides a clean way to organize endpoints with optional service registration and middleware configuration:

```csharp
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public sealed class ProductEndpoints : IMinimalEndpointRoute
{
    // Register services specific to this endpoint module
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
    }

    // Configure middleware specific to this module
    public void UseServices(WebApplication application)
    {
        // Add any middleware needed for this module
    }

    // Define the endpoints
    public void AddRoutes(MinimalRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetAllProducts)
            .Produces200OK<IEnumerable<Product>>()
            .Produces500InternalServerError();

        group.MapGet("/{id:guid}", GetProductById)
            .Produces200OK<Product>()
            .Produces404NotFound();

        group.MapPost("/", CreateProduct)
            .Accepts<CreateProductRequest>()
            .Produces201Created<Product>()
            .Produces400BadRequest();

        group.MapPut("/{id:guid}", UpdateProduct)
            .Accepts<UpdateProductRequest>()
            .Produces200OK<Product>()
            .Produces404NotFound()
            .Produces409Conflict();

        group.MapDelete("/{id:guid}", DeleteProduct)
            .Produces200OK()
            .Produces404NotFound()
            .Produces401Unauthorized();
    }

    private static async Task<IResult> GetAllProducts(
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is not null
            ? Results.Ok(product)
            : Results.NotFound();
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/products/{product.Id}", product);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request, cancellationToken);
        return product is not null
            ? Results.Ok(product)
            : Results.NotFound();
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        IProductService productService,
        CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);
        return deleted ? Results.Ok() : Results.NotFound();
    }
}
```

### Register and Use

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register minimal support options
builder.Services.AddXMinimalSupport();

// Discover and register all IMinimalEndpointRoute implementations
builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);

var app = builder.Build();

// Apply all registered endpoint routes
app.UseXMinimalEndpointRoutes();

app.Run();
```

---

## 🎯 Route Metadata Extensions

Fluent API for adding OpenAPI metadata to endpoints:

```csharp
public void AddRoutes(MinimalRouteBuilder app)
{
    // GET endpoint with success and error responses
    app.MapGet("/api/users", GetUsers)
        .Produces200OK<IEnumerable<User>>()
        .Produces500InternalServerError();

    // GET with path parameter
    app.MapGet("/api/users/{id}", GetUserById)
        .Produces200OK<User>()
        .Produces404NotFound();

    // POST with request body
    app.MapPost("/api/users", CreateUser)
        .Accepts<CreateUserRequest>()
        .Produces201Created<User>()
        .Produces400BadRequest();

    // PUT with conflict handling
    app.MapPut("/api/users/{id}", UpdateUser)
        .Accepts<UpdateUserRequest>()
        .Produces200OK<User>()
        .Produces404NotFound()
        .Produces409Conflict();

    // DELETE with authorization
    app.MapDelete("/api/users/{id}", DeleteUser)
        .Produces200OK()
        .Produces404NotFound()
        .Produces401Unauthorized();

    // Custom HTTP methods
    app.MapMethods("/api/users/{id}/activate", ["PATCH"], ActivateUser)
        .Produces200OK()
        .Produces405MethodNotAllowed();
}
```

### Available Metadata Extensions

| Extension | Status Code | Content Type |
|-----------|-------------|--------------|
| `Produces200OK()` | 200 | `application/json` |
| `Produces200OK<T>()` | 200 | `application/json` |
| `Produces201Created<T>()` | 201 | `application/json` |
| `Produces400BadRequest()` | 400 | `application/problem+json` |
| `Produces401Unauthorized()` | 401 | `application/problem+json` |
| `Produces404NotFound()` | 404 | `application/problem+json` |
| `Produces405MethodNotAllowed()` | 405 | `application/problem+json` |
| `Produces409Conflict()` | 409 | `application/problem+json` |
| `Produces500InternalServerError()` | 500 | `application/problem+json` |
| `Accepts<T>()` | — | `application/json` |

---

## ⚙️ Minimal Support Options

Configure how filters and conventions are applied to endpoints:

```csharp
builder.Services.AddXMinimalSupport(options =>
{
    // Apply configuration only to endpoints matching this predicate
    options.EndpointPredicate = endpoint => 
        endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;
    
    // Configure matching endpoints
    options.ConfigureEndpoint = builder =>
    {
        builder.RequireAuthorization();
        builder.WithOpenApi();
    };
});
```

### Options Properties

| Property | Type | Description |
|----------|------|-------------|
| `EndpointPredicate` | `Func<RouteEndpoint, bool>?` | Predicate to filter which endpoints receive configuration |
| `ConfigureEndpoint` | `Action<IEndpointConventionBuilder>?` | Action to configure matching endpoints |

---

## ⏳ Lazy Service Resolution

Defer service resolution until first access with `Lazy<T>` support:

```csharp
// Register lazy support
builder.Services.AddXLazyResolved();

// Use in services - services are only resolved when accessed
public class OrderProcessor(
    Lazy<IEmailService> emailService,
    Lazy<IPaymentGateway> paymentGateway,
    Lazy<IInventoryService> inventoryService)
{
    public async Task ProcessOrderAsync(Order order, CancellationToken ct)
    {
        // Payment gateway resolved only when needed
        if (order.RequiresPayment)
        {
            await paymentGateway.Value.ChargeAsync(order.Total, ct);
        }

        // Inventory service resolved only when needed
        if (order.HasPhysicalItems)
        {
            await inventoryService.Value.ReserveItemsAsync(order.Items, ct);
        }

        // Email service resolved only when sending confirmation
        await emailService.Value.SendOrderConfirmationAsync(order, ct);
    }
}
```

### Benefits

- **Reduced startup time** — Services not resolved until needed
- **Conditional dependencies** — Only resolve services that are actually used
- **Circular dependency avoidance** — Break circular dependency chains

---

## 📝 JSON Serializer Options

Register ASP.NET Core's configured `JsonSerializerOptions` as a singleton:

```csharp
// In Program.cs
builder.Services.AddXJsonSerializerOptions();

// Use in services
public class DataExportService(JsonSerializerOptions jsonOptions)
{
    public string ExportToJson<T>(T data) =>
        JsonSerializer.Serialize(data, jsonOptions);

    public async Task ExportToFileAsync<T>(T data, string path, CancellationToken ct)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, data, jsonOptions, ct);
    }

    public T? ImportFromJson<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, jsonOptions);
}
```

This ensures consistent JSON serialization settings throughout your application by reusing the options configured in `JsonOptions`.

---

## 🔌 MEF Service Exports

Use MEF (Managed Extensibility Framework) for plugin-based service registration:

### Define a Service Export

```csharp
using System.ComponentModel.Composition;
using Microsoft.AspNetCore.Builder;

[Export(typeof(IUseServiceExport))]
public class LoggingMiddlewareExport : IUseServiceExport
{
    public void UseServices(WebApplication application)
    {
        // Configure middleware from an external plugin
        application.UseMiddleware<RequestLoggingMiddleware>();
        application.UseMiddleware<PerformanceLoggingMiddleware>();
    }
}
```

### Apply Service Exports

```csharp
var app = builder.Build();

// Apply all MEF-exported IUseServiceExport implementations
app.UseXServiceExports();

// Or with custom options
app.UseXServiceExports(options =>
{
    options.Directories = ["/plugins", "/extensions"];
    options.SearchPattern = "*.Plugin.dll";
});

app.Run();
```

### Assembly-Based Service Discovery

```csharp
// Discover and apply IUseService implementations from assemblies
app.UseXServices(typeof(Program).Assembly, typeof(PluginAssembly).Assembly);
```

---

## 🏗️ Complete Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddXMinimalSupport(options =>
{
    options.EndpointPredicate = endpoint => 
        endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;
});

builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);
builder.Services.AddXJsonSerializerOptions();
builder.Services.AddXLazyResolved();

// Add OpenAPI support
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure pipeline
app.UseHttpsRedirection();
app.MapOpenApi();

// Apply endpoint routes
app.UseXMinimalEndpointRoutes();

app.Run();

// Endpoint module
public sealed class HealthEndpoints : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .Produces200OK()
            .WithTags("Health");

        app.MapGet("/health/ready", async (IServiceProvider services) =>
        {
            // Check dependencies
            var dbContext = services.GetService<AppDbContext>();
            if (dbContext is null)
                return Results.Problem("Database not configured", statusCode: 503);

            return Results.Ok(new { Status = "Ready" });
        })
        .Produces200OK()
        .Produces500InternalServerError()
        .WithTags("Health");
    }
}
```

---

## ✅ Best Practices

### ✅ Do

- **Implement `IMinimalEndpointRoute`** — Organize endpoints into cohesive modules
- **Use `AddServices` override** — Register module-specific services within the endpoint class
- **Apply `AddXLazyResolved()`** — For optional or expensive dependencies
- **Use route metadata extensions** — Ensure consistent OpenAPI documentation
- **Configure `MinimalSupportOptions`** — Apply cross-cutting concerns consistently

### ❌ Don't

- **Register endpoints directly in Program.cs** — Use `IMinimalEndpointRoute` for organization
- **Resolve services eagerly** — Use `Lazy<T>` for conditional dependencies
- **Duplicate JSON options** — Use `AddXJsonSerializerOptions()` for consistency
- **Mix endpoint registration approaches** — Choose one pattern per application

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **AspNetCore.Results** | Result pattern integration for HTTP responses |
| **AspNetCore.AsyncPaged** | Async paged enumerable HTTP streaming |
| **System.Composition** | MEF composition utilities |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).

