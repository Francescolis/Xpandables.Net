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

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
