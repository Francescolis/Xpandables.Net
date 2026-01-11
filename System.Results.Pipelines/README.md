# 🔗 Xpandables.Results.Pipelines

[![NuGet](https://img.shields.io/badge/NuGet-10.0.1-blue.svg)](https://www.nuget.org/packages/Xpandables.Results.Pipelines)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Pipeline Decorators** - Pre-built pipeline decorators for validation, transactions, domain events, exception handling, and other cross-cutting concerns in CQRS request pipelines.

---

## 🎯 Overview

`Xpandables.Results.Pipelines` provides production-ready pipeline decorators that wrap request handlers with cross-cutting concerns. These decorators execute before and after your main handler logic, enabling clean separation of business logic from infrastructure concerns like validation, transactions, event publishing, and more.

### ✨ Key Features

- ✅ **PipelineValidationDecorator** - Automatic request validation for `IRequiresValidation`
- 🔄 **PipelineUnitOfWorkDecorator** - Unit of Work integration for `IRequiresUnitOfWork`
- 📡 **PipelineDomainEventsDecorator** - Publish domain events for `IRequiresDomainEvents`
- 📮 **PipelineIntegrationOutboxDecorator** - Outbox publishing for `IRequiresIntegrationOutbox`
- 📝 **PipelinePreDecorator** - Execute logic before the main handler
- 📝 **PipelinePostDecorator** - Execute logic after the main handler
- ⚠️ **PipelineExceptionDecorator** - Centralized exception handling
- 💾 **PipelineEventStoreEventDecorator** - Automatic event store persistence
- 🧩 **Composable** - Chain multiple decorators together

---

## 📥 Installation

```bash
dotnet add package Xpandables.Results.Pipelines
```

---

## 🚀 Quick Start

### Register Pipeline Decorators

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register the pipeline request handler
builder.Services.AddXPipelineRequestHandler();

// Add decorators (order matters - first registered runs outermost)
builder.Services.AddXPipelineExceptionDecorator();       // Handle exceptions
builder.Services.AddXPipelineValidationDecorator();      // Validate requests
builder.Services.AddXPipelinePreDecorator();             // Pre-processing
builder.Services.AddXPipelineUnitOfWorkDecorator();      // Transaction management
builder.Services.AddXPipelineDomainEventsDecorator();    // Publish domain events
builder.Services.AddXPipelineIntegrationOutboxDecorator(); // Outbox pattern
builder.Services.AddXPipelinePostDecorator();            // Post-processing
```

### Mark Requests for Pipeline Features

```csharp
using System.Results.Requests;
using System.ComponentModel.DataAnnotations;

// Request that requires validation
public sealed record CreateUserCommand(string Name, string Email) 
    : IRequest<User>, IRequiresValidation;

// Request that requires Unit of Work (transaction)
public sealed record UpdateOrderCommand(Guid OrderId, OrderStatus Status) 
    : IRequest, IRequiresUnitOfWork;

// Request that may publish domain events
public sealed record ProcessPaymentCommand(Guid OrderId, decimal Amount) 
    : IRequest<PaymentResult>, IRequiresDomainEvents;
```

---

## 🧩 Available Decorators

### PipelineValidationDecorator

Validates requests implementing `IRequiresValidation` before handler execution:

```csharp
// Register
builder.Services.AddXPipelineValidationDecorator();

// Request with validation
public sealed record CreateProductCommand(string Name, decimal Price) 
    : IRequest<Product>, IRequiresValidation;

// Validator
public sealed class CreateProductValidator : RuleValidator<CreateProductCommand>
{
    public override IReadOnlyCollection<ValidationResult> Validate(CreateProductCommand instance)
    {
        var results = new List<ValidationResult>();
        
        if (string.IsNullOrWhiteSpace(instance.Name))
            results.Add(new ValidationResult("Name is required", [nameof(instance.Name)]));
        
        if (instance.Price <= 0)
            results.Add(new ValidationResult("Price must be positive", [nameof(instance.Price)]));
        
        return results;
    }
}
```

### PipelineUnitOfWorkDecorator

Automatically saves changes after handler execution for requests implementing `IRequiresUnitOfWork`:

```csharp
// Register
builder.Services.AddXPipelineUnitOfWorkDecorator();

// Request with unit of work
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderItem> Items) 
    : IRequest<Order>, IRequiresUnitOfWork;

// Handler - SaveChanges is called automatically after execution
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IRepository _repository;

    public async Task<Result<Order>> HandleAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = new Order { CustomerId = request.CustomerId, Items = request.Items };
        await _repository.AddAsync(cancellationToken, order);
        // No need to call SaveChanges - decorator handles it
        return Result.Created(order);
    }
}
```

### PipelinePreDecorator / PipelinePostDecorator

Execute custom logic before and after the main handler:

```csharp
// Register
builder.Services.AddXPipelinePreDecorator();
builder.Services.AddXPipelinePostDecorator();

// Implement pre-handler
public sealed class LoggingPreHandler<TRequest> : IRequestPreHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly ILogger<LoggingPreHandler<TRequest>> _logger;

    public LoggingPreHandler(ILogger<LoggingPreHandler<TRequest>> logger)
        => _logger = logger;

    public Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting {RequestType}", typeof(TRequest).Name);
        return Task.FromResult(Result.Success());
    }
}

// Implement post-handler
public sealed class AuditPostHandler<TRequest> : IRequestPostHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly IAuditService _auditService;

    public async Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        Result result,
        CancellationToken cancellationToken = default)
    {
        await _auditService.LogAsync(typeof(TRequest).Name, result.IsSuccess);
        return result;
    }
}
```

### PipelineExceptionDecorator

Catches exceptions and converts them to Result failures:

```csharp
// Register
builder.Services.AddXPipelineExceptionDecorator();

// Implement exception handler
public sealed class GlobalExceptionHandler<TRequest> : IRequestExceptionHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly ILogger<GlobalExceptionHandler<TRequest>> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler<TRequest>> logger)
        => _logger = logger;

    public Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Request {Request} failed", typeof(TRequest).Name);
        return Task.FromResult(Result.InternalServerError(exception).Build());
    }
}
```

### PipelineDomainEventsDecorator

Buffers and publishes domain events after successful handler execution:

```csharp
// Register
builder.Services.AddXPipelineDomainEventsDecorator();

// Events are collected from aggregates and published after handler completes
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IPendingDomainEventsBuffer _eventsBuffer;
    private readonly IRepository _repository;

    public async Task<Result<Order>> HandleAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(request.CustomerId, request.Items);
        // Order.Create raises OrderCreatedEvent
        
        _eventsBuffer.Add(order.DomainEvents);
        await _repository.AddAsync(cancellationToken, order);
        
        return Result.Created(order);
        // After success, decorator publishes all buffered events
    }
}
```

### PipelineIntegrationOutboxDecorator

Implements the outbox pattern for reliable integration event publishing:

```csharp
// Register
builder.Services.AddXPipelineIntegrationOutboxDecorator();

// Integration events are stored in outbox and published reliably
public sealed class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, PaymentResult>
{
    private readonly IPendingIntegrationEventsBuffer _eventsBuffer;

    public async Task<Result<PaymentResult>> HandleAsync(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // Process payment...
        var result = new PaymentResult { Success = true };
        
        // Add integration event to outbox buffer
        _eventsBuffer.Add(new PaymentProcessedEvent(request.OrderId, request.Amount));
        
        return Result.Success(result);
        // Decorator persists events to outbox for reliable delivery
    }
}
```

---

## ⚙️ Complete Configuration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Core pipeline
builder.Services.AddXPipelineRequestHandler();

// All decorators (order: outermost to innermost)
builder.Services.AddXPipelineExceptionDecorator();           // 1. Catch all exceptions
builder.Services.AddXPipelineValidationDecorator();          // 2. Validate request
builder.Services.AddXPipelinePreDecorator();                 // 3. Pre-processing
builder.Services.AddXPipelineEventStoreEventDecorator();     // 4. Event store
builder.Services.AddXPipelineUnitOfWorkDecorator();          // 5. Transaction
builder.Services.AddXPipelineDomainEventsDecorator();        // 6. Domain events
builder.Services.AddXPipelineIntegrationOutboxDecorator();   // 7. Outbox
builder.Services.AddXPipelinePostDecorator();                // 8. Post-processing

// Register validators
builder.Services
    .AddXValidator()
    .AddXValidatorFactory()
    .AddXValidatorProvider()
    .AddXValidators(typeof(Program).Assembly);
```

---

## ✅ Best Practices

1. **Order decorators carefully** - Exception handler should be outermost
2. **Use marker interfaces** - IRequiresValidation, IRequiresUnitOfWork enable specific decorators
3. **Keep handlers focused** - Let decorators handle cross-cutting concerns
4. **Register all pre/post handlers** - They run for all matching requests
5. **Buffer events** - Use the provided buffers for domain/integration events
6. **Test decorators** - Each decorator can be unit tested independently

---

## 📚 Related Packages

- **Xpandables.Results** - Core Result types and request/handler interfaces
- **Xpandables.Results.Tasks** - Mediator implementation
- **Xpandables.Validation** - Specifications and validator abstractions
- **Xpandables.Events** - Domain and integration event abstractions

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
