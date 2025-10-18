# 🔧 Xpandables.Net.Abstractions

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Core Abstractions & Utilities** - Foundation package providing essential abstractions, collections, and helper types used across the Xpandables.Net ecosystem.

---

## 📋 Overview

The foundational package containing core abstractions, element collections, state management patterns, and utility extensions that power the entire Xpandables.Net library suite.

### 🎯 Key Features

- 📦 **ElementCollection** - Type-safe key-value collections with JSON support
- 🔄 **State Pattern** - `IState`, `IStateContext` for state management
- 🗑️ **Disposable Helpers** - Base classes for proper resource disposal
- 🌐 **HTTP Extensions** - Status code helpers and utilities
- 📝 **Serialization** - JSON serialization support with System.Text.Json

---

## 🏗️ Core Types

### ElementCollection

```csharp
// Type-safe key-value collection

var collection = new ElementCollection();
collection.Add("key1", "value1");
collection.Add("key2", "42");
ElementEntry? entry = collection["key1"];
string? value = entry?.Values.ToString();

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

public sealed class OrderContext : StateContext<IOrderState>
{
    public OrderContext(IOrderState initialState) : base(initialState) { }
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

## 🌐 HTTP Utilities

```csharp
// Status code helpers
HttpStatusCode statusCode = HttpStatusCode.OK;
bool isSuccess = statusCode.IsSuccess; // Extension property
statusCode.EnsureSuccess(); // Throws if not successful

// Headers extensions
IDictionary<string, string> headers = response.Headers.ToDictionary();
ElementCollection headerCollection = response.Headers.ToElementCollection();
```

---

## 📝 Serialization Helpers

```csharp
// Primitive type wrapping with JSON support

[PrimitiveJsonConverter]
public readonly record struct UserId : IPrimitive<UserId, Guid>
{
    private UserId(Guid value) => Value = value;
    public Guid Value { get;}
    public static UserId Create(Guid value) => new(value);
    public static implicit operator Guid(UserId userId) => userId.Value;
    public static implicit operator UserId(Guid value) => Create(value);
    public static implicit operator string(UserId userId) => userId.Value.ToString();
}

// Automatic JSON serialization/deserialization
var userId = UserId.Create(Guid.NewGuid());
string json = JsonSerializer.Serialize(userId);
```

---

## 💡 Use Cases

1. **Building Blocks**: Used by all other Xpandables.Net packages
2. **ElementCollection**: Storing flexible key-value metadata
3. **State Management**: Implementing state machines in domain models
4. **Resource Management**: Proper disposal patterns
5. **Type Extensions**: Common utilities for HTTP, strings, collections

---

## 📚 Used By

This package is a dependency for:
- `Xpandables.Net.ExecutionResults`
- `Xpandables.Net.Optionals`
- `Xpandables.Net.Events`
- `Xpandables.Net.Repositories`
- And all other Xpandables.Net packages

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
