# ? Xpandables.Net.Validators.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Validation ASP.NET Core Integration** - Action filters and model validation integration for ASP.NET Core applications.

---

## ?? Overview

Integrates the Xpandables.Net validation framework with ASP.NET Core's validation pipeline, providing automatic request validation with clean error responses.

### ? Key Features

- ?? **Action Filters** - Automatic model validation
- ?? **ProblemDetails** - RFC 7807 compliant errors
- ?? **FluentValidation Style** - Familiar validation patterns
- ? **Type-Safe** - Strongly-typed validators

---

## ?? Quick Start

```csharp
// Register validation
builder.Services.AddXValidators(typeof(Program).Assembly);
builder.Services.AddXValidationFilter();

// Controller
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    [HttpPost]
    [ValidateModel] // Automatic validation
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        // Request is already validated
        var result = await _service.CreateUserAsync(request);
        return result.ToActionResult();
    }
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
