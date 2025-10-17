# ?? Xpandables.Net.ExecutionResults.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Integration** - Seamlessly convert `ExecutionResult` to ASP.NET Core `IResult` for Minimal APIs and controllers.

---

## ?? Overview

Provides extension methods to convert `ExecutionResult` instances to ASP.NET Core's `IResult`, enabling clean integration with Minimal APIs and MVC controllers.

### ? Key Features

- ?? **Automatic Conversion** - `ExecutionResult` to `IResult`
- ?? **Status Code Mapping** - Correct HTTP status codes
- ?? **ProblemDetails Support** - RFC 7807 compliance
- ? **Zero Overhead** - Efficient conversions

---

## ?? Quick Start

```csharp
// Minimal API
app.MapGet("/users/{id}", async (Guid id, IUserService service) =>
{
    ExecutionResult<User> result = await service.GetUserAsync(id);
    return result.ToIResult(); // Automatic conversion
});

app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
{
    ExecutionResult<User> result = await service.CreateUserAsync(request);
    return result.ToIResult();
});
```

---

## ?? Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        ExecutionResult<User> result = await _service.GetUserAsync(id);
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        ExecutionResult<User> result = await _service.CreateUserAsync(request);
        return result.ToActionResult();
    }
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
