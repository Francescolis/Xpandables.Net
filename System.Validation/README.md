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
- **`SpecificationExtensions`** — Combinators (And, Or, Not, All, Any)

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

// Use in LINQ
var activeAdults = users.Where(validUser.Expression.Compile());
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
var greaterOrEqual = Specification.GreaterThanOrEqual<User, int>(u => u.Age, 18);
var lessOrEqual = Specification.LessThanOrEqual<Product, decimal>(p => p.Price, 100m);

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

// Logical AND
var andSpec = spec1.And(spec2);
var andAlsoSpec = spec1.AndAlso(spec2); // Short-circuit

// Logical OR
var orSpec = spec1.Or(spec2);
var orElseSpec = spec1.OrElse(spec2); // Short-circuit

// Logical NOT
var notSpec = spec1.Not();

// Multiple with All (AND) or Any (OR)
var allSpec = Specification.All(spec1, spec2);
var anySpec = Specification.Any(spec1, spec2);

// Operator syntax
var combined = spec1 & spec2; // AND
var either = spec1 | spec2;   // OR
var negated = !spec1;         // NOT
```

### Create a Validator

```csharp
using System.ComponentModel.DataAnnotations;

public sealed record CreateUserRequest(string Name, string Email, int Age) 
    : IRequiresValidation;

public sealed class CreateUserValidator : IValidator<CreateUserRequest>
{
    public IReadOnlyCollection<ValidationResult> Validate(CreateUserRequest instance)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(instance.Name))
            results.Add(new ValidationResult("Name is required", [nameof(instance.Name)]));

        if (!instance.Email.Contains('@'))
            results.Add(new ValidationResult("Invalid email", [nameof(instance.Email)]));

        if (instance.Age < 18)
            results.Add(new ValidationResult("Must be 18+", [nameof(instance.Age)]));

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
            throw new ValidationException(validationResults.First().ErrorMessage);
        }

        // Create user...
    }
}
```

### Composite Validator

```csharp
public class ComplexValidator : ICompositeValidator<CreateUserRequest>
{
    private readonly IValidator<CreateUserRequest>[] _validators;

    public ComplexValidator(IEnumerable<IValidator<CreateUserRequest>> validators)
        => _validators = validators.ToArray();

    public IReadOnlyCollection<ValidationResult> Validate(CreateUserRequest instance)
        => _validators.SelectMany(v => v.Validate(instance)).ToList();
}
```

## Core Types

| Type | Description |
|------|-------------|
| `ISpecification<T>` | Specification with expression |
| `Specification` | Factory methods |
| `IValidator<T>` | Strongly-typed validator |
| `IRequiresValidation` | Validation marker |
| `IValidatorFactory` | Validator creation |
| `ICompositeValidator<T>` | Multiple validators |

## License

Apache License 2.0
        }

        if (!priceSpec.IsSatisfiedBy(instance))
        {
            results.Add(new ValidationResult("Price must be greater than zero", [nameof(instance.Price)]));
        }

        if (!stockSpec.IsSatisfiedBy(instance))
        {
            results.Add(new ValidationResult("Stock must be greater than zero", [nameof(instance.Stock)]));
        }

        return results;
    }

    public ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(CreateProductRequest instance)
        => new(Validate(instance));
}
```

### Async Validation

```csharp
public sealed class UniqueEmailValidator : IValidator<CreateUserRequest>
{
    private readonly IUserRepository _repository;

    public UniqueEmailValidator(IUserRepository repository)
        => _repository = repository;

    public async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(
        CreateUserRequest instance)
    {
        var results = new List<ValidationResult>();

        var existingUser = await _repository.FindByEmailAsync(instance.Email).ConfigureAwait(false);
        if (existingUser is not null)
        {
            results.Add(new ValidationResult("Email already exists", [nameof(instance.Email)]));
        }

        return results;
    }

    public IReadOnlyCollection<ValidationResult> Validate(CreateUserRequest instance)
        => ValidateAsync(instance).GetAwaiter().GetResult();
}
```

---

## ⚙️ Configuration

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddXValidator()
    .AddXValidatorFactory()
    .AddXValidatorProvider()
    .AddXValidators(typeof(CreateUserValidator).Assembly);
```

---

## 🌍 Real-World Examples

### Order Validation

```csharp
public sealed class OrderValidator : Validator<CreateOrderRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(CreateOrderRequest instance)
    {
        var results = new List<ValidationResult>();

        // Customer validation
        var hasCustomer = Specification.IsNotNull<CreateOrderRequest, Guid?>(o => o.CustomerId);
        if (!hasCustomer.IsSatisfiedBy(instance))
        {
            results.Add(new ValidationResult("Customer is required", [nameof(instance.CustomerId)]));
        }

        // Items validation
        if (instance.Items is null || instance.Items.Count == 0)
        {
            results.Add(new ValidationResult("At least one item is required", [nameof(instance.Items)]));
        }
        else
        {
            var validItemSpec = Specification.GreaterThan<OrderItem, int>(i => i.Quantity, 0)
                .And(Specification.GreaterThan<OrderItem, decimal>(i => i.UnitPrice, 0));

            for (int i = 0; i < instance.Items.Count; i++)
            {
                if (!validItemSpec.IsSatisfiedBy(instance.Items[i]))
                {
                    results.Add(new ValidationResult(
                        $"Item {i + 1} must have quantity and price greater than zero",
                        [$"Items[{i}]"]));
                }
            }
        }

        return results;
    }
}
```

### Complex Business Rules

```csharp
public class DiscountEligibilitySpec
{
    // Premium customer with high order value
    public static ISpecification<Order> PremiumDiscount =>
        Specification.Equal<Order, CustomerType>(o => o.Customer.Type, CustomerType.Premium)
            .And(Specification.GreaterThan<Order, decimal>(o => o.TotalAmount, 1000m));

    // First-time customer discount
    public static ISpecification<Order> FirstTimeDiscount =>
        Specification.Equal<Order, int>(o => o.Customer.OrderCount, 0);

    // Seasonal discount (example: December)
    public static ISpecification<Order> SeasonalDiscount =>
        Specification.Equal<Order, int>(o => o.OrderDate.Month, 12);

    // Any discount applies
    public static ISpecification<Order> AnyDiscount =>
        Specification.Any(PremiumDiscount, FirstTimeDiscount, SeasonalDiscount);
}

// Usage
if (DiscountEligibilitySpec.PremiumDiscount.IsSatisfiedBy(order))
{
    order.ApplyDiscount(0.15m); // 15% premium discount
}
```

---

## ✅ Best Practices

1. **Create reusable specifications** - Define common business rules as static specifications
2. **Combine specifications** - Use And, Or, Not for complex rules
3. **Use factory methods** - Prefer Equal, GreaterThan, Contains over raw expressions
4. **Implement IRequiresValidation** - Mark requests for automatic pipeline validation
5. **Keep validators focused** - One validator per request type
6. **Use async validation** - For database lookups or external service calls

---

## 📚 Related Packages

- **Xpandables.Primitives** - Core primitives and utilities
- **Xpandables.Results** - Result types with validation integration
- **Xpandables.Results.Pipelines** - Automatic validation in request pipelines

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
