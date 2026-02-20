# System.Primitives

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Primitives.svg)](https://www.nuget.org/packages/Xpandables.Primitives)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Primitives.svg)](https://www.nuget.org/packages/Xpandables.Primitives)
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
- **`MemoryAwareCache<TKey, TValue>`** — Cache with GC pressure monitoring and weak references
- **`WeakCacheEntry<T>`** — Weak reference cache entry
- **`ICacheTypeResolver`** — Type resolver for cached items
- **`CacheTypeResolver`** — Default type resolver

### Disposable Helpers
- **`Disposable`** — Base class for IDisposable
- **`DisposableAsync`** — Base class for IAsyncDisposable

### Extension Methods
- **`StringExtensions`** — String manipulation and formatting
- **`ObjectExtensions`** — Object null checks and type conversion
- **`ExceptionExtensions`** — Exception details and HTTP status mapping
- **`HttpStatusCodeExtensions`** — HTTP status helpers (Title, Detail, IsSuccess)
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

    public static bool TryParse(
        string? s, IFormatProvider? provider, out Email result)
    {
        if (s is not null && s.Contains('@'))
        {
            result = Create(s);
            return true;
        }
        result = default;
        return false;
    }

    public string ToString(string? format, IFormatProvider? provider) =>
        Value;

    public int CompareTo(Email other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    public int CompareTo(object? obj) =>
        obj is Email other ? CompareTo(other) : 1;

    public bool Equals(Email other) => Value == other.Value;
}

// Usage
Email email = Email.Create("john@example.com");
string emailStr = email.Value; // "john@example.com"
```

### Element Collections

```csharp
using System.Collections;

// Create a collection
var collection = ElementCollection.With("key1", "value1", "value2")
    + new ElementEntry("key2", "value3");

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

### Memory-Aware Cache

```csharp
using System.Cache;

// Cleanup every 5 mins, items expire after 1 hour (defaults)
var cache = new MemoryAwareCache<string, User>();

// Get existing or create new
User user = cache.GetOrAdd("user:123", key => LoadUser(key));

// Add or update
cache.AddOrUpdate("user:456", new User { Id = 456, Name = "Jane" });

// Try retrieve
if (cache.TryGetValue("user:123", out var cachedUser))
{
    Console.WriteLine($"Found: {cachedUser.Name}");
}

// Items use weak references — automatically collected under GC pressure
```

### HttpStatusCode Extensions

```csharp
using System.Net;

var status = HttpStatusCode.BadRequest;

bool isSuccess = status.IsSuccess;            // false
bool isFailure = status.IsFailure;            // true
bool isValidation = status.IsValidationProblem; // true
string title = status.Title;                  // "Bad Request"
string detail = status.Detail;                // Description text
```

### Exception Extensions

```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    // Get full message including inner exceptions
    string fullMessage = ex.GetFullExceptionMessage();

    // Convert to ElementCollection for structured error data
    ElementCollection entries = ex.GetElementEntries();

    // Get the appropriate HTTP status code
    HttpStatusCode statusCode = ex.GetHttpStatusCode();
}
```

---

## 🔧 Disposable Helpers

### Synchronous Disposable

```csharp
public class MyResource : Disposable
{
    private readonly Stream _stream;

    public MyResource(Stream stream) => _stream = stream;

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

    public MyAsyncResource(DbConnection connection)
        => _connection = connection;

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync(disposing);
    }
}
```

---

## Core Types

| Type | Description |
|------|-------------|
| `IPrimitive<TPrimitive, TValue>` | Strongly-typed value object |
| `ElementCollection` | Key-value collection |
| `IStateContext` | State machine context |
| `MemoryAwareCache<TKey, TValue>` | Weak-reference cache with auto-cleanup |
| `Disposable` / `DisposableAsync` | Disposable base classes |

---

## ✅ Best Practices

- **Use primitives for domain concepts** — Email, Money, ProductId, etc.
- **Validate in `Create()` method** — Fail fast with clear exceptions
- **Make primitives `readonly record struct`** — Stack allocation, value semantics
- **Decorate with `PrimitiveJsonConverter`** — Automatic JSON serialization
- **Keep primitives simple** — Single responsibility, no complex business logic
- **Use `Disposable`/`DisposableAsync`** — Consistent dispose pattern implementation

---

## 📚 Related Packages

- **Xpandables.Validation** — Specification pattern and validation
- **Xpandables.Optionals** — Optional/Maybe monad
- **Xpandables.Results** — Result pattern types

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
