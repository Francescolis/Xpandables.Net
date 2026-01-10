# 🔌 AspNetCore.Composition

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/AspNetCore.Composition)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> MEF-driven middleware composition and DI helpers for ASP.NET Core apps.

---

## 📋 Overview

`AspNetCore.Composition` adds two focused capabilities to ASP.NET Core:

- **Lazy DI**: register `Lazy<T>` once with `AddXLazyResolved()` so services can defer expensive or optional dependencies.
- **MEF/assembly composition**: load middleware/configuration exported from plugin assemblies via `UseXServiceExports()` or apply `IUseService` implementations from known assemblies with `UseXServices(...)`.

Everything is built for .NET 10, trimming, and native AOT friendliness.

### ✨ Key Features

- ⏳ **Lazy<T> DI** — `AddXLazyResolved()` registers `Lazy<>` to resolve from the container on first access.
- 🧩 **MEF exports** — `UseXServiceExports()` discovers `IUseServiceExport` types in plugin folders using `ExportOptions` (path, search pattern, recurse).
- 🧭 **Assembly-based wiring** — `UseXServices(params Assembly[])` instantiates and runs all `IUseService` implementations for simple module composition.

---

## 📦 Installation

```bash
dotnet add package AspNetCore.Composition
```

Or via NuGet Package Manager:

```powershell
Install-Package AspNetCore.Composition
```

---

## 🚀 Quick Start

### Register Lazy<T>

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Enable Lazy<T> injection across the app
builder.Services.AddXLazyResolved();

// Example service consuming lazy dependencies
public sealed class InvoiceProcessor(
    Lazy<IPaymentGateway> payment,
    Lazy<IEmailSender> email)
{
    public async Task ProcessAsync(Invoice invoice, CancellationToken ct)
    {
        if (invoice.RequiresPayment)
        {
            await payment.Value.ChargeAsync(invoice.Total, ct);
        }

        if (invoice.SendReceipt)
        {
            await email.Value.SendReceiptAsync(invoice, ct);
        }
    }
}
```

### Apply MEF Exports (plugins)

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Load middleware exports from the current directory (default) or a plugins folder
app.UseXServiceExports(options =>
{
    options.Path = Path.Combine(AppContext.BaseDirectory, "plugins");
    options.SearchPattern = "*.Plugin.dll";
    options.SearchSubDirectories = true;
});

app.Run();

// In a plugin assembly
[Export(typeof(IUseServiceExport))]
public sealed class LoggingExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
    }
}
```

### Apply Assembly Services (no MEF)

```csharp
var app = builder.Build();

// Scan known assemblies for IUseService implementations and run them
app.UseXServices(typeof(Program).Assembly, typeof(MyFeatureAssembly).Assembly);

app.Run();

// Example in a referenced assembly
public sealed class CorsSetup : IUseService
{
    public void UseServices(WebApplication app)
    {
        app.UseCors("default");
    }
}
```

---

## 🔌 MEF Service Exports

`UseXServiceExports` scans assemblies in a directory using `ExportOptions` and executes each `IUseServiceExport` implementation.

**ExportOptions**

| Property | Type | Description |
|----------|------|-------------|
| `Path` | `string` | Base directory to scan (defaults to entry assembly directory). |
| `SearchPattern` | `string` | File pattern to load (defaults to `*.dll`). |
| `SearchSubDirectories` | `bool` | Recurse into subdirectories when true. |

**Example: multi-tenant plugin folder**

```csharp
app.UseXServiceExports(options =>
{
    options.Path = "/var/app/tenants/tenant-a/plugins";
    options.SearchPattern = "TenantA.*.dll";
});
```

Each discovered export can wire middleware, endpoints, or other pipeline concerns.

---

## ⏳ Lazy Service Resolution

`AddXLazyResolved()` registers `Lazy<>` so instances resolve from DI on first use—helpful for expensive or optional dependencies and for breaking dependency cycles.

```csharp
builder.Services.AddXLazyResolved();

public sealed class AuditWriter(Lazy<IAuditSink> sink)
{
    public Task WriteAsync(AuditEntry entry, CancellationToken ct) =>
        sink.Value.WriteAsync(entry, ct);
}
```

---

## 🏗️ Complete Example

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Lazy<T> support for deferred dependencies
builder.Services.AddXLazyResolved();

var app = builder.Build();

app.UseHttpsRedirection();

// Load all plugin exports from /extensions
app.UseXServiceExports(options =>
{
    options.Path = Path.Combine(AppContext.BaseDirectory, "extensions");
    options.SearchPattern = "*.dll";
    options.SearchSubDirectories = false;
});

// Apply in-assembly middleware modules
app.UseXServices(typeof(Program).Assembly);

app.Run();

// In a separate module assembly
public sealed class ObservabilityModule : IUseService
{
    public void UseServices(WebApplication app)
    {
        app.UseMiddleware<RequestTimingMiddleware>();
        app.UseMiddleware<TracingMiddleware>();
    }
}
```

---

## ✅ Best Practices

- Keep `IUseServiceExport` implementations small and focused (logging, auth, multi-tenant customizations).
- Use `SearchPattern` to avoid loading unintended assemblies from plugin folders.
- Prefer `IUseService` for first-party modules you reference directly; use MEF exports for optional/external plugins.
- Add `AddXLazyResolved()` once at startup so all services can opt into deferred resolution.

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **AspNetCore.Net** | Minimal API routing and metadata helpers |
| **System.Composition** | MEF composition utilities backing the export pipeline |
| **AspNetCore.Results** | Result pattern integration for HTTP responses |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).
## 🌐 AspNetCore.Net

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

```

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

