# System.AsyncPaged.Json

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

JSON serialization and deserialization for `IAsyncPagedEnumerable<T>` using `System.Text.Json`.

## 📖 Overview

`System.AsyncPaged.Json` (NuGet: **Xpandables.AsyncPaged.Json**) provides C# 14 extension members on `JsonSerializer` for streaming paged async enumerables to/from JSON via `PipeWriter` and `Stream`. It also includes `HttpContent` deserialization helpers. Namespace: `System.Text.Json`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `JsonSerializerExtensions` | `JsonSerializerExtensions.cs` | `SerializeAsyncPaged` — writes `IAsyncPagedEnumerable<T>` as JSON to `PipeWriter` (with `JsonTypeInfo<T>` or `JsonSerializerOptions`) |
| `JsonDeserializerExtensions` | `JsonDeserializerExtensions.cs` | `DeserializeAsyncPaged` — reads JSON stream back into `IAsyncPagedEnumerable<T>` |
| `HttpContentExtensions` | `HttpContentExtensions.cs` | `ReadAsAsyncPagedEnumerable` — deserialize from `HttpContent` |

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged.Json
```

**Project References:** `Xpandables.AsyncPaged`

## 🚀 Quick Start

### Serialize `IAsyncPagedEnumerable<T>` to a PipeWriter (ASP.NET Core Endpoint)

```csharp
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.IO.Pipelines;

// In an ASP.NET Core minimal API endpoint
app.MapGet("/api/products", async (HttpContext httpContext, AppDbContext db) =>
{
    IAsyncPagedEnumerable<ProductDto> products = db.Products
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Skip(0).Take(50)
        .Select(p => new ProductDto(p.Id, p.Name, p.Price))
        .ToAsyncPagedEnumerable();

    httpContext.Response.ContentType = "application/json; charset=utf-8";
    PipeWriter pipeWriter = httpContext.Response.BodyWriter;

    // Option A: with source-generated JsonTypeInfo (AOT-safe)
    await JsonSerializer.SerializeAsyncPaged(
        pipeWriter,
        products,
        AppJsonContext.Default.ProductDto,
        httpContext.RequestAborted);

    // Option B: with JsonSerializerOptions (reflection-based)
    await JsonSerializer.SerializeAsyncPaged(
        pipeWriter,
        products,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
        httpContext.RequestAborted);
});
```

### Deserialize from a Stream (Service-to-Service Call)

```csharp
using System.Text.Json;

// Read a JSON array stream from an HTTP response
await using Stream responseStream = await httpResponse.Content.ReadAsStreamAsync();

IAsyncPagedEnumerable<OrderDto?> orders = JsonSerializer
    .DeserializeAsyncPagedEnumerable<OrderDto>(responseStream, jsonOptions);

await foreach (OrderDto? order in orders)
{
    if (order is not null)
        Console.WriteLine($"Order #{order.Id}: {order.Total:C}");
}
```

### Deserialize from a PipeReader

```csharp
using System.IO.Pipelines;
using System.Text.Json;

PipeReader reader = PipeReader.Create(stream);

IAsyncPagedEnumerable<ProductDto?> products = JsonSerializer
    .DeserializeAsyncPagedEnumerable(reader, AppJsonContext.Default.ProductDto);

await foreach (ProductDto? product in products)
{
    if (product is not null)
        Console.WriteLine(product.Name);
}
```

### Deserialize from HttpContent (HttpClient)

```csharp
using System.Net.Http.Json;

HttpResponseMessage response = await httpClient.GetAsync("/api/products");
response.EnsureSuccessStatusCode();

// With default options
IAsyncPagedEnumerable<ProductDto?> products = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable<ProductDto>();

await foreach (ProductDto? product in products)
{
    Console.WriteLine(product?.Name);
}

// With explicit JsonTypeInfo (AOT-safe)
IAsyncPagedEnumerable<ProductDto?> productsAot = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable(AppJsonContext.Default.ProductDto);

// With pagination strategy
IAsyncPagedEnumerable<ProductDto?> productsPaged = response.Content
    .ReadFromJsonAsAsyncPagedEnumerable<ProductDto>(
        strategy: PaginationStrategy.PerPage);
```

### Full Round-Trip Example

```csharp
// Server: serialize paged results to response
app.MapGet("/api/users", async (HttpContext ctx, IUserService userService) =>
{
    IAsyncPagedEnumerable<UserDto> users = await userService.GetUsersPagedAsync(
        page: 1, pageSize: 50);

    ctx.Response.ContentType = "application/json; charset=utf-8";
    await JsonSerializer.SerializeAsyncPaged(
        ctx.Response.BodyWriter, users, AppJsonContext.Default.UserDto);
});

// Client: deserialize paged response stream
HttpResponseMessage resp = await httpClient.GetAsync("/api/users");
IAsyncPagedEnumerable<UserDto?> users = resp.Content
    .ReadFromJsonAsAsyncPagedEnumerable(AppJsonContext.Default.UserDto);

await foreach (UserDto? user in users)
{
    Console.WriteLine($"{user?.Id}: {user?.Name}");
}
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
