# ✅ System.Validation

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Validation & Specifications** - Specification pattern for business rules, rule validators, and reusable validation logic with LINQ integration.

---

## 🎯 Overview

`System.Validation` provides the Specification pattern for encapsulating business rules and validation logic. It includes composable specifications with LINQ support, rule validators, and integration with request pipelines for automatic validation.

### ✨ Key Features

- 📋 **ISpecification<T>** - Specification pattern with expression-based criteria
- 🔗 **Combinators** - And, Or, Not, All, Any for composing specifications
- ✅ **IValidator<T> & DefaultValidator<T>** - Strongly-typed validation with synchronous and async helpers
- 🧩 **CompositeValidator<T>** - Aggregate multiple validators for the same argument type
- 🏭 **Specification Factory Methods** - Equal, NotEqual, Contains, GreaterThan, LessThan, etc.
- 🔍 **LINQ Integration** - Use specifications in Where clauses and repositories
- 🧠 **ValidatorFactory / ValidatorProvider / ValidatorResolver** - Resolve validators dynamically through DI
- 🧱 **AddXValidator / AddXValidators** - Register built-in and custom validators from assemblies
- 🚀 **IRequiresValidation** - Marker interface for automatic pipeline validation

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package System.Validation
```

### Basic Specification Usage

```csharp
using System.ComponentModel.DataAnnotations;

// Create specifications using factory methods
var isActive = Specification.Equal<User, bool>(u => u.IsActive, true);
var isAdult = Specification.GreaterThan<User, int>(u => u.Age, 18);
var hasEmail = Specification.IsNotNull<User, string>(u => u.Email);

// Combine specifications
var validUser = isActive.And(isAdult).And(hasEmail);

// Check if satisfied
if (validUser.IsSatisfiedBy(user))
{
    Console.WriteLine("User meets all criteria");
}

// Use in LINQ queries
var activeAdults = users.Where(validUser.Expression.Compile());
```

---

## 🧩 Core Concepts

### Creating Specifications

```csharp
using System.ComponentModel.DataAnnotations;

// From expression
var customSpec = Specification.FromExpression<Product>(p => p.Price > 0 && p.Stock > 10);

// Factory methods
var isEqual = Specification.Equal<User, string>(u => u.Status, "Active");
var isNotEqual = Specification.NotEqual<User, string>(u => u.Role, "Guest");
var isNull = Specification.IsNull<User, string>(u => u.MiddleName);
var isNotNull = Specification.IsNotNull<User, string>(u => u.Email);
var greaterThan = Specification.GreaterThan<Product, decimal>(p => p.Price, 10m);
var lessThan = Specification.LessThan<Product, int>(p => p.Stock, 100);
var contains = Specification.Contains<User>(u => u.Email, "@");
var startsWith = Specification.StartsWith<User>(u => u.Name, "John");
var endsWith = Specification.EndsWith<User>(u => u.Email, ".com");

// Always true/false
var alwaysTrue = Specification.True<User>();
var alwaysFalse = Specification.False<User>();
```

### Combining Specifications

```csharp
// Logical AND
var spec1 = Specification.Equal<User, bool>(u => u.IsActive, true);
var spec2 = Specification.GreaterThan<User, int>(u => u.Age, 18);

var andSpec = spec1.And(spec2);
var andAlsoSpec = spec1.AndAlso(spec2); // Short-circuit evaluation

// Logical OR
var orSpec = spec1.Or(spec2);
var orElseSpec = spec1.OrElse(spec2); // Short-circuit evaluation

// Logical NOT
var notSpec = spec1.Not();

// Combine multiple with All (AND) or Any (OR)
var allSpec = Specification.All(spec1, spec2, notSpec);
var anySpec = Specification.Any(spec1, spec2);

// Operator syntax
var combined = spec1 & spec2; // AND
var either = spec1 | spec2;   // OR
var negated = !spec1;         // NOT
```

### Using with LINQ

```csharp
// In-memory collections
var activeUsers = users.Where(isActive.Expression.Compile());

// With Entity Framework (expression is preserved)
var query = dbContext.Users.Where(isActive.Expression);

// Combine with repository pattern
var spec = Specification.Equal<User, bool>(u => u.IsActive, true)
    .And(Specification.GreaterThan<User, int>(u => u.Age, 21));

var results = await repository
    .FetchAsync<User, User>(q => q.Where(spec.Expression))
    .ToListAsync();
```

---

## ✔️ Validators

### Define a Validator

```csharp
using System.ComponentModel.DataAnnotations;

public sealed record CreateUserRequest(string Name, string Email, int Age) : IRequiresValidation;

public sealed class CreateUserValidator : IValidator<CreateUserRequest>
{
    public IReadOnlyCollection<ValidationResult> Validate(CreateUserRequest instance)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(instance.Name))
        {
            results.Add(new ValidationResult("Name is required", [nameof(instance.Name)]));
        }

        if (string.IsNullOrWhiteSpace(instance.Email) || !instance.Email.Contains('@'))
        {
            results.Add(new ValidationResult("Valid email is required", [nameof(instance.Email)]));
        }

        if (instance.Age < 18)
        {
            results.Add(new ValidationResult("Must be 18 or older", [nameof(instance.Age)]));
        }

        return results;
    }

    public ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(CreateUserRequest instance)
        => new(Validate(instance));
}
```

### Using Specifications in Validators

```csharp
public sealed class ProductValidator : IValidator<CreateProductRequest>
{
    public IReadOnlyCollection<ValidationResult> Validate(CreateProductRequest instance)
    {
        var results = new List<ValidationResult>();

        var nameSpec = Specification.IsNotNull<CreateProductRequest, string>(p => p.Name);
        var priceSpec = Specification.GreaterThan<CreateProductRequest, decimal>(p => p.Price, 0);
        var stockSpec = Specification.GreaterThan<CreateProductRequest, int>(p => p.Stock, 0);

        if (!nameSpec.IsSatisfiedBy(instance))
        {
            results.Add(new ValidationResult("Name is required", [nameof(instance.Name)]));
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
public sealed class OrderValidator : RuleValidator<CreateOrderRequest>
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

- **System.Primitives** - Core primitives and utilities
- **System.Results** - Result types with validation integration
- **System.Results.Pipelines** - Automatic validation in request pipelines

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
