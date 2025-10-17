# ?? Xpandables.Net.Rests

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Type-Safe REST Client** - Build HTTP clients declaratively using attributes and interfaces for clean, maintainable API integrations.

---

## ?? Overview

`Xpandables.Net.Rests` provides an attribute-based REST client that eliminates boilerplate HTTP code. Define your API contracts with attributes and let the framework handle serialization, authentication, headers, and error handling automatically.

### ? Key Features

- ?? **Attribute-Based** - Declare API contracts with attributes
- ?? **Built-in Authentication** - OAuth, Bearer, Basic auth support
- ?? **Multiple Content Types** - JSON, Form, Multipart, Byte arrays
- ?? **Automatic Serialization** - System.Text.Json integration
- ?? **Performance** - Minimal allocations, reusable HTTP clients
- ? **Type-Safe** - Compile-time checking of requests/responses

---

## ?? Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.Rests
```

### Basic Setup

```csharp
using Xpandables.Net.Rests;

services.AddXRestClient((sp, client) =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### Simple GET Request

```csharp
// Define request
[RestGet("/users/{id}")]
public sealed record GetUserRequest(Guid Id) 
    : IRestRequest<User>, IRestPathString
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };
}

// Usage
var response = await _restClient.SendAsync(new GetUserRequest(userId));
if (response.IsSuccess)
{
    var user = response.To<User>();
}
```

---

## ?? HTTP Methods

### GET Request with Query String

```csharp
[RestGet("/products")]
public sealed record SearchProductsRequest(
    string? Name,
    decimal? MinPrice,
    int PageSize = 20) : IRestRequest<List<Product>>, IRestQueryString
{
    public IDictionary<string, string?> GetQueryString() => 
        new Dictionary<string, string?>
        {
            ["name"] = Name,
            ["minPrice"] = MinPrice?.ToString(),
            ["pageSize"] = PageSize.ToString()
        };
}
```

### POST Request with JSON Body

```csharp
[RestPost("/orders")]
public sealed record CreateOrderRequest(
    Guid CustomerId,
    List<OrderItem> Items) : IRestRequest<Order>, IRestString
{
    public string GetStringContent() => 
        JsonSerializer.Serialize(new { CustomerId, Items });
}
```

### PUT Request

```csharp
[RestPut("/users/{id}")]
public sealed record UpdateUserRequest(
    Guid Id,
    string Name,
    string Email) : IRestRequest<User>, IRestString, IRestPathString
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };
    
    public string GetStringContent() => 
        JsonSerializer.Serialize(new { Name, Email });
}
```

### DELETE Request

```csharp
[RestDelete("/orders/{id}")]
public sealed record DeleteOrderRequest(Guid Id) 
    : IRestRequest, IRestPathString
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };
}
```

---

## ?? Advanced Features

### Form URL Encoded

```csharp
[RestPost("/auth/token")]
public sealed record GetTokenRequest(
    string Username,
    string Password) : IRestRequest<TokenResponse>, IRestFormUrlEncoded
{
    public IDictionary<string, string> GetFormUrlEncoded() => 
        new Dictionary<string, string>
        {
            ["username"] = Username,
            ["password"] = Password,
            ["grant_type"] = "password"
        };
}
```

### Multipart Form Data (File Upload)

```csharp
[RestPost("/files/upload")]
public sealed record UploadFileRequest(
    string FileName,
    byte[] FileContent) : IRestRequest<UploadResponse>, IRestMultipart
{
    public IEnumerable<MultipartContent> GetMultipartContents()
    {
        yield return new MultipartContent
        {
            Name = "file",
            FileName = FileName,
            Content = new ByteArrayContent(FileContent)
        };
    }
}
```

### Custom Headers

```csharp
[RestGet("/data")]
public sealed record GetDataRequest() 
    : IRestRequest<Data>, IRestHeader
{
    public IDictionary<string, string> GetHeaders() => 
        new Dictionary<string, string>
        {
            ["X-API-Version"] = "2.0",
            ["X-Custom-Header"] = "CustomValue"
        };
}
```

### Authentication

```csharp
// Bearer token (automatic)
[RestGet("/protected", IsSecured = true)]
public sealed record GetProtectedDataRequest() 
    : IRestRequest<ProtectedData>;

// Basic authentication
[RestPost("/login")]
public sealed record LoginRequest(
    string Username,
    string Password) : IRestRequest<LoginResponse>, IRestBasicAuthentication
{
    public (string Username, string Password) GetBasicAuthentication() => 
        (Username, Password);
}
```

---

## ?? Complete Example

```csharp
// API client interface
public interface IUserApiClient
{
    Task<RestResponse<List<User>>> GetUsersAsync();
    Task<RestResponse<User>> GetUserAsync(Guid id);
    Task<RestResponse<User>> CreateUserAsync(CreateUserDto dto);
    Task<RestResponse> DeleteUserAsync(Guid id);
}

// Request definitions
[RestGet("/api/users")]
public sealed record GetUsersRequest() : IRestRequest<List<User>>;

[RestGet("/api/users/{id}")]
public sealed record GetUserRequest(Guid Id) 
    : IRestRequest<User>, IRestPathString
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };
}

[RestPost("/api/users", IsSecured = true)]
public sealed record CreateUserRequest(CreateUserDto Dto) 
    : IRestRequest<User>, IRestString
{
    public string GetStringContent() => 
        JsonSerializer.Serialize(Dto);
}

[RestDelete("/api/users/{id}", IsSecured = true)]
public sealed record DeleteUserRequest(Guid Id) 
    : IRestRequest, IRestPathString
{
    public IDictionary<string, string> GetPathString() => 
        new Dictionary<string, string> { ["id"] = Id.ToString() };
}

// Implementation
public sealed class UserApiClient : IUserApiClient
{
    private readonly IRestClient _restClient;
    
    public UserApiClient(IRestClient restClient)
    {
        _restClient = restClient;
    }
    
    public Task<RestResponse<List<User>>> GetUsersAsync() =>
        _restClient.SendAsync(new GetUsersRequest());
    
    public Task<RestResponse<User>> GetUserAsync(Guid id) =>
        _restClient.SendAsync(new GetUserRequest(id));
    
    public Task<RestResponse<User>> CreateUserAsync(CreateUserDto dto) =>
        _restClient.SendAsync(new CreateUserRequest(dto));
    
    public Task<RestResponse> DeleteUserAsync(Guid id) =>
        _restClient.SendAsync(new DeleteUserRequest(id));
}
```

---

## ?? Configuration

### HTTP Client Configuration

```csharp
services.AddXRestClient((sp, client) =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
})
.AddHttpMessageHandler<AuthenticationHandler>(); // Custom handler
```

### Response Handling

```csharp
var response = await _restClient.SendAsync(request);

// Check status
if (response.IsSuccess)
{
    var data = response.To<User>();
}

// Access response details
HttpStatusCode statusCode = response.StatusCode;
IDictionary<string, string> headers = response.Headers;
Exception? exception = response.Exception;

// Convert to ExecutionResult
ExecutionResult<User> result = response.ToExecutionResult<User>();
```

---

## ?? Best Practices

1. **Reuse IRestClient**: Register as singleton in DI
2. **Use Typed Responses**: Leverage `IRestRequest<T>` for type safety
3. **Handle Errors**: Always check `response.IsSuccess`
4. **Secure Endpoints**: Use `IsSecured = true` for authenticated endpoints
5. **Configure Timeouts**: Set appropriate timeouts for different operations

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
