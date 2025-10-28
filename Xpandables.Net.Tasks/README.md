﻿# 📦 Xpandables.Net

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Core Library** - Comprehensive toolkit for building modern .NET applications with ExecutionResult, Optional, CQRS, Event Sourcing, Specifications, and more.

---

## 📋 Overview

`Xpandables.Net` is the core library providing fundamental building blocks for enterprise .NET applications. It includes implementations of the Result pattern, Optional monad, Mediator pattern (CQRS), Event Sourcing, Repository pattern, REST client, and Specification pattern.

### 🎯 Key Features

- ✅ **ExecutionResult** - Railway-oriented programming with HTTP-aware result types
- 🎁 **Optional** - Null-safe value handling (like Rust's Option type)
- 📡 **Mediator/CQRS** - Request/response pipeline with pre/post handlers
- 📝 **Event Sourcing** - Complete event sourcing implementation with aggregates
- 💾 **Repository** - Generic repository pattern with unit of work
- 🌐 **REST Client** - Type-safe, attribute-based HTTP client
- ✔️ **Specifications** - Business rules encapsulation with LINQ support
- 🔄 **Async Paging** - Asynchronous enumerable with pagination

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package Xpandables.Net
```

### Basic Examples

#### ExecutionResult - Railway Oriented Programming

```csharp
using Xpandables.Net.ExecutionResults;

public async Task<ExecutionResult<User>> GetUserAsync(Guid userId)
{
    Optional<User> user = await _repository.FindByIdAsync(userId);
    
    return user
        .Map(u => ExecutionResult.Success(u))
        .Empty(() => ExecutionResult
            .NotFound()
            .WithError("userId", "User not found")
            .Build<User>());
}

// Chain operations
public async Task<ExecutionResult<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateRequest(request)
        .BindAsync(CreateOrder)
        .BindAsync(ProcessPayment)
        .BindAsync(SendConfirmation)
        .Map(order => ExecutionResult.Created(order))
        .Empty(() => ExecutionResult
            .BadRequest()
            .WithError("request", "Failed to create order")
            .Build<Order>());
}
```

#### Optional - Null-Safe Values

```csharp
using Xpandables.Net.Optionals;

// Create optionals
var some = Optional.Some("hello");
var none = Optional.Empty<string>();

// Safe operations
string result = some
    .Map(s => s.ToUpper())
    .GetValueOrDefault("default");  // "HELLO"

// Pattern matching
user.Map(u => Console.WriteLine($"Found: {u.Name}"))
    .Empty(() => Console.WriteLine("User not found"));

// LINQ integration
var users = await repository
    .GetAllAsync()
    .FirstOrEmpty();  // Returns Optional<User>

if (users.IsNotEmpty)
{
    Console.WriteLine(users.Value.Name);
}
```

#### CQRS with Mediator

```csharp
using Xpandables.Net.Cqrs;
using Xpandables.Net.Tasks;
using Xpandables.Net.ExecutionResults;

// Define command
public sealed record CreateUserCommand(
    string Name, 
    string Email) : IRequest<User>;

// Handle command
public sealed class CreateUserHandler 
    : IRequestHandler<CreateUserCommand, User>
{
    private readonly IRepository _repository;
    
    public CreateUserHandler(IRepository repository) 
        => _repository = repository;
    
    public async Task<ExecutionResult<User>> HandleAsync(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = new User 
        { 
            Name = request.Name, 
            Email = request.Email 
        };
        
        await _repository.AddAsync(cancellationToken, user);
        
        return ExecutionResult.Created(user);
    }
}

// Use mediator
var command = new CreateUserCommand("John", "john@example.com");
ExecutionResult<User> result = await _mediator.SendAsync(command);

if (result.IsSuccess)
{
    Console.WriteLine($"User created: {result.Value.Name}");
}
```

#### Event Sourcing

```csharp
using Xpandables.Net.Events;
using Xpandables.Net.Aggregates;

// Define aggregate
public sealed class BankAccountAggregate : Aggregate
{
    public string AccountNumber { get; private set; } = default!;
    public decimal Balance { get; private set; }

    public static BankAccountAggregate Create(
        string accountNumber, 
        decimal initialBalance)
    {
        var aggregate = new BankAccountAggregate();
        aggregate.AppendEvent(new AccountCreatedEvent(
            Guid.NewGuid(),
            accountNumber,
            initialBalance));
        return aggregate;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive");

        AppendEvent(new MoneyDepositedEvent(Id, amount));
    }

    private void On(AccountCreatedEvent @event)
    {
        Id = @event.AggregateId;
        AccountNumber = @event.AccountNumber;
        Balance = @event.InitialBalance;
    }

    private void On(MoneyDepositedEvent @event)
    {
        Balance += @event.Amount;
    }
}

// Use aggregate store
var account = BankAccountAggregate.Create("ACC-001", 1000m);
account.Deposit(500m);

await _aggregateStore.AppendAsync(account);

// Reload from events
var reloaded = await _aggregateStore
    .ReadAsync<BankAccountAggregate>(account.Id);
```

#### Specifications - Business Rules

```csharp
using Xpandables.Net.Validators;

// Create specifications
var isAdult = Specification
    .GreaterThan<Person, int>(p => p.Age, 18);

var hasValidEmail = Specification
    .Contains<Person>(p => p.Email, "@");

var isActive = Specification
    .Equal<Person, bool>(p => p.IsActive, true);

// Combine specifications
var validUser = Specification.All(isAdult, hasValidEmail, isActive);

// Use with LINQ
var users = await _repository
    .FetchAsync<User, User>(q => q.Where(validUser))
    .ToListAsync();

// Check satisfaction
if (validUser.IsSatisfiedBy(person))
{
    Console.WriteLine("Person meets all criteria");
}
```

#### Repository Pattern

```csharp
using Xpandables.Net.Repositories;

// Fetch with filtering
var activeUsers = await _repository
    .FetchAsync<User, User>(q => q
        .Where(u => u.IsActive)
        .OrderBy(u => u.Name))
    .ToListAsync();

// Add entities
await _repository.AddAsync(cancellationToken, user1, user2, user3);

// Update with expression
await _repository.UpdateAsync<User>(
    q => q.Where(u => u.Age < 18),
    u => new User { Status = "Minor" });

// Bulk update
var updater = EntityUpdater<User>
    .Create()
    .SetProperty(u => u.LastLoginDate, DateTime.UtcNow)
    .SetProperty(u => u.LoginCount, u => u.LoginCount + 1);

await _repository.UpdateAsync(
    q => q.Where(u => u.IsActive),
    updater);

// Delete
await _repository.DeleteAsync<User>(
    q => q.Where(u => !u.IsActive && u.CreatedDate < oldDate));
```

---

## 🔧 Configuration

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net.DependencyInjection;

var services = new ServiceCollection();

// Add core services
services.AddXMediator();                    // CQRS Mediator
services.AddXRequestHandlers();             // Scan for request handlers
services.AddXPipelineRequestHandler();      // Request pipeline

// Add decorators
services.AddXPipelinePreDecorator();        // Pre-request processing
services.AddXPipelinePostDecorator();       // Post-request processing
services.AddXPipelineExceptionDecorator();  // Exception handling
services.AddXPipelineValidationDecorator(); // Validation

// Add event sourcing
services.AddXEventSourcing();
services.AddXAggregateStore();
services.AddXEventStore();
services.AddXPublisher();
services.AddXSubscriber();

// Add repository
services.AddXRepository<MyDbContext>();
```

---

## 📚 Advanced Features

### Pipeline Decorators

Add cross-cutting concerns to your request handlers:

```csharp
// Pre-handler (runs before main handler)
public sealed class LoggingPreHandler<TRequest> 
    : IRequestPreHandler<TRequest>
    where TRequest : class, IRequest
{
    public Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing: {Request}", 
            typeof(TRequest).Name);
        return Task.FromResult(ExecutionResult.Ok().Build());
    }
}

// Post-handler (runs after main handler)
public sealed class CacheInvalidationPostHandler<TRequest> 
    : IRequestPostHandler<TRequest>
    where TRequest : class, IRequest
{
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        ExecutionResult response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccess)
        {
            await _cache.InvalidateAsync("users");
        }
        return response;
    }
}

// Exception handler
public sealed class GlobalExceptionHandler<TRequest> 
    : IRequestExceptionHandler<TRequest>
    where TRequest : class, IRequest
{
    public Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Request failed");
        
        return Task.FromResult(
            ExecutionResult
                .InternalServerError(exception)
                .Build());
    }
}
```

### Event Publishing & Subscription

```csharp
// Publish events
await _publisher.PublishAsync(new UserCreatedEvent(userId, name));

// Subscribe with action
_subscriber.Subscribe<UserCreatedEvent>(evt => 
    Console.WriteLine($"User created: {evt.UserId}"));

// Subscribe with async handler
_subscriber.Subscribe<UserCreatedEvent>(async (evt, ct) =>
{
    await SendWelcomeEmail(evt.UserId, ct);
});

// Subscribe with handler class
public sealed class UserCreatedHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        await SendWelcomeEmail(@event.UserId, cancellationToken);
    }
}

_subscriber.Subscribe(new UserCreatedHandler());
```

### Async Paging

```csharp
using Xpandables.Net.Collections.Generic;

// Create paged enumerable
public async Task<IAsyncPagedEnumerable<Product>> GetProductsAsync(
    int pageSize,
    int pageIndex)
{
    var query = _dbContext.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name);

    return query.ToAsyncPagedEnumerable(pageSize, pageIndex);
}

// Consume paged results
var products = await GetProductsAsync(20, 1);

await foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

// Get pagination info
var pagination = await products.GetPaginationAsync();
Console.WriteLine($"Page {pagination.PageIndex} of {pagination.TotalPages}");
Console.WriteLine($"Total items: {pagination.TotalCount}");
```

---

## 💡 Best Practices

1. **Use ExecutionResult** for all public API boundaries
2. **Prefer Optional** over null checks
3. **Encapsulate business rules** in Specifications
4. **Use CQRS** to separate reads from writes
5. **Apply Event Sourcing** for audit trails and temporal queries
6. **Leverage decorators** for cross-cutting concerns
7. **Keep aggregates small** and focused

---

## 📚 Related Packages

- **Xpandables.Net.AspNetCore** - ASP.NET Core integrations
- **Xpandables.Net.EntityFramework** - EF Core repository implementation
- **Xpandables.Net.SampleApi** - Complete working example

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
