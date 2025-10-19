# Correct Xpandables.Net REST Client Usage

## ? How Xpandables.Net REST Works

Unlike Refit or similar libraries, Xpandables.Net uses a **request-based pattern** where:

1. **Request classes** have the REST attributes (not interface methods)
2. **Request classes** implement `IRestRequest` 
3. You use the provided `IRestClient` to send requests
4. You can create wrapper classes for convenience

## Architecture Pattern

```
REST Attributes ? Request Classes (IRestRequest) ? IRestClient.SendAsync() ? Response
```

## Example Implementation

### 1. Request Classes with REST Attributes

```csharp
// POST request
[RestPost("/api/bank-accounts")]
public sealed record CreateBankAccountRequest : IRestRequest
{
    public required string Owner { get; init; }
    public required string Email { get; init; }
    public decimal InitialBalance { get; init; }
}

// GET request with path parameter
[RestGet("/api/bank-accounts/{accountId}")]
public sealed record GetBankAccountRequest : IRestRequest, IRestPathString
{
    public required Guid AccountId { get; init; }
    
    // IRestPathString implementation
    public KeyValuePair<string, string> PathString => 
        new("accountId", AccountId.ToString());
}

// GET request with query parameters
[RestGet("/api/bank-accounts/{accountId}/transactions")]
public sealed record GetTransactionsRequest : IRestRequest, IRestPathString, IRestQueryString
{
    public required Guid AccountId { get; init; }
    public int PageSize { get; init; } = 20;
    
    // IRestPathString implementation
    public KeyValuePair<string, string> PathString => 
        new("accountId", AccountId.ToString());
    
    // IRestQueryString implementation
    public IEnumerable<KeyValuePair<string, string>> QueryStrings
    {
        get
        {
            yield return new("pageSize", PageSize.ToString());
        }
    }
}
```

### 2. Client Wrapper (Optional but Recommended)

```csharp
public sealed class BankAccountClient(IRestClient restClient)
{
    public async Task<BankAccountResponse?> CreateAccountAsync(
        string owner,
        string email,
        decimal initialBalance = 0,
        CancellationToken cancellationToken = default)
    {
        // Create request
        var request = new CreateBankAccountRequest
        {
            Owner = owner,
            Email = email,
            InitialBalance = initialBalance
        };

        // Send using IRestClient
        var response = await restClient.SendAsync(request, cancellationToken);
        
        // Parse response
        if (response.IsSuccess)
        {
            return await response.ReadFromJsonAsync<BankAccountResponse>(cancellationToken);
        }

        return null;
    }
}
```

### 3. Service Registration

```csharp
// In Program.cs (Blazor WebAssembly)
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register IRestClient with Xpandables.Net
builder.Services.AddXRestClient((sp, httpClient) =>
{
    httpClient.BaseAddress = new Uri("https://localhost:7001");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});

// Register request and response composers
builder.Services.AddXRestRequestComposers();
builder.Services.AddXRestResponseComposers();

// Register your wrapper (optional)
builder.Services.AddScoped<BankAccountClient>();
```

### 4. Usage in Blazor Components

```csharp
@inject BankAccountClient BankClient

@code {
    private async Task CreateAccount()
    {
        var account = await BankClient.CreateAccountAsync(
            "John Doe",
            "john@example.com",
            1000);

        if (account != null)
        {
            // Success!
            Navigation.NavigateTo($"/accounts/{account.AccountId}");
        }
    }
}
```

## Available REST Interfaces

When creating request classes, you can implement these interfaces for additional functionality:

| Interface | Purpose | Example |
|-----------|---------|---------|
| `IRestRequest` | Base interface (required) | All requests |
| `IRestPathString` | URL path parameters | `/api/users/{id}` |
| `IRestQueryString` | Query string parameters | `?pageSize=20&page=1` |
| `IRestHeader` | Custom HTTP headers | `X-Custom-Header: value` |
| `IRestCookie` | Cookies | Session cookies |
| `IRestBasicAuthentication` | Basic auth | Username/password |
| `IRestMultipart` | File uploads | Form data with files |
| `IRestFormUrlEncoded` | Form data | `application/x-www-form-urlencoded` |
| `IRestByteArray` | Binary data | Byte array content |
| `IRestStream` | Stream data | File streams |
| `IRestString` | String content | Plain text |
| `IRestPatch` | PATCH operations | Partial updates |

## REST Attributes

Use these attributes on your request classes:

| Attribute | HTTP Method | Example |
|-----------|-------------|---------|
| `[RestGet]` | GET | `[RestGet("/api/users")]` |
| `[RestPost]` | POST | `[RestPost("/api/users")]` |
| `[RestPut]` | PUT | `[RestPut("/api/users/{id}")]` |
| `[RestPatch]` | PATCH | `[RestPatch("/api/users/{id}")]` |
| `[RestDelete]` | DELETE | `[RestDelete("/api/users/{id}")]` |
| `[RestHead]` | HEAD | `[RestHead("/api/users")]` |
| `[RestOptions]` | OPTIONS | `[RestOptions("/api/users")]` |

## Key Differences from Refit

| Feature | Refit | Xpandables.Net |
|---------|-------|----------------|
| Attributes on | Interface methods | Request classes |
| Interface | Required | Optional (just a wrapper) |
| Request representation | Method parameters | Request objects |
| Client | Generated proxy | Manual wrapper (optional) |
| Registration | `AddRefitClient<T>()` | `AddXRestClient()` |

## Benefits of Xpandables.Net Approach

1. ? **Type-safe requests** - Request objects are strongly typed
2. ? **Testable** - Easy to create and test request objects
3. ? **Flexible** - Can add validation, mapping, etc. to request classes
4. ? **No code generation** - Works at runtime without source generators
5. ? **Composable** - Can implement multiple interfaces on one request
6. ? **Reusable** - Request objects can be passed around, stored, etc.

## Common Patterns

### Pattern 1: Simple GET
```csharp
[RestGet("/api/users")]
public record GetUsersRequest : IRestRequest;
```

### Pattern 2: GET with Path Parameter
```csharp
[RestGet("/api/users/{userId}")]
public record GetUserRequest : IRestRequest, IRestPathString
{
    public Guid UserId { get; init; }
    public KeyValuePair<string, string> PathString => new("userId", UserId.ToString());
}
```

### Pattern 3: POST with Body
```csharp
[RestPost("/api/users")]
public record CreateUserRequest : IRestRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}
```

### Pattern 4: GET with Query Parameters
```csharp
[RestGet("/api/users")]
public record SearchUsersRequest : IRestRequest, IRestQueryString
{
    public string? Name { get; init; }
    public int PageSize { get; init; } = 10;
    
    public IEnumerable<KeyValuePair<string, string>> QueryStrings
    {
        get
        {
            if (Name != null)
                yield return new("name", Name);
            yield return new("pageSize", PageSize.ToString());
        }
    }
}
```

## Summary

The Xpandables.Net REST client is:
- ? **Request-focused** (not interface-focused)
- ? **Uses existing `IRestClient`** (don't create your own)
- ? **Attributes on request classes** (not interface methods)
- ? **Registered with `AddXRestClient()`**
- ? **Flexible and type-safe**

This is the correct way to use Xpandables.Net REST! ??
