# ? Xpandables.Net.Validators

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Flexible Validation Framework** - Build robust validation logic using specifications and composable validators.

---

## ?? Overview

A lightweight, extensible validation framework that supports both synchronous and asynchronous validation with specification pattern integration.

### ? Key Features

- ? **Specification Pattern** - Composable business rules
- ?? **Sync/Async Support** - Both validation modes
- ?? **Type-Safe** - Generic validation with compile-time checks
- ?? **Composable** - Combine validators with AND/OR logic
- ?? **Standard Results** - Uses `ValidationResult` from System.ComponentModel.DataAnnotations

---

## ?? Quick Start

```csharp
public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateUserRequest instance)
    {
        var spec = new Specification<CreateUserRequest>()
            .And(u => !string.IsNullOrEmpty(u.Email), "Email is required")
            .And(u => u.Email.Contains("@"), "Invalid email format")
            .And(u => u.Age >= 18, "Must be 18 or older");
        
        return spec.IsSatisfiedBy(instance) 
            ? [] 
            : spec.GetErrors();
    }
}
```

---

## ?? Specifications

```csharp
// Build complex specifications
var userSpec = new Specification<User>()
    .And(u => u.Age >= 18, "Must be 18+")
    .And(u => u.Email.Contains("@"), "Invalid email")
    .Or(u => u.PhoneNumber != null, "Email or phone required");

if (userSpec.IsSatisfiedBy(user))
{
    // User is valid
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
