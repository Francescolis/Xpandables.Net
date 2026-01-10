# 🌐 System.Rests

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/System.Rests)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **REST Client** - Type-safe, attribute-based HTTP client with automatic serialization, request composition, and response handling.

---

## 📋 Overview

`System.Rests` provides a type-safe, attribute-based HTTP client for building RESTful API clients. It uses attributes to define endpoints and request types, with automatic request composition and response handling.

### 🎯 Key Features

- 🌐 **IRestClient** - Core HTTP client interface with async support
- 🏷️ **REST Attributes** - RestGet, RestPost, RestPut, RestDelete, RestPatch
- 📦 **Request Types** - IRestString, IRestQueryString, IRestFormUrlEncoded, IRestMultipart
- 🔐 **Authentication** - Built-in Basic Auth and Bearer token support
- 📤 **Request Composition** - Automatic query string, headers, cookies, path parameters
- 📥 **Response Handling** - JSON deserialization with RestResponse
- 🌊 **Streaming** - IRestRequestStream for async enumerable responses
- ⚙️ **Extensible** - Custom request/response composers

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package System.Rests
```

### Define a REST Request

```csharp
using System.Rests.Abstractions;

// GET request with query parameters
[RestGet("/api/users/{id}")]
public sealed record GetUserRequest(Guid Id) : IRestRequest<User>, IRestPathString;

// POST request with JSON body
[RestPost("/api/users")]
public sealed record CreateUserRequest(string Name, string Email) : IRestRequest<User>, IRestString;

// PUT request with JSON body
[RestPut("/api/users/{id}")]
public sealed record UpdateUserRequest(Guid Id, string Name, string Email) : IRestRequest<User>, IRestString, IRestPathString;

// DELETE request
[RestDelete("/api/users/{id}")]
public sealed record DeleteUserRequest(Guid Id) : IRestRequest, IRestPathString;
```

### Use the REST Client

```csharp
using System.Rests.Abstractions;

public class UserService
{
    private readonly IRestClient _restClient;

    public UserService(IRestClient restClient)
        => _restClient = restClient;

    public async Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var response = await _restClient.SendAsync(
            new GetUserRequest(userId),
            cancellationToken);

        if (response.IsSuccess)
        {
            return response.GetResult<User>();
        }

        return null;
    }

    public async Task<User?> CreateUserAsync(string name, string email, CancellationToken cancellationToken)
    {
        using var response = await _restClient.SendAsync(
            new CreateUserRequest(name, email),
            cancellationToken);

        return response.IsSuccess ? response.GetResult<User>() : null;
    }
}
```

---

## 🧩 Core Concepts

### REST Attributes

```csharp
// HTTP GET - query parameters by default
[RestGet("/api/products")]
public sealed record GetProductsRequest : IRestRequest<Product[]>, IRestQueryString;

// HTTP POST - JSON body by default, secured
[RestPost("/api/products")]
public sealed record CreateProductRequest(string Name, decimal Price) : IRestRequest<Product>, IRestString;

// HTTP PUT - JSON body, secured
[RestPut("/api/products/{id}")]
public sealed record UpdateProductRequest(Guid Id, string Name, decimal Price) : IRestRequest<Product>, IRestString, IRestPathString;

// HTTP DELETE - secured
[RestDelete("/api/products/{id}")]
public sealed record DeleteProductRequest(Guid Id) : IRestRequest, IRestPathString;

// HTTP PATCH - for partial updates
[RestPatch("/api/products/{id}")]
public sealed record PatchProductRequest(Guid Id, IEnumerable<IPatchOperation> Operations) : IRestRequest<Product>, IRestPatch, IRestPathString;
```

### Request Types

```csharp
// IRestString - JSON body
[RestPost("/api/users")]
public sealed record CreateUserRequest(string Name) : IRestRequest<User>, IRestString;

// IRestQueryString - Query parameters
[RestGet("/api/users")]
public sealed record SearchUsersRequest(string? Name, int? Page) : IRestRequest<User[]>, IRestQueryString;

// IRestPathString - URL path parameters
[RestGet("/api/users/{id}")]
public sealed record GetUserRequest(Guid Id) : IRestRequest<User>, IRestPathString;

// IRestFormUrlEncoded - Form data
[RestPost("/api/login")]
public sealed record LoginRequest(string Username, string Password) : IRestRequest<Token>, IRestFormUrlEncoded;

// IRestMultipart - File uploads
[RestPost("/api/files")]
public sealed record UploadFileRequest(Stream FileContent, string FileName) : IRestRequest<FileInfo>, IRestMultipart;

// IRestHeader - Custom headers
[RestGet("/api/secure")]
public sealed record SecureRequest : IRestRequest<Data>, IRestHeader
{
    public IDictionary<string, string> GetHeaders() => new Dictionary<string, string>
    {
        ["X-Custom-Header"] = "value"
    };
}

// IRestCookie - Cookies
[RestGet("/api/session")]
public sealed record SessionRequest : IRestRequest<Session>, IRestCookie
{
    public IDictionary<string, string> GetCookies() => new Dictionary<string, string>
    {
        ["session_id"] = "abc123"
    };
}

// IRestBasicAuthentication - Basic auth
[RestGet("/api/secure")]
public sealed record BasicAuthRequest : IRestRequest<Data>, IRestBasicAuthentication
{
    public string Username => "user";
    public string Password => "pass";
}
```

### Streaming Responses

```csharp
// For async enumerable responses
[RestGet("/api/events")]
public sealed record GetEventsRequest : IRestRequestStream<Event>;

// Usage
public async IAsyncEnumerable<Event> GetEventsAsync(CancellationToken cancellationToken)
{
    using var response = await _restClient.SendAsync(
        new GetEventsRequest(),
        cancellationToken);

    await foreach (var evt in response.GetResultStream<Event>(cancellationToken))
    {
        yield return evt;
    }
}
```

---

## ⚙️ Configuration

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register REST client
builder.Services.AddXRestClient(options =>
{
    options.BaseAddress = new Uri("https://api.example.com");
    options.Timeout = TimeSpan.FromSeconds(30);
});

// With authentication handler
builder.Services.AddXRestClient(options =>
{
    options.BaseAddress = new Uri("https://api.example.com");
})
.ConfigurePrimaryHttpMessageHandler<BearerTokenHandler>();
```

---

## ✅ Best Practices

1. **Use appropriate request interfaces** - IRestString for JSON, IRestQueryString for GET params
2. **Combine interfaces** - A request can implement multiple interfaces (IRestPathString + IRestString)
3. **Use records** - Immutable request types work best
4. **Dispose responses** - Always use `using` with RestResponse
5. **Handle errors** - Check IsSuccess before accessing results
6. **Configure timeouts** - Set appropriate timeouts for your API

---

## 📚 Related Packages

- **System.Results** - Result types for response handling
- **AspNetCore.Net** - ASP.NET Core integration
- **System.Text.Json** - JSON serialization

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
