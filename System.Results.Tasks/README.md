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

```csharp
using System.Results.Tasks;

// Register
services.AddXRequestHandlers(typeof(Program).Assembly);
services.AddXMediatorWithPipelines();

// Use
public class MyService(IMediator mediator)
{
    public async Task<Result<UserDto>> GetUserAsync(Guid id, CancellationToken ct)
    {
        var request = new GetUserRequest(id);
        return await mediator.SendAsync<GetUserRequest, UserDto>(request, ct);
    }
}
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
