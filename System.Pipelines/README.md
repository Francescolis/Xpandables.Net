# System.Results

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Results.svg)](https://www.nuget.org/packages/Xpandables.Results)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Results.svg)](https://www.nuget.org/packages/Xpandables.Results)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Result pattern, request/handler abstractions, and pipeline decorator contracts for .NET.

## 📖 Overview

`System.Results` (NuGet: **Xpandables.Results**) provides `Result` / `Result<TValue>` types for representing operation outcomes with HTTP status codes, error collections, headers, and extensions. It also defines the request/handler interfaces (`IRequest`, `IRequestHandler`) and pipeline decorator contracts (`IPipelineDecorator`, `IPipelineRequestHandler`) for CQRS-style dispatching. Namespace: `System.Results`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

### 📋 Result Types

| Type | File | Description |
|------|------|-------------|
| `ResultBase` | `ResultBase.cs` | Abstract base — `StatusCode`, `Title`, `Detail`, `Location`, `Errors`, `Headers`, `Extensions`, `Exception` |
| `Result` | `Result.cs` | Non-generic result (`IsGeneric = false`) |
| `Result<TValue>` | `Result.cs` | Generic result with `Value` property, implicit conversion to/from `Result` |
| `SuccessResult` / `SuccessResultBuilder` | `SuccessResult.cs` / `SuccessResultBuilder.cs` | Fluent success builders |
| `FailureResult` / `FailureResultBuilder` | `FailureResult.cs` / `FailureResultBuilder.cs` | Fluent failure builders |
| `IResultBuilder` | `IResultBuilder.cs` | Builder contract |
| `ResultBuilder` | `ResultBuilder.cs` | Default builder |
| `ResultException` | `ResultException.cs` | Exception wrapping a `Result` |
| `ResultJsonContext` | `ResultJsonContext.cs` | Source-generated JSON serialization context |

### 📨 Request / Handler

| Type | File | Description |
|------|------|-------------|
| `IRequest` | `Requests/IRequest.cs` | Marker interface for pipeline dispatch |
| `IRequest<TResult>` | `Requests/IRequest.cs` | Request producing `Result<TResult>` |
| `IStreamRequest<TResult>` | `Requests/IRequest.cs` | Request producing `IAsyncEnumerable<TResult>` |
| `IStreamPagedRequest<TResult>` | `Requests/IRequest.cs` | Request producing `IAsyncPagedEnumerable<TResult>` |
| `IRequestHandler<TRequest>` | `Requests/IRequestHandler.cs` | Handler returning `Result` |
| `IRequestHandler<TRequest, TResponse>` | `Requests/IRequestHandler.cs` | Handler returning `Result<TResponse>` |
| `IStreamRequestHandler<TRequest, TResult>` | `Requests/IStreamRequestHandler.cs` | Stream handler |
| `IStreamPagedRequestHandler<TRequest, TResult>` | `Requests/IStreamPagedRequestHandler.cs` | Paged stream handler |
| `IRequestContextHandler<TRequest>` | `Requests/IRequestContextHandler.cs` | Handler receiving `RequestContext<TRequest>` |
| `IRequestPreHandler<TRequest>` | `Requests/IRequestPreHandler.cs` | Pre-processing hook |
| `IRequestPostHandler<TRequest>` | `Requests/IRequestPostHandler.cs` | Post-processing hook |
| `IRequestExceptionHandler<TRequest>` | `Requests/IRequestExceptionHandler.cs` | Exception handler |
| `RequestContext<TRequest>` | `Requests/RequestContext.cs` | Context wrapper for pipeline |
| `RequestHandler` | `Requests/RequestHandler.cs` | Delegate type for pipeline next |

### 🔗 Pipeline Contracts

| Type | File | Description |
|------|------|-------------|
| `IPipelineDecorator<TRequest>` | `Pipelines/IPipelineDecorator.cs` | Decorator wrapping pipeline handlers |
| `IPipelineRequestHandler<TRequest>` | `Pipelines/IPipelineRequestHandler.cs` | Pipeline entry point for a request |

### ⚙️ Dependency Injection

C# 14 extension members on `IServiceCollection`:

```csharp
services.AddXRequestHandlers(typeof(Program).Assembly);  // Scan & register sealed handlers
services.AddXPipelineDecorator(typeof(MyDecorator<>));   // Register custom pipeline decorator
```

## 📦 Installation

```bash
dotnet add package Xpandables.Results
```

**Project References:** `Xpandables.Primitives`, `Xpandables.AsyncPaged`

## 🚀 Quick Start

### Creating Success Results

```csharp
using System.Results;

// 200 OK — non-generic
Result ok = Result.Success();

// 200 OK — with value
Result<UserDto> userResult = Result.Success(new UserDto(id, "Alice", "alice@example.com"));

// 201 Created — with value and Location header
Result<OrderDto> created = Result.Created<OrderDto>()
    .WithValue(orderDto)
    .WithLocation(new Uri($"/api/orders/{orderDto.Id}", UriKind.Relative));

// 204 No Content
Result noContent = Result.NoContent();

// 202 Accepted
Result accepted = Result.Accepted();

// Builders are implicitly convertible to Result — you can return them directly
public Task<Result<UserDto>> HandleAsync(...)
{
    UserDto user = /* fetch */;
    return Task.FromResult<Result<UserDto>>(
        Result.Success(user).WithHeader("X-Request-Id", requestId));
}
```

### Creating Failure Results

```csharp
// 400 Bad Request — with keyed error
Result badRequest = Result.Failure("Email", "Invalid email format");

// 404 Not Found — with keyed error
Result<UserDto> notFound = Result.NotFound<UserDto>("UserId", $"User {id} not found");

// 409 Conflict
Result conflict = Result.Conflict("OrderId", "Order already exists");

// 500 Internal Server Error — from exception
try { /* ... */ }
catch (Exception ex)
{
    return Result.InternalServerError(ex);
}

// 401 / 403 / 422
Result unauthorized = Result.Unauthorized();
Result forbidden = Result.Forbidden();
Result unprocessable = Result.UnprocessableEntity();

// Chain builder methods for richer error information
Result detailed = Result.Failure()
    .WithTitle("Validation Failed")
    .WithDetail("One or more fields are invalid.")
    .WithError("Name", "Name is required")
    .WithError("Age", "Must be 18 or older")
    .WithExtension("traceId", Activity.Current?.Id);
```

### Inspecting Results

```csharp
Result<UserDto> result = await mediator.SendAsync<GetUserRequest, UserDto>(request, ct);

if (result.IsSuccess)
{
    UserDto user = result.Value;
    Console.WriteLine($"Found: {user.Name}");
}
else
{
    Console.WriteLine($"Failed: {result.StatusCode} — {result.Title}");

    // Inspect individual errors
    foreach (var error in result.Errors)
        Console.WriteLine($"  [{error.Key}]: {string.Join(", ", error.Values)}");

    // Access exception if present
    if (result.Exception is not null)
        Console.WriteLine($"  Exception: {result.Exception.Message}");
}

// Convert between generic and non-generic
Result nonGeneric = result;                           // implicit
Result<UserDto> backToGeneric = (Result<UserDto>)nonGeneric;  // implicit
```

### Exception-Safe Delegates with Try / TryAsync

```csharp
// Wrap synchronous code — exceptions become FailureResult
Result<int> parsed = ((Func<int>)(() => int.Parse(input))).Try();

// Wrap async code
Result<UserDto> userResult = await FetchUserAsync(id).TryAsync();
```

### Request / Handler Pattern (CQRS)

```csharp
// 1. Define request (query)
public sealed record GetOrderByIdRequest(Guid OrderId) : IRequest<OrderDto>;

// 2. Implement handler
public sealed class GetOrderByIdHandler(AppDbContext db)
    : IRequestHandler<GetOrderByIdRequest, OrderDto>
{
    public async Task<Result<OrderDto>> HandleAsync(
        GetOrderByIdRequest request, CancellationToken ct)
    {
        Order? order = await db.Orders.FindAsync([request.OrderId], ct);

        if (order is null)
            return Result.NotFound<OrderDto>("OrderId", $"Order {request.OrderId} not found");

        return Result.Success(new OrderDto(order.Id, order.CustomerName, order.Total));
    }
}

// 3. Define command
public sealed record CreateOrderRequest(string CustomerName, decimal Total) : IRequest<Guid>;

// 4. Implement command handler
public sealed class CreateOrderHandler(AppDbContext db)
    : IRequestHandler<CreateOrderRequest, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateOrderRequest request, CancellationToken ct)
    {
        var order = new Order { CustomerName = request.CustomerName, Total = request.Total };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return Result.Created<Guid>()
            .WithValue(order.Id)
            .WithLocation(new Uri($"/api/orders/{order.Id}", UriKind.Relative));
    }
}
```

### Stream Request Handler

```csharp
// Request that returns a stream of results
public sealed record GetOrdersStreamRequest(string Status) : IStreamRequest<OrderDto>;

public sealed class GetOrdersStreamHandler(AppDbContext db)
    : IStreamRequestHandler<GetOrdersStreamRequest, OrderDto>
{
    public Task<Result<IAsyncEnumerable<OrderDto>>> HandleAsync(
        GetOrdersStreamRequest request, CancellationToken ct)
    {
        IAsyncEnumerable<OrderDto> stream = db.Orders
            .Where(o => o.Status == request.Status)
            .Select(o => new OrderDto(o.Id, o.CustomerName, o.Total))
            .AsAsyncEnumerable();

        return Task.FromResult(Result.Success(stream));
    }
}
```

### Pre-Processing and Exception Handling Hooks

```csharp
// Pre-handler: runs before the main handler (e.g., logging, enrichment)
public sealed class AuditPreHandler<TRequest> : IRequestPreHandler<TRequest>
    where TRequest : class, IRequest
{
    public Task<Result> HandleAsync(RequestContext<TRequest> context, CancellationToken ct)
    {
        Console.WriteLine($"Processing: {typeof(TRequest).Name}");
        return Task.FromResult(Result.Success().Build() as Result);
    }
}

// Exception handler: catches and transforms exceptions
public sealed class GlobalExceptionHandler<TRequest> : IRequestExceptionHandler<TRequest>
    where TRequest : class, IRequest
{
    public Task<Result> HandleAsync(
        RequestContext<TRequest> context, Exception exception, CancellationToken ct)
    {
        return Task.FromResult<Result>(
            Result.InternalServerError("Unhandled", exception.Message, exception));
    }
}
```

### Register Handlers via Assembly Scanning

```csharp
// Scan and register all sealed IRequestHandler implementations
services.AddXRequestHandlers(typeof(GetOrderByIdHandler).Assembly);

// Register a custom pipeline decorator
services.AddXPipelineDecorator(typeof(MyCustomDecorator<>));
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
