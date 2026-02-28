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

### Creating Results

```csharp
using System.Results;

// Success
Result success = SuccessResult.Ok();
Result<int> typed = SuccessResult.Ok(42);

// Failure
Result failure = FailureResult.BadRequest("Name", "Name is required");
Result notFound = FailureResult.NotFound("User not found");
```

### Request / Handler

```csharp
// Define request
public sealed record GetUserRequest(Guid Id) : IRequest<UserDto>;

// Implement handler
public sealed class GetUserHandler : IRequestHandler<GetUserRequest, UserDto>
{
    public async Task<Result<UserDto>> HandleAsync(
        GetUserRequest request, CancellationToken ct)
    {
        // fetch user...
        return SuccessResult.Ok(userDto);
    }
}
```

### Register Handlers

```csharp
services.AddXRequestHandlers(typeof(GetUserHandler).Assembly);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
