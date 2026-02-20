# 🌐 AspNetCore.Net

[![NuGet Version](https://img.shields.io/nuget/v/Xpandables.AspNetCore.Net.svg)](https://www.nuget.org/packages/Xpandables.AspNetCore.Net)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AspNetCore.Net.svg)](https://www.nuget.org/packages/Xpandables.AspNetCore.Net)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> Minimal API routing, metadata, and JSON helpers for organized ASP.NET Core applications.

---

## 📋 Overview

`AspNetCore.Net` keeps minimal APIs tidy by giving you a small set of primitives: modular endpoint modules, a route builder that applies cross-cutting conventions, consistent OpenAPI metadata helpers, and easy access to the configured `JsonSerializerOptions`. Everything is designed for .NET 10, trimming, and AOT-friendly builds.

### ✨ Key Features

- 🛣️ **Modular endpoint modules** — Implement sealed `IMinimalEndpointRoute` classes and register them with `AddXMinimalEndpointRoutes(...)` for automatic discovery.
- ⚙️ **Cross-cutting configuration** — `AddXMinimalSupport` exposes `MinimalSupportOptions` so you can attach conventions (authorization, OpenAPI, CORS, etc.) to endpoints, optionally filtered by a predicate.
- 🧭 **Routing helpers** — `MinimalRouteBuilder` wraps `IEndpointRouteBuilder` with `MapGet/MapPost/MapPut/MapDelete/MapPatch/MapMethods/MapGroup` that automatically apply your configured filters.
- 🎯 **Metadata helpers** — `Accepts<T>()` and `ProducesXXX()` helpers standardize status codes and content types for consistent OpenAPI docs.
- 📝 **JSON helpers** — `AddXJsonSerializerOptions()` registers the configured `JsonSerializerOptions` as a singleton; `HttpContext` helpers surface serializer options, content type, and encoding per request.

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

// Configure how endpoints are decorated
builder.Services.AddXMinimalSupport(options =>
{
    // Apply conventions only to /api endpoints
    options.EndpointPredicate = endpoint =>
        endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;

    // Attach cross-cutting conventions
    options.ConfigureEndpoint = endpoint =>
    {
        endpoint.RequireAuthorization();
        endpoint.WithOpenApi();
    };
});

// Auto-discover sealed IMinimalEndpointRoute modules in the entry assembly
builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);

// Expose the configured JsonSerializerOptions for DI consumers
builder.Services.AddXJsonSerializerOptions();

var app = builder.Build();

app.UseHttpsRedirection();

// Wire up all discovered minimal endpoint routes
app.UseXMinimalEndpointRoutes();

app.Run();
```

---

## 🛣️ Modular Endpoint Routing

### Define Endpoint Routes

Create sealed modules that implement `IMinimalEndpointRoute`. Each module receives a `MinimalRouteBuilder`, so every mapped endpoint automatically inherits your configured filters and metadata helpers.

```csharp
using Microsoft.AspNetCore.Routing;

public sealed class OrdersEndpoints : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", GetOrders)
            .Produces200OK<IEnumerable<OrderSummary>>()
            .Produces500InternalServerError();

        group.MapGet("/{id:guid}", GetOrderById)
            .Produces200OK<OrderDetails>()
            .Produces404NotFound();

        group.MapPost("/", CreateOrder)
            .Accepts<CreateOrderRequest>()
            .Produces201Created<OrderDetails>()
            .Produces400BadRequest();
    }

    private static async Task<IResult> GetOrders(IOrdersService service, CancellationToken ct)
    {
        var orders = await service.ListAsync(ct);
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrderById(Guid id, IOrdersService service, CancellationToken ct)
    {
        var order = await service.GetAsync(id, ct);
        return order is null ? Results.NotFound() : Results.Ok(order);
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request,
        IOrdersService service,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return Results.Created($"/api/orders/{created.Id}", created);
    }
}
```

### Register and Use

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddXMinimalSupport();
builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);

var app = builder.Build();

app.UseXMinimalEndpointRoutes();

app.Run();
```

---

## 🎯 Route Metadata Extensions

Fluent helpers add OpenAPI-friendly metadata without repeating status codes or content types.

```csharp
public void AddRoutes(MinimalRouteBuilder app)
{
    app.MapGet("/api/users", GetUsers)
        .Produces200OK<IEnumerable<User>>()
        .Produces500InternalServerError();

    app.MapPost("/api/users", CreateUser)
        .Accepts<CreateUserRequest>()
        .Produces201Created<User>()
        .Produces400BadRequest();

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

Control which endpoints receive cross-cutting conventions and how they are configured.

```csharp
builder.Services.AddXMinimalSupport(options =>
{
    // Only apply conventions to API endpoints
    options.EndpointPredicate = endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;

    // Add conventions to matching endpoints
    options.ConfigureEndpoint = builder =>
    {
        builder.RequireAuthorization();
        builder.WithOpenApi();
    };
});
```

**Options**

| Property | Type | Description |
|----------|------|-------------|
| `EndpointPredicate` | `Func<RouteEndpoint, bool>?` | Filter which endpoints receive configuration. |
| `ConfigureEndpoint` | `Action<IEndpointConventionBuilder>?` | Apply conventions/metadata to matching endpoints. |

---

## 📝 JSON Serializer Options

Expose your app's configured `JsonSerializerOptions` to any service via DI.

```csharp
// Program.cs
builder.Services.AddXJsonSerializerOptions();

// Service using the shared options
public sealed class ExportService(JsonSerializerOptions jsonOptions)
{
    public string ToJson<T>(T value) => JsonSerializer.Serialize(value, jsonOptions);
}
```

### HttpContext helpers

Retrieve request-scoped serialization settings and content negotiation details.

```csharp
app.MapGet("/api/profile", (HttpContext ctx) =>
{
    var options = ctx.GetJsonSerializerOptionsOrDefault();
    var contentType = ctx.GetContentType("application/json");
    var encoding = ctx.GetEncoding();

    return Results.Ok(new
    {
        SerializerIndented = options.WriteIndented,
        ContentType = contentType,
        Encoding = encoding.WebName
    });
});
```

Helpers include `GetJsonSerializerOptions()`, `GetMvcJsonSerializerOptions()`, `GetJsonSerializerOptionsOrDefault()`, `GetMvcJsonSerializerOptionsOrDefault()`, `GetContentType()`, `GetContentType(string defaultContentType)`, and `GetEncoding()`.

---

## 🏗️ Complete Example

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Apply OpenAPI + auth only to API endpoints
builder.Services.AddXMinimalSupport(options =>
{
    options.EndpointPredicate = endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api") ?? false;
    options.ConfigureEndpoint = b =>
    {
        b.RequireAuthorization();
        b.WithOpenApi();
    };
});

builder.Services.AddXMinimalEndpointRoutes(typeof(Program).Assembly);
builder.Services.AddXJsonSerializerOptions();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .Produces200OK()
    .WithTags("Health");

app.UseXMinimalEndpointRoutes();

app.Run();

// Example endpoint module
public sealed class CustomersEndpoints : IMinimalEndpointRoute
{
    public void AddRoutes(MinimalRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapGet("/", GetCustomers)
            .Produces200OK<IEnumerable<CustomerSummary>>();

        group.MapGet("/{id:guid}", GetCustomer)
            .Produces200OK<CustomerDetails>()
            .Produces404NotFound();
    }

    private static IResult GetCustomers(ICustomerStore store) =>
        Results.Ok(store.List());

    private static IResult GetCustomer(Guid id, ICustomerStore store) =>
        store.TryGet(id, out var customer)
            ? Results.Ok(customer)
            : Results.NotFound();
}
```

---

## ✅ Best Practices

- Use sealed `IMinimalEndpointRoute` modules to keep endpoints cohesive and discoverable.
- Apply `AddXMinimalSupport` to centralize conventions (auth, OpenAPI, CORS) instead of repeating per endpoint.
- Use `Accepts<T>()` and `ProducesXXX()` helpers to keep OpenAPI metadata consistent.
- Call `AddXJsonSerializerOptions()` once so services share the same serializer settings configured by ASP.NET Core.
- Prefer `MapGroup` to scope tags and prefixes for related endpoints.

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

