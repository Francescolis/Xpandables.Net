# System.Validation

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Validation.svg)](https://www.nuget.org/packages/Xpandables.Validation)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Validation.svg)](https://www.nuget.org/packages/Xpandables.Validation)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Specification pattern and validation framework for .NET with dependency injection support.

## 📖 Overview

`System.Validation` (NuGet: **Xpandables.Validation**) provides the Specification pattern for encapsulating business rules and a validation framework for validating objects using strongly-typed validators. All types live in the `System.ComponentModel.DataAnnotations` namespace.

Built for **.NET 10** and **C# 14** with full async support.

## ✨ Features

### 🔍 Specification Pattern

| Type | File | Description |
|------|------|-------------|
| `ISpecification<TSource>` | `ISpecification.cs` | Contract with `Expression` and `IsSatisfiedBy` |
| `Specification` | `Specification.cs` | Abstract record — static factory & combinator methods |
| `Specification<TSource>` | `Specification.cs` | Concrete record with operator overloads (`&`, `\|`, `!`) |
| `SpecificationExtensions` | `SpecificationExtensions.cs` | C# 14 extension members — `And`, `Or`, `OrElse`, `Not`, `ToSpecification`, LINQ overloads |

### 🛡️ Validators

| Type | File | Description |
|------|------|-------------|
| `IValidator` | `IValidator.cs` | Base interface with `Order`, `Validate`, and default `ValidateAsync` |
| `IValidator<TArgument>` | `IValidator.cs` | Strongly-typed generic validator (`TArgument : class, IRequiresValidation`) |
| `Validator<TArgument>` | `Validator.cs` | Abstract base class — override `Validate` (sync) and optionally `ValidateAsync` |
| `EmptyValidator<TArgument>` | `EmptyValidator.cs` | Sealed no-op validator — always returns an empty collection |
| `ICompositeValidator<TArgument>` | `ICompositeValidator.cs` | Marker extending `IValidator<TArgument>` for composite aggregation |
| `CompositeValidator<TArgument>` | `CompositeValidator.cs` | Sealed — iterates all registered validators in `Order` sequence |

### 🔧 Validator Resolution

| Type | File | Description |
|------|------|-------------|
| `IValidatorFactory` | `IValidatorFactory.cs` | Creates validators by `Type` or generic `TArgument` |
| `ValidatorFactory` | `ValidatorFactory.cs` | Default factory — resolves via `IValidatorResolver` collection and `IServiceProvider` |
| `IValidatorProvider` | `IValidatorProvider.cs` | Retrieves validators by type with `IRequiresValidation` enforcement |
| `ValidatorProvider` | `ValidatorProvider.cs` | Default provider — delegates to `IValidatorFactory` |
| `IValidatorResolver` | `IValidatorResolver.cs` | Maps a `TargetType` to a validator resolved from DI |
| `ValidatorResolver<TArgument>` | `ValidatorResolver.cs` | Sealed — resolves `IValidator<TArgument>` from the container, removes duplicates |

### 🏷️ Marker Interface

| Type | File | Description |
|------|------|-------------|
| `IRequiresValidation` | `IRequiresValidation.cs` | Empty marker — types implementing this are eligible for validation |

---

## 📦 Installation

```bash
dotnet add package Xpandables.Validation
```

**Dependency:** `Microsoft.Extensions.DependencyInjection`

---

## 🚀 Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

// Scans assemblies for sealed IValidator<T> implementations and registers:
//   • IValidator<TArgument>         → concrete validator (Transient)
//   • ICompositeValidator<TArgument>→ CompositeValidator<TArgument> (Transient)
//   • IValidatorResolver            → ValidatorResolver<TArgument> (Singleton)
services.AddXValidators(typeof(Program).Assembly);
```

### Create a Specification

```csharp
using System.ComponentModel.DataAnnotations;

// Factory methods
var isActive = Specification.Equal<User, bool>(u => u.IsActive, true);
var isAdult  = Specification.GreaterThan<User, int>(u => u.Age, 18);
var hasEmail = Specification.IsNotNull<User, string>(u => u.Email);

// From expression
var custom = Specification.FromExpression<Product>(p => p.Price > 0);

// Combine specifications (extension methods on ISpecification<T>)
var validUser = isActive.And(isAdult).And(hasEmail);

// Evaluate
if (validUser.IsSatisfiedBy(user))
{
    Console.WriteLine("User meets all criteria");
}

// Use in LINQ queries (extension methods on IQueryable<T>)
var activeAdults = users.AsQueryable().Where(validUser);
```

---

## 🔍 Specification Factory Methods

All methods are static on the `Specification` abstract record.

```csharp
// Equality
Specification.Equal<User, string>(u => u.Status, "Active");
Specification.NotEqual<User, string>(u => u.Role, "Guest");

// Null checks (TValue : class constraint)
Specification.IsNull<User, string>(u => u.MiddleName);
Specification.IsNotNull<User, string>(u => u.Email);

// Comparisons (TValue : IComparable<TValue>)
Specification.GreaterThan<Product, decimal>(p => p.Price, 10m);
Specification.LessThan<Product, int>(p => p.Stock, 100);

// String operations (with optional StringComparison, default Ordinal)
Specification.Contains<User>(u => u.Email, "@");
Specification.StartsWith<User>(u => u.Name, "John");
Specification.EndsWith<User>(u => u.Email, ".com");

// Constants
Specification.True<User>();
Specification.False<User>();
```

## 🔗 Combining Specifications

```csharp
var spec1 = Specification.Equal<User, bool>(u => u.IsActive, true);
var spec2 = Specification.GreaterThan<User, int>(u => u.Age, 18);

// Extension methods on ISpecification<T> (C# 14 extension members)
var andSpec    = spec1.And(spec2);       // Logical AND
var orSpec     = spec1.Or(spec2);        // Logical OR
var orElseSpec = spec1.OrElse(spec2);    // OR with short-circuit
var notSpec    = spec1.Not();            // Logical NOT

// Static aggregation methods
var allSpec = Specification.All(spec1, spec2);   // AND all
var anySpec = Specification.Any(spec1, spec2);   // OR all

// Collection extension methods
IEnumerable<ISpecification<User>> specs = [spec1, spec2];
var allOf = specs.AllOf();   // AND all
var anyOf = specs.AnyOf();   // OR all

// Operator syntax (requires Specification<T>)
Specification<User> s1 = Specification.FromExpression<User>(u => u.IsActive);
Specification<User> s2 = Specification.FromExpression<User>(u => u.Age > 18);

var combined = s1 & s2;   // AND
var either   = s1 | s2;   // OR
var negated  = !s1;        // NOT
```

## 📊 LINQ Integration

Extension methods on `IQueryable<T>` and `IEnumerable<T>` accept `ISpecification<T>` directly.

```csharp
var isActive = Specification.Equal<User, bool>(u => u.IsActive, true);

// IQueryable — expression tree, database-compatible
IQueryable<User> activeUsers = dbContext.Users.Where(isActive);

// IEnumerable — compiled predicate, in-memory
IEnumerable<User> filtered = userList.Where(isActive);
bool anyActive   = userList.Any(isActive);
bool allActive   = userList.All(isActive);
User first       = userList.First(isActive);
User? firstOrDef = userList.FirstOrDefault(isActive);
User single      = userList.Single(isActive);
User? singleDef  = userList.SingleOrDefault(isActive);
int  count       = userList.Count(isActive);
```

### Expression Conversion

```csharp
// Expression → Specification
Expression<Func<User, bool>> expr = u => u.IsActive;
ISpecification<User> spec = expr.ToSpecification();

// Specification → Expression
Expression<Func<User, bool>> back = (Expression<Func<User, bool>>)s1;

// Specification → Func (implicit)
Func<User, bool> func = s1;
```

---

## 🛡️ Validators

### Create a Validator

Validators must be **sealed classes** inheriting `Validator<TArgument>`. The argument type must implement `IRequiresValidation`.

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

### Inject and Use

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

Override `ValidateAsync` for I/O-bound checks. The base `Validator<T>.ValidateAsync` delegates to `Validate` by default.

```csharp
public sealed class UniqueEmailValidator : Validator<CreateUserRequest>
{
    private readonly IUserRepository _repository;

    public UniqueEmailValidator(IUserRepository repository)
        => _repository = repository;

    public override int Order => 10; // Higher = runs later

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

`CompositeValidator<T>` aggregates all registered `IValidator<T>` implementations, executing them in `Order` sequence and collecting all `ValidationResult` entries.

```csharp
// AddXValidators registers ICompositeValidator<T> automatically
services.AddXValidators(typeof(CreateUserValidator).Assembly);

// Inject ICompositeValidator<T> to run all validators
public class UserService(ICompositeValidator<CreateUserRequest> validator)
{
    public async Task CreateUserAsync(CreateUserRequest request)
    {
        // Runs CreateUserValidator (Order=0), then UniqueEmailValidator (Order=10)
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

## ⚙️ Dependency Injection

All registration methods are C# 14 extension members on `IServiceCollection` in the `Microsoft.Extensions.DependencyInjection` namespace (file: `DependencyInjection/IValiadatorExtensions.cs`).

```csharp
using Microsoft.Extensions.DependencyInjection;

// Assembly scanning — registers validators, composite validators, and resolvers
services.AddXValidators(typeof(CreateUserValidator).Assembly);

// Or register infrastructure individually
services
    .AddXCompositeValidator()       // ICompositeValidator<T> → CompositeValidator<T> (Transient)
    .AddXValidatorFactory()         // IValidatorFactory → ValidatorFactory (Singleton)
    .AddXValidatorProvider();       // IValidatorProvider → ValidatorProvider (Scoped)

// Custom factory / provider overloads
services.AddXValidatorFactory<MyCustomFactory>();
services.AddXValidatorFactory(myFactoryInstance);
services.AddXValidatorProvider<MyCustomProvider>();
services.AddXValidatorProvider(myProviderInstance);
```

### What `AddXValidators` Registers

For each **sealed** `IValidator<TArgument>` found in the scanned assemblies:

| Service | Implementation | Lifetime |
|---------|---------------|----------|
| `IValidator<TArgument>` | Concrete validator class | Transient |
| `ICompositeValidator<TArgument>` | `CompositeValidator<TArgument>` | Transient |
| `IValidatorResolver` | `ValidatorResolver<TArgument>` | Singleton |

---

## 🌍 Examples

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
    public static ISpecification<Order> PremiumDiscount =>
        Specification.Equal<Order, string>(o => o.CustomerType, "Premium")
            .And(Specification.GreaterThan<Order, decimal>(
                o => o.TotalAmount, 1000m));

    public static ISpecification<Order> FirstTimeDiscount =>
        Specification.Equal<Order, int>(o => o.CustomerOrderCount, 0);

    public static ISpecification<Order> AnyDiscount =>
        Specification.Any(PremiumDiscount, FirstTimeDiscount);
}

// Usage
if (DiscountEligibility.PremiumDiscount.IsSatisfiedBy(order))
{
    order.ApplyDiscount(0.15m);
}
```

---

## 📁 Project Structure

```
System.Validation/
├── DependencyInjection/
│   └── IValiadatorExtensions.cs     # IServiceCollection extension members
├── ISpecification.cs                # ISpecification<TSource> interface
├── Specification.cs                 # Specification record + Specification<TSource>
├── SpecificationExtensions.cs       # Extension members (And/Or/Not/LINQ)
├── IValidator.cs                    # IValidator + IValidator<TArgument>
├── Validator.cs                     # Abstract Validator<TArgument> base
├── EmptyValidator.cs                # No-op validator
├── ICompositeValidator.cs           # ICompositeValidator<TArgument>
├── CompositeValidator.cs            # Sealed composite aggregator
├── IValidatorFactory.cs             # Factory interface
├── ValidatorFactory.cs              # Default factory
├── IValidatorProvider.cs            # Provider interface
├── ValidatorProvider.cs             # Default provider
├── IValidatorResolver.cs            # Resolver interface
├── ValidatorResolver.cs             # Default resolver
└── IRequiresValidation.cs           # Marker interface
```

---

## 📋 Core Types Summary

| Type | Description |
|------|-------------|
| `ISpecification<T>` | Specification with `Expression` and `IsSatisfiedBy` |
| `Specification` | Static factory and combinator methods |
| `Specification<T>` | Concrete record with operator overloads |
| `IValidator<T>` | Strongly-typed validator with sync/async support |
| `Validator<T>` | Abstract base — override `Validate` and optionally `ValidateAsync` |
| `EmptyValidator<T>` | No-op validator returning empty results |
| `ICompositeValidator<T>` | Aggregates multiple validators by `Order` |
| `CompositeValidator<T>` | Sealed composite implementation |
| `IValidatorFactory` | Creates validators by type |
| `ValidatorFactory` | Default factory using resolvers and DI |
| `IValidatorProvider` | Retrieves validators with `IRequiresValidation` enforcement |
| `ValidatorProvider` | Default provider delegating to factory |
| `IValidatorResolver` | Runtime resolver mapping `TargetType` → validator |
| `ValidatorResolver<T>` | Sealed resolver resolving from DI container |
| `IRequiresValidation` | Marker interface for validatable types |

---

## ✅ Best Practices

1. **Seal your validators** — `AddXValidators` only discovers `sealed` implementations
2. **Implement `IRequiresValidation`** — Required constraint on all `TArgument` types
3. **Use factory methods** — Prefer `Equal`, `GreaterThan`, `Contains` over raw expressions
4. **Combine specifications** — Use `And`, `Or`, `Not` extension methods for complex rules
5. **Override `ValidateAsync`** — For I/O-bound checks (database, external services)
6. **Use `Order` property** — Control execution sequence in `CompositeValidator<T>`
7. **Inject `ICompositeValidator<T>`** — When multiple validators exist for the same type

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Primitives** | Core primitives and utilities |
| **Xpandables.Results** | Result types with validation integration |
| **Xpandables.Results.Pipelines** | Automatic validation in request pipelines |

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
