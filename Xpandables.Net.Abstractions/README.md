# ?? Xpandables.Net.Abstractions

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Core Abstractions & Utilities** - Foundation package providing essential abstractions, collections, and helper types used across the Xpandables.Net ecosystem.

---

## ?? Overview

The foundational package containing core abstractions, element collections, state management patterns, and utility extensions that power the entire Xpandables.Net library suite.

### ?? Key Features

- ?? **ElementCollection** - Type-safe key-value collections with JSON support
- ?? **State Pattern** - `IState`, `IStateContext` for state management
- ??? **Disposable Helpers** - Base classes for proper resource disposal
- ?? **HTTP Extensions** - Status code helpers and utilities
- ?? **Serialization** - JSON serialization support with System.Text.Json

---

## ??? Core Types

### ElementCollection

```csharp
// Type-safe key-value collection
var collection = new ElementCollection
{
    ["key1"] = "value1",
    ["key2"] = 42,
    ["key3"] = new { Name = "Test", Id = 1 }
};

// Access values
string? value = collection["key1"]?.Value as string;
int? number = collection["key2"]?.ToInt32();

// JSON serialization
string json = JsonSerializer.Serialize(collection);
```

### State Pattern

```csharp
public interface IOrderState : IState<Order>
{
    void Process();
    void Ship();
    void Cancel();
}

public sealed class PendingOrderState : State<Order>, IOrderState
{
    public void Process() 
    {
        // Transition to processing
        Context.SetState(new ProcessingOrderState());
    }
    
    public void Ship() => throw new InvalidOperationException();
    public void Cancel() => Context.SetState(new CancelledOrderState());
}
```

### Disposable Base Classes

```csharp
public sealed class MyResource : Disposable
{
    private readonly Stream _stream;
    
    public MyResource(Stream stream)
    {
        _stream = stream;
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

---

## ?? HTTP Utilities

```csharp
// Status code helpers
bool isSuccess = statusCode.IsSuccess(); // 2xx
bool isClientError = statusCode.IsClientError(); // 4xx
bool isServerError = statusCode.IsServerError(); // 5xx

// Headers extensions
IDictionary<string, string> headers = response.Headers.ToDictionary();
ElementCollection headerCollection = response.Headers.ToElementCollection();
```

---

## ?? Serialization Helpers

```csharp
// Primitive type wrapping with JSON support
public sealed record UserId : IPrimitive<Guid>
{
    public Guid Value { get; init; }
}

// Automatic JSON serialization/deserialization
var userId = new UserId { Value = Guid.NewGuid() };
string json = JsonSerializer.Serialize(userId);
```

---

## ?? Use Cases

1. **Building Blocks**: Used by all other Xpandables.Net packages
2. **ElementCollection**: Storing flexible key-value metadata
3. **State Management**: Implementing state machines in domain models
4. **Resource Management**: Proper disposal patterns
5. **Type Extensions**: Common utilities for HTTP, strings, collections

---

## ?? Used By

This package is a dependency for:
- `Xpandables.Net.ExecutionResults`
- `Xpandables.Net.Optionals`
- `Xpandables.Net.Events`
- `Xpandables.Net.Repositories`
- And all other Xpandables.Net packages

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
