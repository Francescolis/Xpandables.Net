# ✅ Xpandables.Results

[![NuGet](https://img.shields.io/badge/NuGet-10.0.1-blue.svg)](https://www.nuget.org/packages/Xpandables.Results)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Operation Results & Request Contracts** - Railway-oriented programming with HTTP-aware result types and CQRS request/handler contracts.

---

## 📋 Overview

`Xpandables.Results` provides a robust implementation of the Result pattern for representing operation outcomes, along with a request/handler infrastructure for CQRS-style command and query handling. Built for .NET 10, it enables functional error handling and clean separation of concerns.

### ✨ Key Features

- ✅ **Result & Result&lt;T&gt;** - Represent success/failure outcomes with HTTP status codes
- 🏗️ **SuccessResult / FailureResult** - Typed result classes for clear intent
- 🔧 **Fluent Builders** - SuccessResultBuilder and FailureResultBuilder for composing results
- 📡 **IRequest / IRequestHandler** - CQRS-style request/handler pattern
- 🔄 **Pipeline Infrastructure** - IPipelineDecorator and IPipelineRequestHandler
- 🌊 **IStreamRequestHandler** - Support for streaming results
- 📦 **HTTP Integration** - Built-in HTTP status codes and headers support

---

## 📥 Installation

```bash
dotnet add package Xpandables.Results
```

---

## 🚀 Quick Start

### Basic Result Usage

```csharp
using System.Results;

// Create a success result
Result success = Result.Success();

// Create a success result with value
Result<User> userResult = Result.Success(new User { Name = "John" });

// Create a failure result
Result failure = Result.Failure("email", "Email is required");

// Create a failure with HTTP status
Result notFound = Result.NotFound("userId", "User not found");
```

### Checking Results

```csharp
Result<User> result = await GetUserAsync(userId);

if (result.IsSuccess)
{
    Console.WriteLine($"Found user: {result.Value.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Title}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Values)}");
    }
}
```

---

## 🧩 Core Concepts

### Success Results

```csharp
// HTTP 200 OK
SuccessResult ok = Result.Success();

// HTTP 201 Created
SuccessResult created = Result.Created();

// HTTP 204 No Content  
SuccessResult noContent = Result.NoContent();

// HTTP 202 Accepted
SuccessResult accepted = Result.Accepted();

// With value and location
SuccessResult<User> userCreated = Result.Created<User>()
    .WithValue(newUser)
    .WithLocation($"/api/users/{newUser.Id}");
```

### Failure Results

```csharp
// HTTP 400 Bad Request (default)
FailureResult badRequest = Result.Failure();

// With error details
FailureResult validation = Result.Failure("email", "Invalid email format");

// HTTP 404 Not Found
FailureResult notFound = Result.NotFound("userId", "User not found");

// HTTP 409 Conflict
FailureResult conflict = Result.Conflict("email", "Email already exists");

// HTTP 401 Unauthorized
FailureResult unauthorized = Result.Unauthorized();

// HTTP 403 Forbidden
FailureResult forbidden = Result.Forbidden();

// HTTP 500 Internal Server Error
FailureResult serverError = Result.InternalServerError(exception);
```

### Fluent Builder Pattern

```csharp
// Build complex success results
SuccessResult<Order> result = Result.Created<Order>()
    .WithValue(order)
    .WithLocation($"/api/orders/{order.Id}")
    .WithHeader("X-Order-Number", order.Number);

// Build complex failure results
FailureResult result = Result.BadRequest()
    .WithError("name", "Name is required")
    .WithError("email", "Email format is invalid")
    .WithDetail("Please correct the validation errors")
    .WithException(validationException);
```

---

## 📡 Request/Handler Pattern (CQRS)

### Define Requests

```csharp
using System.Results.Requests;

// Query - returns data
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<User>;

// Command - performs action
public sealed record CreateUserCommand(string Name, string Email) : IRequest<User>;

// Command without return value
public sealed record DeleteUserCommand(Guid UserId) : IRequest;

// Stream request - returns multiple items
public sealed record GetAllUsersQuery : IStreamRequest<User>;
```

### Implement Handlers

```csharp
// Handler for query with response
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

// Handler for command without response value
public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _repository;

    public async Task<Result> HandleAsync(
        DeleteUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(request.UserId, cancellationToken);

        if (!deleted)
        {
            return Result.NotFound("userId", "User not found");
        }

        return Result.NoContent();
    }
}
```

---

## 🔄 Pipeline Infrastructure

### Pre-Handler (Before Execution)

```csharp
using System.Results.Requests;

public sealed class LoggingPreHandler<TRequest> : IRequestPreHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly ILogger<LoggingPreHandler<TRequest>> _logger;

    public Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing {RequestType}", typeof(TRequest).Name);
        return Task.FromResult(Result.Success());
    }
}
```

### Post-Handler (After Execution)

```csharp
public sealed class AuditPostHandler<TRequest> : IRequestPostHandler<TRequest>
    where TRequest : class, IRequest
{
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

### Exception Handler

```csharp
public sealed class GlobalExceptionHandler<TRequest> : IRequestExceptionHandler<TRequest>
    where TRequest : class, IRequest
{
    public Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Request {RequestType} failed", typeof(TRequest).Name);
        return Task.FromResult(Result.InternalServerError(exception).Build());
    }
}
```

---

## ⚙️ Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register request handlers from assembly
builder.Services.AddXRequestHandlers(typeof(Program).Assembly);
```

---

## ✅ Best Practices

1. **Use specific status codes** - NotFound, Conflict, Unauthorized for clear semantics
2. **Include error details** - Always provide key and message for debugging
3. **Keep handlers focused** - One handler per request type
4. **Use pipeline handlers** - For cross-cutting concerns like logging, validation
5. **Convert exceptions to results** - Don't let exceptions bubble up
6. **Use builders** - For complex result construction

---

## 📚 Related Packages

- **System.Results.Pipelines** - Pre-built pipeline decorators
- **System.Results.Tasks** - Mediator implementation
- **AspNetCore.Net** - ASP.NET Core integration
- **System.Optionals** - Optional value handling

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
