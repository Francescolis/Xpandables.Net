# 📡 System.Results.Tasks

[![NuGet](https://img.shields.io/badge/NuGet-10.0.1-blue.svg)](https://www.nuget.org/packages/Xpandables.Results.Tasks)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Mediator & Handler Dispatch** - CQRS mediator for dispatching requests to handlers with optional pipeline decorators.

---

## 📋 Overview

`Xpandables.Results.Tasks` provides the `IMediator` interface and `Mediator` implementation used to dispatch `IRequest` instances to their registered handlers. It is the runtime dispatcher typically used with `Xpandables.Results` request/handler contracts and `Xpandables.Results.Pipelines` decorators.

### ✨ Key Features

- 📡 **IMediator** - Central interface for sending requests to handlers
- 🎯 **Mediator** - Default implementation with handler resolution
- 🔄 **Request Dispatch** - Automatic routing to appropriate handlers
- 🔌 **DI Integration** - Seamless dependency injection support
- ⚡ **Async First** - Fully asynchronous request handling
- 🔗 **Pipeline Support** - Works with `IPipelineRequestHandler` decorators

---

## 📥 Installation

```bash
dotnet add package Xpandables.Results.Tasks
```

---

## 🚀 Quick Start

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register mediator
builder.Services.AddXMediator();

// Register request handlers from assembly
builder.Services.AddXRequestHandlers(typeof(Program).Assembly);
```

### Define Requests and Handlers

```csharp
using System.Results;
using System.Results.Requests;

// Define a query
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<User>;

// Implement handler
public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
        => _repository = repository;

    public async Task<Result<User>> HandleAsync(
        GetUserByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var user = await _repository.FindByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.NotFound<User>("userId", "User not found");
        }

        return Result.Success(user);
    }
}
```

### Send Requests via Mediator

```csharp
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        Result<User> result = await _mediator.SendAsync(new GetUserByIdQuery(id));

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Errors);
    }
}
```

---

## 🧩 Core Concepts

### IMediator Interface

```csharp
public interface IMediator
{
    /// <summary>
    /// Sends the specified request asynchronously and returns the result.
    /// </summary>
    Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}
```

### Commands and Queries

```csharp
// Command - performs an action, may or may not return a value
public sealed record CreateUserCommand(string Name, string Email) : IRequest<User>;
public sealed record DeleteUserCommand(Guid UserId) : IRequest;

// Query - retrieves data
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<User>;
public sealed record GetAllUsersQuery : IRequest<IEnumerable<User>>;
```

### Using the Mediator

```csharp
public class OrderService
{
    private readonly IMediator _mediator;

    public OrderService(IMediator mediator) => _mediator = mediator;

    public async Task<Result<Order>> CreateOrderAsync(
        Guid customerId,
        List<OrderItem> items,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(customerId, items);
        return await _mediator.SendAsync(command, cancellationToken);
    }

    public async Task<Result<Order>> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(orderId);
        return await _mediator.SendAsync(query, cancellationToken);
    }
}
```

---

## 🔄 Pipeline Integration

The mediator works seamlessly with pipeline decorators from `System.Results.Pipelines`:

```csharp
// Register pipeline decorators
builder.Services.AddXPipelineRequestHandler();
builder.Services.AddXPipelineValidationDecorator();
builder.Services.AddXPipelineUnitOfWorkDecorator();
builder.Services.AddXPipelineExceptionDecorator();

// Requests are now processed through the pipeline
var result = await _mediator.SendAsync(new CreateUserCommand("John", "john@example.com"));
// 1. Exception decorator catches errors
// 2. Validation decorator validates request
// 3. Handler executes
// 4. Unit of work saves changes
```

---

## ⚙️ Complete Configuration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Core mediator
builder.Services.AddXMediator();

// Request handlers
builder.Services.AddXRequestHandlers(typeof(Program).Assembly);

// Optional: Pipeline decorators
builder.Services.AddXPipelineRequestHandler();
builder.Services.AddXPipelineExceptionDecorator();
builder.Services.AddXPipelineValidationDecorator();
builder.Services.AddXPipelinePreDecorator();
builder.Services.AddXPipelinePostDecorator();

// Optional: Validators
builder.Services.AddXRuleValidators(typeof(Program).Assembly);
```

---

## ✅ Best Practices

1. **Use CQRS separation** - Commands modify state, queries read state
2. **One handler per request** - Keep handlers focused and testable
3. **Use the mediator** - Don't inject handlers directly
4. **Combine with pipelines** - Add cross-cutting concerns via decorators
5. **Handle errors as Results** - Return Result.Failure instead of throwing

---

## 📚 Related Packages

- **System.Results** - Core Result types and request/handler interfaces
- **System.Results.Pipelines** - Pipeline decorators for validation, transactions, etc.
- **System.Primitives.Validation** - Rule validators and specifications
- **AspNetCore.Net** - ASP.NET Core integration

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
