# System.Validation

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Validation.svg)](https://www.nuget.org/packages/Xpandables.Validation)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Specification pattern and validation framework for .NET with DI support.

## Overview

`System.Validation` provides the Specification pattern for encapsulating business rules and a validation framework for validating objects using strongly-typed validators. It can be used standalone or integrated with CQRS pipelines.

Built for .NET 10 with full async support.

## Features

### Specification Pattern
- **`ISpecification<TSource>`** — Specification with expression-based criteria
- **`Specification`** — Static factory methods for creating specifications
- **`SpecificationExtensions`** — Combinators (And, Or, OrElse, Not)

### Validators
- **`IValidator`** — Base validator interface
- **`IValidator<TArgument>`** — Strongly-typed validator
- **`Validator<TArgument>`** — Base validator implementation
- **`EmptyValidator<TArgument>`** — No-op validator
- **`ICompositeValidator<TArgument>`** — Aggregate multiple validators
- **`CompositeValidator<TArgument>`** — Composite validator implementation

### Validator Resolution
- **`IValidatorFactory`** — Create validators by type
- **`ValidatorFactory`** — Default factory implementation
- **`IValidatorProvider`** — Provide validators for types
- **`ValidatorProvider`** — Default provider implementation
- **`IValidatorResolver`** — Resolve validators from DI
- **`ValidatorResolver`** — Default resolver implementation

### Marker Interface
- **`IRequiresValidation`** — Mark types that require validation

## Installation

```bash
dotnet add package Xpandables.Validation
```

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Scans assemblies for sealed IValidator<T> implementations and registers them,
// along with ICompositeValidator<T> and IValidatorResolver for each argument type.
services.AddXValidators(typeof(Program).Assembly);
```

### Create Specifications

```csharp
using System.ComponentModel.DataAnnotations;

// Factory methods
var isActive = Specification.Equal<User, bool>(u => u.IsActive, true);
var isAdult = Specification.GreaterThan<User, int>(u => u.Age, 18);
var hasEmail = Specification.IsNotNull<User, string>(u => u.Email);

// From expression
var customSpec = Specification.FromExpression<Product>(p => p.Price > 0);

// Combine specifications
var validUser = isActive.And(isAdult).And(hasEmail);

// Check if satisfied
if (validUser.IsSatisfiedBy(user))
{
    Console.WriteLine("User meets all criteria");
}

// Use in LINQ queries
var activeAdults = users.AsQueryable().Where(validUser);
```

### Specification Factory Methods

```csharp
// Equality
var isEqual = Specification.Equal<User, string>(u => u.Status, "Active");
var isNotEqual = Specification.NotEqual<User, string>(u => u.Role, "Guest");

// Null checks
var isNull = Specification.IsNull<User, string>(u => u.MiddleName);
var isNotNull = Specification.IsNotNull<User, string>(u => u.Email);

// Comparisons
var greaterThan = Specification.GreaterThan<Product, decimal>(p => p.Price, 10m);
var lessThan = Specification.LessThan<Product, int>(p => p.Stock, 100);

// String operations
var contains = Specification.Contains<User>(u => u.Email, "@");
var startsWith = Specification.StartsWith<User>(u => u.Name, "John");
var endsWith = Specification.EndsWith<User>(u => u.Email, ".com");

// Constants
var alwaysTrue = Specification.True<User>();
var alwaysFalse = Specification.False<User>();
```

### Combining Specifications

```csharp
var spec1 = Specification.Equal<User, bool>(u => u.IsActive, true);
var spec2 = Specification.GreaterThan<User, int>(u => u.Age, 18);

// Extension methods (work on ISpecification<T>)
var andSpec = spec1.And(spec2);
var orSpec = spec1.Or(spec2);
var orElseSpec = spec1.OrElse(spec2); // Short-circuit
var notSpec = spec1.Not();

// Static methods
var allSpec = Specification.All(spec1, spec2);
var anySpec = Specification.Any(spec1, spec2);

// Operator syntax (requires Specification<T> — use FromExpression)
Specification<User> s1 = Specification.FromExpression<User>(u => u.IsActive);
Specification<User> s2 = Specification.FromExpression<User>(u => u.Age > 18);

var combined = s1 & s2; // AND
var either = s1 | s2;   // OR
var negated = !s1;       // NOT
```

### LINQ Integration

```csharp
var isActive = Specification.Equal<User, bool>(u => u.IsActive, true);

// IQueryable (expression tree — database-compatible)
IQueryable<User> activeUsers = dbContext.Users.Where(isActive);

// IEnumerable (compiled predicate — in-memory)
IEnumerable<User> filtered = userList.Where(isActive);
bool anyActive = userList.Any(isActive);
bool allActive = userList.All(isActive);
```

---

## 🛡️ Validators

### Create a Validator

```csharp
using System.ComponentModel.DataAnnotations;

public sealed record CreateUserRequest(string Name, string Email, int Age)
    : IRequiresValidation;

public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateUserRequest instance)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(instance.Name))
            results.Add(new ValidationResult(
                "Name is required", [nameof(instance.Name)]));

        if (!instance.Email.Contains('@'))
            results.Add(new ValidationResult(
                "Invalid email", [nameof(instance.Email)]));

        if (instance.Age < 18)
            results.Add(new ValidationResult(
                "Must be 18+", [nameof(instance.Age)]));

        return results;
    }
}
```

### Use the Validator

```csharp
public class UserService(IValidator<CreateUserRequest> validator)
{
    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var validationResults = validator.Validate(request);

        if (validationResults.Count > 0)
        {
            throw new ValidationException(
                validationResults.First().ErrorMessage);
        }

        // Create user...
    }
}
```

### Async Validation

```csharp
public sealed class UniqueEmailValidator : Validator<CreateUserRequest>
{
    private readonly IUserRepository _repository;

    public UniqueEmailValidator(IUserRepository repository)
        => _repository = repository;

    // Lower order = runs first
    public override int Order => 10;

    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateUserRequest instance) => [];

    public override async ValueTask<IReadOnlyCollection<ValidationResult>>
        ValidateAsync(CreateUserRequest instance)
    {
        var existing = await _repository
            .FindByEmailAsync(instance.Email)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return [new ValidationResult(
                "Email already exists",
                [nameof(instance.Email)])];
        }

        return [];
    }
}
```

### Composite Validator

When multiple validators are registered for the same type,
`CompositeValidator<T>` aggregates them in `Order` sequence:

```csharp
// Registration (AddXValidators does this automatically)
services.AddXValidators(typeof(CreateUserValidator).Assembly);

// Usage — inject ICompositeValidator<T> to run all validators
public class UserService(ICompositeValidator<CreateUserRequest> validator)
{
    public async Task CreateUserAsync(
        CreateUserRequest request,
        CancellationToken ct)
    {
        // Runs CreateUserValidator, then UniqueEmailValidator (by Order)
        var results = await validator
            .ValidateAsync(request)
            .ConfigureAwait(false);

        if (results.Count > 0)
        {
            // Handle validation errors...
        }
    }
}
```

---

## ⚙️ Configuration

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register all validators from assembly
builder.Services.AddXValidators(typeof(CreateUserValidator).Assembly);

// Or register infrastructure separately
builder.Services
    .AddXCompositeValidator()       // ICompositeValidator<T> → CompositeValidator<T>
    .AddXValidatorFactory()         // IValidatorFactory → ValidatorFactory
    .AddXValidatorProvider();       // IValidatorProvider → ValidatorProvider
```

---

## 🌍 Real-World Examples

### Validator with Specifications

```csharp
public sealed record CreateProductRequest(
    string Name, decimal Price, int Stock) : IRequiresValidation;

public sealed class CreateProductValidator : Validator<CreateProductRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateProductRequest instance)
    {
        var results = new List<ValidationResult>();

        var nameSpec = Specification.IsNotNull<CreateProductRequest, string>(
            p => p.Name);
        var priceSpec = Specification.GreaterThan<CreateProductRequest, decimal>(
            p => p.Price, 0m);
        var stockSpec = Specification.GreaterThan<CreateProductRequest, int>(
            p => p.Stock, 0);

        if (!nameSpec.IsSatisfiedBy(instance))
            results.Add(new ValidationResult(
                "Name is required", [nameof(instance.Name)]));

        if (!priceSpec.IsSatisfiedBy(instance))
            results.Add(new ValidationResult(
                "Price must be greater than zero", [nameof(instance.Price)]));

        if (!stockSpec.IsSatisfiedBy(instance))
            results.Add(new ValidationResult(
                "Stock must be greater than zero", [nameof(instance.Stock)]));

        return results;
    }
}
```

### Complex Business Rules

```csharp
public static class DiscountEligibility
{
    // Premium customer with high order value
    public static ISpecification<Order> PremiumDiscount =>
        Specification.Equal<Order, string>(o => o.CustomerType, "Premium")
            .And(Specification.GreaterThan<Order, decimal>(o => o.TotalAmount, 1000m));

    // First-time customer discount
    public static ISpecification<Order> FirstTimeDiscount =>
        Specification.Equal<Order, int>(o => o.CustomerOrderCount, 0);

    // Any discount applies
    public static ISpecification<Order> AnyDiscount =>
        Specification.Any(PremiumDiscount, FirstTimeDiscount);
}

// Usage
if (DiscountEligibility.PremiumDiscount.IsSatisfiedBy(order))
{
    order.ApplyDiscount(0.15m); // 15% premium discount
}
```

---

## Core Types

| Type | Description |
|------|-------------|
| `ISpecification<T>` | Specification with expression |
| `Specification` | Static factory and combinator methods |
| `IValidator<T>` | Strongly-typed validator |
| `Validator<T>` | Base validator with sync/async support |
| `ICompositeValidator<T>` | Aggregates multiple validators |
| `IRequiresValidation` | Marker for types needing validation |
| `IValidatorFactory` | Creates validators by type |

---

## ✅ Best Practices

1. **Create reusable specifications** — Define common rules as static specifications
2. **Combine specifications** — Use `And`, `Or`, `Not` for complex rules
3. **Use factory methods** — Prefer `Equal`, `GreaterThan`, `Contains` over raw expressions
4. **Implement `IRequiresValidation`** — Mark request types for automatic pipeline validation
5. **Keep validators focused** — One validator per request type
6. **Use async validation** — Override `ValidateAsync` for database lookups or external calls
7. **Use `Order` property** — Control validator execution sequence in composite validators

---

## 📚 Related Packages

- **Xpandables.Primitives** — Core primitives and utilities
- **Xpandables.Results** — Result types with validation integration
- **Xpandables.Results.Pipelines** — Automatic validation in request pipelines

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
