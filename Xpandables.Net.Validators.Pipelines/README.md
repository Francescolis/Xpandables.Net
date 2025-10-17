# ? Xpandables.Net.Validators.Pipelines

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Validation Pipeline Decorators** - Automatic validation integration for the mediator pipeline supporting requests marked with `IRequiresValidation`.

---

## ?? Overview

Provides pipeline decorators that automatically validate requests implementing `IRequiresValidation` before they reach their handlers, ensuring data integrity and clean separation of concerns.

### ? Key Features

- ? **Automatic Validation** - Validate requests in the pipeline
- ?? **Decorator Pattern** - Non-invasive validation
- ?? **Type-Safe** - Compile-time validation contracts
- ?? **ExecutionResult Integration** - Clean error responses

---

## ?? Quick Start

```csharp
// Register validation pipeline
services.AddXValidatorPipeline();

// Mark request for validation
public sealed record CreateUserCommand(
    string Email,
    int Age) : IRequest, IRequiresValidation;

// Define validator
public sealed class CreateUserValidator : Validator<CreateUserCommand>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateUserCommand instance)
    {
        var errors = new List<ValidationResult>();
        
        if (string.IsNullOrEmpty(instance.Email))
            errors.Add(new ValidationResult("Email is required"));
        
        if (instance.Age < 18)
            errors.Add(new ValidationResult("Must be 18+"));
        
        return errors;
    }
}

// Validation happens automatically before handler execution
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
