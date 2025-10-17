# ?? Xpandables.Net.Tasks

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Mediator Pattern & CQRS Implementation** - Decouple your application with a powerful mediator pipeline supporting request/response patterns, decorators, and dependency management.

---

## ?? Overview

`Xpandables.Net.Tasks` provides a robust implementation of the Mediator pattern, enabling loose coupling between components through a central request/response pipeline. Perfect for implementing CQRS, command handlers, and complex business workflows.

### ? Key Features

- ?? **Mediator Pattern** - Decoupled request/response handling
- ?? **CQRS Support** - Commands and queries separation
- ?? **Pipeline Behaviors** - Pre/post processing, validation, logging
- ?? **Decorator Pattern** - Extensible request handlers
- ?? **Async First** - Built for asynchronous operations
- ?? **Dependency Injection** - Seamless DI integration
- ? **High Performance** - Minimal overhead

---

## ?? Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.Tasks
```

### Basic Setup

```csharp
using Xpandables.Net.Tasks;
using Microsoft.Extensions.DependencyInjection;

// Register services
services.AddXMediator(options =>
{
    options.AddHandlers(typeof(Program).Assembly);
    options.AddPipelineBehaviors();
});
```

### Basic Usage

```csharp
// 1. Define a request
public sealed record GetUserQuery(Guid UserId) : IRequest;

// 2. Implement a handler
public sealed class GetUserHandler : IRequestHandler<GetUserQuery>
{
    private readonly IUserRepository _repository;
    
    public GetUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ExecutionResult> HandleAsync(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.UserId);
        
        return user is not null
            ? ExecutionResult.Success(user)
            : ExecutionResult.NotFound()
                .WithTitle("User not found");
    }
}

// 3. Send the request
public class UserController
{
    private readonly IMediator _mediator;
    
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _mediator.SendAsync(new GetUserQuery(id));
        return result.ToActionResult();
    }
}
```

---

## ?? Core Concepts

### Requests

```csharp
// Simple request (no specific response type)
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderItem> Items) 
    : IRequest;

// Request with typed response
public sealed record GetOrderQuery(Guid OrderId) 
    : IRequest;

// Request requiring validation
public sealed record UpdateUserCommand(Guid UserId, string Email) 
    : IRequest, IRequiresValidation;
```

### Handlers

```csharp
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<CreateOrderHandler> _logger;
    
    public CreateOrderHandler(
        IOrderRepository repository,
        ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<ExecutionResult> HandleAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Items = request.Items,
                CreatedAt = DateTime.UtcNow
            };
            
            await _repository.AddAsync(order);
            
            _logger.LogInformation(
                "Order {OrderId} created successfully", 
                order.Id);
            
            return ExecutionResult
                .Created(order)
                .WithLocation($"/api/orders/{order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            return ExecutionResult
                .InternalServerError()
                .WithException(ex);
        }
    }
}
```

---

## ?? Pipeline Behaviors

### Pre-Processing Decorator

```csharp
public sealed class LoggingPreDecorator<TRequest> 
    : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    private readonly ILogger<LoggingPreDecorator<TRequest>> _logger;
    
    public LoggingPreDecorator(ILogger<LoggingPreDecorator<TRequest>> logger)
    {
        _logger = logger;
    }
    
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executing request {RequestType}",
            typeof(TRequest).Name);
        
        var result = await next();
        
        return result;
    }
}
```

### Post-Processing Decorator

```csharp
public sealed class PerformanceMonitoringDecorator<TRequest> 
    : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    private readonly ILogger _logger;
    
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        
        var result = await next();
        
        sw.Stop();
        
        _logger.LogInformation(
            "Request {RequestType} completed in {ElapsedMs}ms",
            typeof(TRequest).Name,
            sw.ElapsedMilliseconds);
        
        return result;
    }
}
```

### Validation Decorator

```csharp
public sealed class ValidationDecorator<TRequest> 
    : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresValidation
{
    private readonly IValidator<TRequest> _validator;
    
    public ValidationDecorator(IValidator<TRequest> validator)
    {
        _validator = validator;
    }
    
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        var validationResults = await _validator.ValidateAsync(request);
        
        if (validationResults.Any())
        {
            var result = ExecutionResult
                .BadRequest()
                .WithTitle("Validation failed");
            
            foreach (var error in validationResults)
            {
                result = result.WithError(
                    error.MemberNames.FirstOrDefault() ?? "General",
                    error.ErrorMessage ?? "Validation error");
            }
            
            return result;
        }
        
        return await next();
    }
}
```

---

## ?? Advanced Examples

### Example 1: CQRS with Commands and Queries

```csharp
// Command - Modifies state
public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    int Stock) : IRequest, IRequiresValidation;

public sealed class CreateProductHandler 
    : IRequestHandler<CreateProductCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };
        
        await _repository.AddAsync(product);
        
        return ExecutionResult
            .Created(product)
            .WithLocation($"/api/products/{product.Id}");
    }
}

// Query - Reads state
public sealed record GetProductsQuery(int PageSize, int PageNumber) 
    : IRequest;

public sealed class GetProductsHandler 
    : IRequestHandler<GetProductsQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _repository
            .GetPagedAsync(request.PageNumber, request.PageSize);
        
        return ExecutionResult.Success(products);
    }
}
```

### Example 2: Transaction Management

```csharp
public sealed class TransactionDecorator<TRequest> 
    : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var result = await next();
            
            if (result.IsSuccess)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
            
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### Example 3: Caching Decorator

```csharp
public sealed class CachingDecorator<TRequest> 
    : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, ICacheable
{
    private readonly IDistributedCache _cache;
    
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.GetCacheKey();
        
        // Try to get from cache
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData is not null)
        {
            var result = JsonSerializer.Deserialize<ExecutionResult>(cachedData);
            return result!;
        }
        
        // Execute handler
        var executionResult = await next();
        
        // Cache successful results
        if (executionResult.IsSuccess)
        {
            var serialized = JsonSerializer.Serialize(executionResult);
            await _cache.SetStringAsync(
                cacheKey,
                serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
        }
        
        return executionResult;
    }
}
```

---

## ?? Dependency Management

### Dependent Requests

```csharp
public sealed record ProcessOrderCommand(Guid OrderId) 
    : IDependencyRequest
{
    public IEnumerable<IRequest> GetDependencies()
    {
        yield return new ValidateInventoryQuery(OrderId);
        yield return new CalculateShippingQuery(OrderId);
    }
}

// Dependencies are executed first
var result = await _mediator.SendAsync(new ProcessOrderCommand(orderId));
```

---

## ?? Best Practices

1. **Keep Handlers Focused**: One responsibility per handler
   ```csharp
   // ? Do
   public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand>
   
   // ? Don't mix concerns
   public sealed class OrderHandler : IRequestHandler<CreateOrderCommand>, 
       IRequestHandler<UpdateOrderCommand>
   ```

2. **Use Pipeline Decorators for Cross-Cutting Concerns**:
   - Logging
   - Validation
   - Caching
   - Transaction management
   - Performance monitoring

3. **Return ExecutionResult**: Consistent error handling
   ```csharp
   return result.IsSuccess
       ? ExecutionResult.Success(data)
       : ExecutionResult.BadRequest().WithTitle("Operation failed");
   ```

4. **Leverage Dependency Injection**: Constructor injection for all dependencies

---

## ?? Related Packages

- [`Xpandables.Net.ExecutionResults`](../Xpandables.Net.ExecutionResults/README.md) - Result handling
- [`Xpandables.Net.Validators`](../Xpandables.Net.Validators/README.md) - Validation support
- [`Xpandables.Net.Validators.Pipelines`](../Xpandables.Net.Validators.Pipelines/README.md) - Validation decorators

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
