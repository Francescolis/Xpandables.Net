# System.Results.Tasks

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Results.Tasks.svg)](https://www.nuget.org/packages/Xpandables.Results.Tasks)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Results.Tasks.svg)](https://www.nuget.org/packages/Xpandables.Results.Tasks)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Mediator implementation with pre-built pipeline request handler for CQRS dispatch.

## 📖 Overview

`System.Results.Tasks` (NuGet: **Xpandables.Results.Tasks**) provides `IMediator` / `Mediator` for dispatching requests through a decorator pipeline, and `PipelineRequestHandler<TRequest>` which builds the pipeline chain at construction time for maximum performance. Namespace: `System.Results.Tasks`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `IMediator` | `IMediator.cs` | Contract — `SendAsync<TRequest>` and `SendAsync<TRequest, TResponse>` |
| `Mediator` | `Mediator.cs` | Sealed implementation — resolves `IPipelineRequestHandler<TRequest>` from DI |
| `PipelineRequestHandler<TRequest>` | `PipelineRequestHandler.cs` | Sealed — pre-builds decorator chain at construction; fast path when no decorators |

### ⚙️ Dependency Injection

C# 14 extension members on `IServiceCollection`:

```csharp
// Basic mediator + pipeline handler
services.AddXMediator();

// Mediator + standard pipeline decorators (pre, post, validation, exception)
services.AddXMediatorWithPipelines();

// Mediator + event sourcing pipeline decorators
services.AddXMediatorWithEventSourcingPipelines();

// Custom mediator implementation
services.AddXMediator<MyCustomMediator>();
```

### 📋 Recommended Decorator Registration Order

**Standard pipeline:**
1. `PipelinePreHandlerDecorator`
2. `PipelinePostHandlerDecorator`
3. `PipelineRequireUnitOfWorkDecorator`
4. `PipelineValidationDecorator`
5. `PipelineExceptionDecorator`

**Event sourcing pipeline:**
1. `PipelinePublishDomainEventDecorator`
2. `PipelineEnqueueIntegrationEventDecorator`
3. `PipelinePreHandlerDecorator`
4. `PipelinePostHandlerDecorator`
5. `PipelineRequireDataUnitOfWorkDecorator`
6. `PipelineCommitDomainEventDecorator`
7. `PipelineValidationDecorator`
8. `PipelineExceptionDecorator`

## 📦 Installation

```bash
dotnet add package Xpandables.Results.Tasks
```

**Project References:** `Xpandables.Results.Pipelines` (which transitively includes Results, Validation, Events, Data, Entities)

## 🚀 Quick Start

### 1. Register Services at Startup

```csharp
using System.Results.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Scan and register all sealed request handlers from the assembly
builder.Services.AddXRequestHandlers(typeof(Program).Assembly);

// Option A: Mediator with standard pipeline (pre, post, validation, exception)
builder.Services.AddXMediatorWithPipelines();

// Option B: Mediator with event sourcing pipeline
// builder.Services.AddXMediatorWithEventSourcingPipelines();

// Option C: Bare mediator (no decorators — you add them yourself)
// builder.Services.AddXMediator();
```

### 2. Define Requests and Handlers

```csharp
// Query request — returns a single DTO
public sealed record GetProductByIdRequest(Guid ProductId) : IRequest<ProductDto>;

public sealed class GetProductByIdHandler(AppDbContext db)
    : IRequestHandler<GetProductByIdRequest, ProductDto>
{
    public async Task<Result<ProductDto>> HandleAsync(
        GetProductByIdRequest request, CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([request.ProductId], ct);

        if (product is null)
            return Result.NotFound<ProductDto>(
                "ProductId", $"Product {request.ProductId} not found");

        return Result.Success(new ProductDto(
            product.Id, product.Name, product.Price));
    }
}

// Command request — returns void-like Result
public sealed record DeactivateProductRequest(Guid ProductId) : IRequest;

public sealed class DeactivateProductHandler(AppDbContext db)
    : IRequestHandler<DeactivateProductRequest>
{
    public async Task<Result> HandleAsync(
        DeactivateProductRequest request, CancellationToken ct)
    {
        Product? product = await db.Products.FindAsync([request.ProductId], ct);

        if (product is null)
            return Result.NotFound("ProductId", "Product not found");

        product.IsActive = false;
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}
```

### 3. Send Requests via IMediator

```csharp
// Inject IMediator and dispatch requests
public sealed class ProductService(IMediator mediator)
{
    // Send a query — get a typed response
    public async Task<Result<ProductDto>> GetProductAsync(
        Guid productId, CancellationToken ct)
    {
        var request = new GetProductByIdRequest(productId);
        return await mediator.SendAsync<GetProductByIdRequest, ProductDto>(request, ct);
    }

    // Send a command — get a non-generic Result
    public async Task<Result> DeactivateProductAsync(
        Guid productId, CancellationToken ct)
    {
        var request = new DeactivateProductRequest(productId);
        return await mediator.SendAsync(request, ct);
    }
}
```

### 4. Use in a Minimal API Endpoint

```csharp
app.MapGet("/api/products/{id:guid}", async (
    Guid id, IMediator mediator, CancellationToken ct) =>
{
    Result<ProductDto> result = await mediator
        .SendAsync<GetProductByIdRequest, ProductDto>(
            new GetProductByIdRequest(id), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Problem(
            title: result.Title,
            detail: result.Detail,
            statusCode: (int)result.StatusCode);
});

app.MapDelete("/api/products/{id:guid}", async (
    Guid id, IMediator mediator, CancellationToken ct) =>
{
    Result result = await mediator
        .SendAsync(new DeactivateProductRequest(id), ct);

    return result.IsSuccess
        ? Results.NoContent()
        : Results.Problem(statusCode: (int)result.StatusCode);
});
```

### 5. Full Registration with Event Sourcing

```csharp
// For event-sourced applications
builder.Services.AddXRequestHandlers(typeof(Program).Assembly);
builder.Services.AddXMediatorWithEventSourcingPipelines();
// This registers in order:
//   1. PipelinePublishDomainEventDecorator
//   2. PipelineEnqueueIntegrationEventDecorator
//   3. PipelinePreHandlerDecorator
//   4. PipelinePostHandlerDecorator
//   5. PipelineRequireDataUnitOfWorkDecorator
//   6. PipelineCommitDomainEventDecorator
//   7. PipelineValidationDecorator
//   8. PipelineExceptionDecorator
```

### How the Pipeline Works

```
Request arrives at IMediator.SendAsync()
  │
  ▼
PipelineRequestHandler<TRequest> (pre-built delegate chain)
  │
  ├── PipelineExceptionDecorator       ← catches exceptions
  │     ├── PipelineValidationDecorator ← runs ICompositeValidator
  │     │     ├── PipelinePreHandlerDecorator  ← IRequestPreHandler
  │     │     │     ├── YOUR HANDLER           ← IRequestHandler<TRequest>
  │     │     │     └── PipelinePostHandlerDecorator ← IRequestPostHandler
  │     │     └──
  │     └──
  └── Result returned to caller
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
