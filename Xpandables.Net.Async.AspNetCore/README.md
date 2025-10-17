# ?? Xpandables.Net.Async.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Async ASP.NET Core Integration** - Extensions for `IAsyncEnumerable` and pagination in ASP.NET Core responses.

---

## ?? Overview

Provides ASP.NET Core-specific extensions for streaming async enumerables and paginated responses, enabling efficient data transfer in web APIs.

### ? Key Features

- ?? **Streaming Responses** - Stream `IAsyncEnumerable<T>` to HTTP
- ?? **Paginated APIs** - Built-in pagination support
- ? **Performance** - Memory-efficient data transfer
- ?? **Type-Safe** - Strongly-typed responses

---

## ?? Quick Start

```csharp
app.MapGet("/users", (IUserRepository repository) =>
{
    IAsyncPagedEnumerable<User> users = repository
        .GetAllAsync()
        .ToPaged(pageSize: 20);
    
    return Results.Stream(users.AsAsyncEnumerable());
});
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
