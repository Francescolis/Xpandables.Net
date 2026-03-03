# System.Results.Pipelines

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Results.Pipelines.svg)](https://www.nuget.org/packages/Xpandables.Results.Pipelines)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Results.Pipelines.svg)](https://www.nuget.org/packages/Xpandables.Results.Pipelines)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Pipeline decorator implementations for cross-cutting concerns in the mediator pipeline.

## 📖 Overview

`System.Results.Pipelines` (NuGet: **Xpandables.Results.Pipelines**) provides concrete `IPipelineDecorator<TRequest>` implementations for validation, exception handling, pre/post-processing, unit-of-work, and event sourcing concerns. Namespace: `System.Results.Pipelines`.

Built for **.NET 10** and **C# 14**.

## ✨ Pipeline Decorators

| Type | File | Description |
|------|------|-------------|
| `PipelineValidationDecorator<TRequest>` | `PipelineValidationDecorator.cs` | Runs `ICompositeValidator<TRequest>` before the handler |
| `PipelineExceptionDecorator<TRequest>` | `PipelineExceptionDecorator.cs` | Catches exceptions, delegates to `IRequestExceptionHandler<TRequest>` |
| `PipelinePreHandlerDecorator<TRequest>` | `PipelinePreHandlerDecorator.cs` | Invokes `IRequestPreHandler<TRequest>` before the handler |
| `PipelinePostHandlerDecorator<TRequest>` | `PipelinePostHandlerDecorator.cs` | Invokes `IRequestPostHandler<TRequest>` after the handler |
| `PipelineRequireEntityUnitOfWorkDecorator<TRequest>` | `PipelineRequireEntityUnitOfWorkDecorator.cs` | Wraps handler in Entity Framework `IEntityUnitOfWork` transaction |
| `PipelineRequireDataUnitOfWorkDecorator<TRequest>` | `PipelineRequireDataUnitOfWorkDecorator.cs` | Wraps handler in ADO.NET `IDataUnitOfWork` transaction |
| `PipelineCommitDomainEventDecorator<TRequest>` | `PipelineCommitDomainEventDecorator.cs` | Commits pending domain events after handler |
| `PipelinePublishDomainEventDecorator<TRequest>` | `PipelinePublishDomainEventDecorator.cs` | Publishes domain events via `IEventPublisher` |
| `PipelineEnqueueIntegrationEventDecorator<TRequest>` | `PipelineEnqueueIntegrationEventDecorator.cs` | Enqueues integration events to outbox |

### ⚙️ Dependency Injection

C# 14 extension members on `IServiceCollection`:

```csharp
services.AddXPipelineValidationDecorator();
services.AddXPipelineExceptionDecorator();
services.AddXPipelinePreHanderDecorator();
services.AddXPipelinePostHanderDecorator();
services.AddXPipelineRequireEntityUnitOfWorkDecorator();
services.AddXPipelineRequireDataUnitOfWorkDecorator();
services.AddXPipelineCommitDomainEventDecorator();
services.AddXPipelinePublishDomainEventDecorator();
services.AddXPipelineEnqueueIntegrationEventDecorator();
```

## 📦 Installation

```bash
dotnet add package Xpandables.Results.Pipelines
```

**Project References:** `Xpandables.Results`, `Xpandables.Validation`, `Xpandables.Data`, `Xpandables.Entities`, `Xpandables.Events`

## 🚀 Quick Start

### Validation Decorator

Automatically validates requests before they reach the handler. If validation fails, a `FailureResult` is returned immediately.

```csharp
// 1. Mark a request for validation
public sealed record CreateProductRequest(string Name, decimal Price) : IRequest<Guid>;

// 2. Implement a validator (from System.Validation)
public sealed class CreateProductValidator : ICompositeValidator<CreateProductRequest>
{
    public ValueTask<Result> ValidateAsync(
        CreateProductRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ValueTask.FromResult<Result>(
                Result.Failure("Name", "Product name is required"));

        if (request.Price <= 0)
            return ValueTask.FromResult<Result>(
                Result.Failure("Price", "Price must be positive"));

        return ValueTask.FromResult(Result.Success().Build() as Result);
    }
}

// 3. Register the decorator
services.AddXPipelineValidationDecorator();
// The decorator intercepts CreateProductRequest, runs CreateProductValidator,
// and short-circuits with 400 if validation fails.
```

### Exception Decorator

Catches unhandled exceptions and delegates to `IRequestExceptionHandler<TRequest>`.

```csharp
// Implement a global exception handler for a specific request type
public sealed class CreateProductExceptionHandler
    : IRequestExceptionHandler<CreateProductRequest>
{
    public Task<Result> HandleAsync(
        RequestContext<CreateProductRequest> context,
        Exception exception,
        CancellationToken ct)
    {
        // Log, transform, or return a failure result
        return Task.FromResult<Result>(
            Result.InternalServerError(
                "CreateProduct", "Failed to create product", exception));
    }
}

services.AddXPipelineExceptionDecorator();
```

### Pre/Post Handler Decorators

```csharp
// Pre-handler: runs before the main handler
public sealed class AuditPreHandler : IRequestPreHandler<CreateProductRequest>
{
    public Task<Result> HandleAsync(
        RequestContext<CreateProductRequest> context, CancellationToken ct)
    {
        Console.WriteLine($"Creating product: {context.Request.Name}");
        return Task.FromResult(Result.Success().Build() as Result);
    }
}

// Post-handler: runs after the main handler succeeds
public sealed class NotifyPostHandler : IRequestPostHandler<CreateProductRequest>
{
    public Task<Result> HandleAsync(
        RequestContext<CreateProductRequest> context, CancellationToken ct)
    {
        Console.WriteLine($"Product created successfully.");
        return Task.FromResult(Result.Success().Build() as Result);
    }
}

services.AddXPipelinePreHanderDecorator();
services.AddXPipelinePostHanderDecorator();
```

### Entity Framework Unit-of-Work Decorator

Wraps the handler in a transaction when the request implements `IEntityRequiresUnitOfWork`.

```csharp
// Mark the request as requiring a transaction
public sealed record TransferFundsRequest(Guid FromId, Guid ToId, decimal Amount)
    : IRequest, IEntityRequiresUnitOfWork;

// The handler runs inside SaveChangesAsync — automatic commit on success, rollback on failure
public sealed class TransferFundsHandler(AppDbContext db)
    : IRequestHandler<TransferFundsRequest>
{
    public async Task<Result> HandleAsync(
        TransferFundsRequest request, CancellationToken ct)
    {
        var from = await db.Accounts.FindAsync([request.FromId], ct);
        var to = await db.Accounts.FindAsync([request.ToId], ct);

        from!.Balance -= request.Amount;
        to!.Balance += request.Amount;

        // No need to call SaveChangesAsync — the decorator handles it
        return Result.Success();
    }
}

services.AddXPipelineRequireEntityUnitOfWorkDecorator();
```

### ADO.NET Unit-of-Work Decorator

Wraps the handler in an ADO.NET transaction when the request implements `IDataRequiresUnitOfWork`.

```csharp
public sealed record BulkInsertOrdersRequest(List<OrderData> Orders)
    : IRequest, IDataRequiresUnitOfWork;

services.AddXPipelineRequireDataUnitOfWorkDecorator();
```

### Event Sourcing Decorators

For event-sourced systems, these decorators manage domain event commits, publishing, and integration event outbox enqueuing.

```csharp
// Register all event sourcing decorators at once
services.AddXPipelinePublishDomainEventDecorator();
services.AddXPipelineEnqueueIntegrationEventDecorator();
services.AddXPipelineCommitDomainEventDecorator();

// Or register the full event sourcing pipeline via System.Results.Tasks:
// services.AddXMediatorWithEventSourcingPipelines();
```

### Custom Pipeline Decorator

```csharp
// Implement IPipelineDecorator<TRequest> for custom cross-cutting concerns
public sealed class TimingDecorator<TRequest> : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    public async Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        Result result = await nextHandler(ct);
        sw.Stop();

        Console.WriteLine(
            $"{typeof(TRequest).Name} completed in {sw.ElapsedMilliseconds}ms");

        return result;
    }
}

// Register
services.AddXPipelineDecorator(typeof(TimingDecorator<>));
```

### Complete Registration Example

```csharp
// Standard pipeline with custom timing
services.AddXRequestHandlers(typeof(Program).Assembly);
services.AddXMediator();
services.AddXPipelineDecorator(typeof(TimingDecorator<>));  // custom
services.AddXPipelinePreHanderDecorator();
services.AddXPipelinePostHanderDecorator();
services.AddXPipelineRequireEntityUnitOfWorkDecorator();
services.AddXPipelineValidationDecorator();
services.AddXPipelineExceptionDecorator();

// ⚠️ Order matters! Decorators execute from outermost to innermost.
// ExceptionDecorator should be last registered (wraps everything).
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
