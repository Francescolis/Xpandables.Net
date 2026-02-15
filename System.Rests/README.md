# System.Rests

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Rests.svg)](https://www.nuget.org/packages/Xpandables.Rests)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Type-safe, attribute-based REST client with automatic request composition, response handling, and interceptors.

## Overview

`System.Rests` provides a type-safe, attribute-based HTTP client for building RESTful API clients. It uses attributes to define endpoints and request types, with automatic request composition, response handling, request/response interceptors, and resilience options.

Built for .NET 10 with full async support.

## Features

### Core Client
- **`IRestClient`** — Core HTTP client interface with async support
- **`RestClient`** — Default implementation with interceptor pipeline

### Request/Response Building
- **`IRestRequestBuilder`** — Build HTTP requests from context
- **`IRestResponseBuilder`** — Build responses from HTTP messages
- **`RestRequestBuilder`** — Default request builder with interceptors
- **`RestResponseBuilder`** — Default response builder with interceptors

### REST Attributes
- **`RestGetAttribute`** — HTTP GET requests
- **`RestPostAttribute`** — HTTP POST requests
- **`RestPutAttribute`** — HTTP PUT requests
- **`RestDeleteAttribute`** — HTTP DELETE requests
- **`RestPatchAttribute`** — HTTP PATCH requests
- **`IRestAttributeBuilder`** — Dynamic attribute building

### Request Types
- **`IRestRequest`** — Base request interface
- **`IRestString`** — JSON body requests
- **`IRestQueryString`** — Query parameter requests
- **`IRestPathString`** — URL path parameter requests
- **`IRestFormUrlEncoded`** — Form data requests
- **`IRestMultipart`** — File upload requests
- **`IRestByteArray`** — Binary data requests
- **`IRestStream`** — Stream data requests
- **`IRestHeader`** — Custom header requests
- **`IRestCookie`** — Cookie requests
- **`IRestBasicAuthentication`** — Basic auth requests
- **`IRestPatch`** — JSON Patch requests
- **`IRestMime`** — MIME type support

### Interceptors
- **`IRestRequestInterceptor`** — Intercept requests before sending
- **`IRestResponseInterceptor`** — Intercept responses after receiving
- **`Order`** — Control interceptor execution order

### Resilience Options
- **`RestClientOptions`** — Configure timeout, retry, circuit breaker, logging
- **`RestRetryOptions`** — Retry policy configuration
- **`RestCircuitBreakerOptions`** — Circuit breaker configuration
- **`RestLogLevel`** — Logging levels

### Request Composers
- **`IRestRequestComposer`** — Compose HTTP request messages
- **`RestStringComposer`** — JSON body composition
- **`RestQueryStringComposer`** — Query string composition
- **`RestPathStringComposer`** — Path parameter composition
- **`RestFormUrlEncodedComposer`** — Form data composition
- **`RestMultipartComposer`** — Multipart composition
- **`RestHeaderComposer`** — Header composition
- **`RestCookieComposer`** — Cookie composition
- **`RestBasicAuthComposer`** — Basic auth composition
- **`RestByteArrayComposer`** — Binary composition
- **`RestStreamComposer`** — Stream composition
- **`RestPatchComposer`** — JSON Patch composition

### Response Composers
- **`IRestResponseComposer`** — Compose REST responses
- **`RestResponseResultComposer`** — Typed result responses
- **`RestResponseContentComposer`** — Content responses
- **`RestResponseStreamComposer`** — Stream responses
- **`RestResponseStreamPagedComposer`** — Paged stream responses
- **`RestResponseNoContentComposer`** — No content responses
- **`RestResponseFailureComposer`** — Error responses

### Other Types
- **`RestRequest`** — Request wrapper
- **`RestResponse`** — Response wrapper with typed result
- **`RestRequestContext`** — Request building context
- **`RestResponseContext`** — Response building context
- **`RestSettings`** — Global settings
- **`RestAttributeProvider`** — Attribute resolution
- **`RestAuthorizationHandler`** — Authorization handling

## Installation

```bash
dotnet add package Xpandables.Rests
```

## Quick Start

### Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddXRestAttributeProvider();
services.AddXRestRequestComposers();
services.AddXRestResponseComposers();
services.AddXRestRequestBuilder();
services.AddXRestResponseBuilder();
services.AddXRestClient((sp, client) =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});
```

### Define REST Requests

```csharp
using System.Rests.Abstractions;

// GET with path parameters
public sealed record GetUserRequest(Guid Id) 
    : IRestRequest<User>, IRestPathString, IRestAttributeBuilder
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };

    public RestAttribute Build(IServiceProvider sp) => 
        new RestGetAttribute("/api/users/{id}");
}

// POST with JSON body
public sealed record CreateUserRequest(string Name, string Email) 
    : IRestRequest<User>, IRestString, IRestAttributeBuilder
{
    public RestAttribute Build(IServiceProvider sp) => 
        new RestPostAttribute("/api/users");
}
```

### Use the Client

```csharp
public class UserService(IRestClient client)
{
    public async Task<User?> GetUserAsync(Guid id, CancellationToken ct)
    {
        RestResponse response = await client.SendAsync(new GetUserRequest(id), ct);

        if (response.IsSuccess)
        {
            return response.ToRestResponse<User>().Result;
        }

        return null;
    }
}
```

### Add Request Interceptor

```csharp
public class LoggingInterceptor : IRestRequestInterceptor
{
    public int Order => 0;

    public ValueTask InterceptAsync(RestRequestContext context, CancellationToken ct)
    {
        Console.WriteLine($"Sending: {context.Message.RequestUri}");
        return ValueTask.CompletedTask;
    }
}

// Register
services.AddSingleton<IRestRequestInterceptor, LoggingInterceptor>();
```

### Add Response Interceptor

```csharp
public class MetricsInterceptor : IRestResponseInterceptor
{
    public int Order => 0;

    public ValueTask<RestResponse> InterceptAsync(
        RestResponseContext context, 
        RestResponse response, 
        CancellationToken ct)
    {
        Console.WriteLine($"Status: {response.StatusCode}");
        return ValueTask.FromResult(response);
    }
}
```

### Configure Resilience

```csharp
services.ConfigureXRestClientOptions(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.Retry = new RestRetryOptions
    {
        MaxRetryAttempts = 3,
        UseExponentialBackoff = true
    };
    options.CircuitBreaker = new RestCircuitBreakerOptions
    {
        FailureThreshold = 5,
        BreakDuration = TimeSpan.FromSeconds(30)
    };
});
```

## Core Types

| Type | Description |
|------|-------------|
| `IRestClient` | HTTP client interface |
| `IRestRequest` | Base request interface |
| `RestAttribute` | Endpoint attribute |
| `IRestRequestInterceptor` | Request interceptor |
| `IRestResponseInterceptor` | Response interceptor |
| `RestResponse` | Response wrapper |
| `RestClientOptions` | Resilience configuration |

---

## ✅ Best Practices

1. **Use appropriate request interfaces** — `IRestString` for JSON, `IRestQueryString` for GET params
2. **Combine interfaces** — A request can implement multiple interfaces (`IRestPathString` + `IRestString`)
3. **Use records** — Immutable request types work best
4. **Dispose responses** — Always use `using` with `RestResponse`
5. **Handle errors** — Check `IsSuccess` before accessing results
6. **Configure timeouts** — Set appropriate timeouts for your API

---

## 📚 Related Packages

- **Xpandables.Results** — Result types for response handling
- **Xpandables.AspNetCore** — ASP.NET Core integration

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
