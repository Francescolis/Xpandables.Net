# ✅ Xpandables.Net.ExecutionResults

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Railway-Oriented Programming for .NET** - A robust result type for standardized error handling and HTTP-aware operation outcomes.

---

## 📋 Overview

`Xpandables.Net.ExecutionResults` provides a comprehensive result pattern implementation that eliminates the need for exceptions in expected error scenarios. It encapsulates operation outcomes with status codes, error details, headers, and metadata—perfect for building resilient APIs and applications.

### 🎯 Key Features

- 🌐 **HTTP-Aware Results** - Built-in `HttpStatusCode` support
- 📦 **Generic and Non-Generic** - `ExecutionResult` and `ExecutionResult<T>`
- 📝 **Rich Metadata** - Headers, extensions, errors, and location support
- 🔗 **Fluent API** - Builder pattern for result construction
- 🔄 **Chainable Operations** - Railway-oriented programming support
- 📄 **JSON Serialization** - Full System.Text.Json support
- ⚡ **Performance Optimized** - Record types with value semantics

---

## 🚀 Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.ExecutionResults
```

### Basic Usage

```csharp
using Xpandables.Net.ExecutionResults;

// Success result
ExecutionResult<User> successResult = ExecutionResult
    .Success(new User { Id = 1, Name = "John" });

// Failure result
ExecutionResult failureResult = ExecutionResult
    .BadRequest()
    .WithTitle("Validation Failed")
    .WithDetail("Email is required")
    .WithError("Email", "The email field is required");

// Not Found result
ExecutionResult<User> notFoundResult = ExecutionResult
    .NotFound<User>()
    .WithTitle("User Not Found")
    .WithDetail($"User with ID {userId} does not exist");
```

---

## 🏗️ Core Concepts

### ExecutionResult Types

```csharp
// Non-generic result (no value)
ExecutionResult result = ExecutionResult.Success();

// Generic result with typed value
ExecutionResult<Order> orderResult = ExecutionResult.Success(order);
```

### HTTP Status Code Helpers

```csharp
// Success responses (2xx)
ExecutionResult.Ok();                          // 200
ExecutionResult.Created(resource);             // 201
ExecutionResult.Accepted();                    // 202
ExecutionResult.NoContent();                   // 204

// Client errors (4xx)
ExecutionResult.BadRequest();                  // 400
ExecutionResult.Unauthorized();                // 401
ExecutionResult.Forbidden();                   // 403
ExecutionResult.NotFound();                    // 404
ExecutionResult.Conflict();                    // 409
ExecutionResult.UnprocessableEntity();         // 422

// Server errors (5xx)
ExecutionResult.InternalServerError();         // 500
ExecutionResult.ServiceUnavailable();          // 503
```

---

## 💎 Advanced Examples

### Example 1: Building a Complex Result

```csharp
public async Task<ExecutionResult<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    var order = new Order 
    { 
        Id = Guid.NewGuid(), 
        Total = request.Amount 
    };

    await _repository.SaveAsync(order);

    return ExecutionResult
        .Created(order)
        .WithLocation(new Uri($"/api/orders/{order.Id}", UriKind.Relative))
        .WithHeader("X-Order-Id", order.Id.ToString())
        .WithExtension("processingTime", "125ms");
}
```

### Example 2: Validation Errors

```csharp
public ExecutionResult<User> ValidateUser(User user)
{
    var errors = new List<(string Key, string Message)>();

    if (string.IsNullOrEmpty(user.Email))
        errors.Add(("Email", "Email is required"));

    if (user.Age < 18)
        errors.Add(("Age", "Must be 18 or older"));

    if (errors.Any())
    {
        var result = ExecutionResult
            .BadRequest<User>()
            .WithTitle("Validation Failed");

        foreach (var (key, message) in errors)
        {
            result = result.WithError(key, message);
        }

        return result;
    }

    return ExecutionResult.Success(user);
}
```

### Example 3: Exception Handling

```csharp
public async Task<ExecutionResult<Data>> GetDataAsync(string id)
{
    try
    {
        var data = await _service.FetchDataAsync(id);
        return ExecutionResult.Success(data);
    }
    catch (NotFoundException ex)
    {
        return ExecutionResult
            .NotFound<Data>()
            .WithTitle("Resource Not Found")
            .WithDetail(ex.Message)
            .WithException(ex);
    }
    catch (Exception ex)
    {
        return ExecutionResult
            .InternalServerError<Data>()
            .WithTitle("Internal Error")
            .WithDetail("An unexpected error occurred")
            .WithException(ex);
    }
}
```

---

## 💡 Best Practices

1. **Use Specific Status Codes**: Choose the most appropriate HTTP status code
   ```csharp
   // ❌ Don't
   return ExecutionResult.Failure();
   
   // ✔️ Do
   return ExecutionResult.NotFound();
   ```

2. **Provide Meaningful Messages**: Always include title and detail
   ```csharp
   return ExecutionResult
       .BadRequest()
       .WithTitle("Invalid Request")
       .WithDetail("The email format is invalid");
   ```

3. **Use Generic Results for Values**: Return typed results when returning data
   ```csharp
   // ? Type-safe
   ExecutionResult<User> GetUser(Guid id);
   ```

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
