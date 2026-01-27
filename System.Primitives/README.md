# System.Primitives

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Primitives.svg)](https://www.nuget.org/packages/Xpandables.Primitives)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Core primitives and utilities for .NET applications. Zero dependencies, foundational abstractions.

## Overview

`System.Primitives` provides foundational abstractions and utilities used throughout the Xpandables.Net ecosystem. It includes strongly-typed primitive wrappers (Value Objects), state management patterns, memory-aware caching, element collections, and essential extension methods.

Built for .NET 10 with zero external dependencies.

## Features

### Primitives (Value Objects)
- **`IPrimitive`** — Base primitive interface
- **`IPrimitive<TValue>`** — Typed primitive with value
- **`IPrimitive<TPrimitive, TValue>`** — Strongly-typed primitive with factory methods
- **`PrimitiveJsonConverterAttribute`** — JSON serialization support for primitives

### Collections
- **`ElementCollection`** — Key-value collection with O(1) lookup
- **`ElementEntry`** — Single entry with key and values
- **`ElementCollectionExtensions`** — Extension methods for collections
- **`EnumerableExtensions`** — LINQ extensions
- **`QueryableEmpty<T>`** — Empty queryable implementation
- **`ValidationResultExtensions`** — Convert validation results to collections
- **`HttpHeadersExtensions`** — Convert HTTP headers to collections

### State Pattern
- **`IState`** — State interface
- **`IStateContext`** — State context for state machines
- **`IMemento`** — Memento pattern for state snapshots
- **`IOriginator`** — Originator for memento pattern
- **`StateContext`** — Default state context implementation
- **`State`** — Base state implementation

### Caching
- **`MemoryAwareCache<TKey, TValue>`** — Cache with GC pressure monitoring
- **`WeakCacheEntry<T>`** — Weak reference cache entry
- **`ICacheTypeResolver`** — Type resolver for cached items
- **`CacheTypeResolver`** — Default type resolver

### Disposable Helpers
- **`Disposable`** — Base class for IDisposable
- **`DisposableAsync`** — Base class for IAsyncDisposable

### Extension Methods
- **`StringExtensions`** — String manipulation utilities
- **`ObjectExtensions`** — Object utilities
- **`ExceptionExtensions`** — Exception utilities
- **`HttpStatusCodeExtensions`** — HTTP status helpers
- **`JsonSerializerExtensions`** — JSON serialization helpers
- **`ThrowExceptionExtensions`** — Fluent exception throwing

### Dependency Injection
- **`IAddService`** — Service registration interface
- **`IServiceDecoratorExtensions`** — Decorator pattern for DI
- **`ExportOptions`** — MEF-style export options
- **`StaticConfiguration`** — Static configuration utilities
- **`LazyResolved<T>`** — Lazy service resolution

## Installation

```bash
dotnet add package Xpandables.Primitives
```

## Quick Start

### Strongly-Typed Primitives

```csharp
using System;

[PrimitiveJsonConverter<Email, string>]
public readonly record struct Email : IPrimitive<Email, string>
{
    public required string Value { get; init; }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email format");
        return new Email { Value = value.ToLower() };
    }

    public static string DefaultValue => string.Empty;

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string value) => Create(value);

    public static bool operator ==(Email left, Email right) => left.Value == right.Value;
    public static bool operator !=(Email left, Email right) => !(left == right);
}

// Usage
Email email = "john@example.com";  // Implicit conversion + validation
string emailStr = email;           // Implicit back to string
```

### Element Collections

```csharp
using System.Collections;

// Create a collection
var collection = ElementCollection.Create("key1", "value1", "value2")
    .Add("key2", "value3");

// Access entries
foreach (var entry in collection)
{
    Console.WriteLine($"{entry.Key}: {string.Join(", ", entry.Values)}");
}

// Lookup by key (O(1))
if (collection.TryGetValue("key1", out var values))
{
    Console.WriteLine($"Found: {values}");
}
```

### State Pattern

```csharp
using System.States;

public class TrafficLight : StateContext<TrafficLightState>
{
    public TrafficLight() : base(new RedState()) { }

    public void Next() => State.Handle(this);
}

public abstract class TrafficLightState : State
{
    public abstract void Handle(TrafficLight context);
}

public class RedState : TrafficLightState
{
    public override void Handle(TrafficLight context) => context.SetState(new GreenState());
}
```

### Memory-Aware Cache

```csharp
using System.Cache;

var cache = new MemoryAwareCache<string, ExpensiveObject>();

cache.GetOrAdd("key", () => new ExpensiveObject());

// Cache automatically evicts under memory pressure
```

## Core Types

| Type | Description |
|------|-------------|
| `IPrimitive<TPrimitive, TValue>` | Strongly-typed value object |
| `ElementCollection` | Key-value collection |
| `IStateContext` | State machine context |
| `MemoryAwareCache<K, V>` | GC-aware cache |
| `Disposable` | IDisposable base class |

## License

Apache License 2.0
var context = new StateContext<Order>(order, OrderState.Pending);

context.Request();  // Transitions to Confirmed
Console.WriteLine($"State: {context.State.Name}");  // "Confirmed"
```

---

## 💾 Memory-Aware Cache

Cache with automatic eviction under memory pressure:

```csharp
var cache = new MemoryAwareCache<string, User>(maxItems: 1000);

// Add items
cache.Add("user:123", new User { Id = 123, Name = "John" });
cache.Add("user:456", new User { Id = 456, Name = "Jane" });

// Retrieve
if (cache.TryGet("user:123", out var user))
{
    Console.WriteLine($"Found: {user.Name}");
}

// Cache uses weak references
// Items automatically evicted when GC needs memory
```

---

## 🧩 Extension Methods

### String Extensions
```csharp
string text = "  hello  ";
bool isEmpty = text.IsNullOrWhiteSpace();  // false
string trimmed = text.Trim();  // "hello"
```

### Object Extensions
```csharp
var obj = new { Name = "John", Age = 30 };
var dict = obj.ToDictionary();  // Convert to Dictionary<string, object>
```

### Exception Extensions
```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    var executionResult = ex.ToExecutionResult();
    // Converts exception to OperationResult
}
```

### HttpStatusCode Extensions
```csharp
var status = HttpStatusCode.BadRequest;

string title = status.Title;  // "Bad Request"
string detail = status.Detail;  // Description
bool isSuccess = status.IsSuccess();  // false
bool isFailure = status.IsFailure();  // true
bool isValidation = status.IsValidationProblem();  // true
```

---

## 🎯 Real-World Examples

### UserId Primitive

```csharp
[PrimitiveJsonConverter<UserId, Guid>]
public readonly record struct UserId : IPrimitive<UserId, Guid>
{
    public required Guid Value { get; init; }

    public static UserId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");
        return new UserId { Value = value };
    }

    public static Guid DefaultValue => Guid.Empty;
    public static UserId NewId() => Create(Guid.NewGuid());

    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid value) => Create(value);

    public static bool operator ==(UserId left, UserId right) => 
        left.Value == right.Value;
    public static bool operator !=(UserId left, UserId right) => 
        !(left == right);
}

// Usage in domain model
public class User
{
    public UserId Id { get; init; } = UserId.NewId();
    public Email Email { get; init; }
    public string Name { get; init; } = default!;
}
```

### Domain Model with Primitives

```csharp
public class Order
{
    public OrderId Id { get; init; } = OrderId.NewId();
    public UserId CustomerId { get; init; }
    public Money TotalAmount { get; private set; }
    public List<OrderLine> Lines { get; init; } = [];

    public void AddLine(ProductId productId, Quantity quantity, Money unitPrice)
    {
        var line = new OrderLine
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Total = unitPrice.Multiply(quantity.Value)
        };

        Lines.Add(line);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        decimal sum = Lines.Sum(l => l.Total.Value);
        TotalAmount = Money.Create(sum);
    }
}

// Usage
var order = new Order { CustomerId = currentUserId };
order.AddLine(
    productId: ProductId.From("PROD-001"),
    quantity: Quantity.Create(2),
    unitPrice: Money.Create(49.99m)
);

Console.WriteLine($"Order total: {order.TotalAmount}");  // $99.98
```

---

## 🔧 Disposable Helpers

### Synchronous Disposable

```csharp
public class MyResource : Disposable
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

### Async Disposable

```csharp
public class MyAsyncResource : DisposableAsync
{
    private readonly DbConnection _connection;

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && _connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync(disposing);
    }
}
```

---

## ✅ Best Practices

### ✅ Do

- **Use primitives for domain concepts** - Email, Money, ProductId, etc.
- **Validate in Create() method** - Fail fast with clear exceptions
- **Make primitives immutable** - Use `readonly record struct`
- **Add domain operations** - Methods like Add(), Subtract() for Money
- **Use implicit conversions** - Seamless usage with underlying types
- **Decorate with JsonConverter** - Automatic JSON serialization
- **Keep primitives simple** - Single responsibility

### ❌ Don't

- **Create primitives for everything** - Only for important domain concepts
- **Put complex business logic in primitives** - Keep them lightweight
- **Make primitives classes** - Use structs for better performance
- **Skip validation** - Always validate in Create() method
- **Expose public setters** - Primitives should be immutable
- **Throw generic exceptions** - Use specific ArgumentException messages

---

## 📊 Performance Benefits

| Aspect | Classes | Structs (Primitives) |
|--------|---------|----------------------|
| Allocation | Heap | Stack (most cases) |
| GC Pressure | Higher | Lower |
| Copy Cost | Reference | Value copy |
| Memory | Pointer overhead | Direct value |
| Best For | Large objects | Small values |

Primitives use `readonly record struct` for:
- ✅ Stack allocation in most cases
- ✅ Reduced GC pressure
- ✅ Value semantics
- ✅ Immutability guarantees

---

## 📚 Related Packages

- **[System.Primitives.Validation](../System.Primitives.Validation)** - FluentValidation integration
- **[System.Primitives.Composition](../System.Primitives.Composition)** - DI composition utilities
- **[System.ExecutionResults](../System.ExecutionResults)** - Result pattern types
- **[System.Optionals](../System.Optionals)** - Optional value types

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
