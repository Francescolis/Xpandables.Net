# System.Optionals

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Optionals.svg)](https://www.nuget.org/packages/Xpandables.Optionals)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Optionals.svg)](https://www.nuget.org/packages/Xpandables.Optionals)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Optional monad for .NET — eliminate null-reference errors with a functional approach.

## 📖 Overview

`System.Optionals` (NuGet: **Xpandables.Optionals**) provides a lightweight `Optional<T>` type that represents a value that may or may not be present. It supports functional-style operations (`Map`, `Bind`, `Match`), comparison, JSON serialization, LINQ integration, and operator overloads. Namespace: `System.Optionals`.

Built for **.NET 10** and **C# 14**. No external dependencies.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `Optional<T>` | `Optional.cs` | Read-only record struct — `Value`, `IsEmpty`, `IsNotEmpty`, implements `IEnumerable<T>` |
| `Optional` | `OptionalFactory.cs` | Static factory — `Empty<T>()`, `Some<T>(value)` |
| `OptionalSync` (partial) | `OptionalSync.cs` | Sync operations — `Map`, `Bind`, `Match`, `Filter`, `ToOptional<TU>` |
| `OptionalAsync` (partial) | `OptionalAsync.cs` | Async operations — `MapAsync`, `BindAsync`, `MatchAsync`, `FilterAsync` |
| `OptionalOperators` (partial) | `OptionalOperators.cs` | Comparison operators (`<`, `<=`, `>`, `>=`) and implicit conversions |
| `OptionalComparer` (partial) | `OptionalComparer.cs` | `IComparable<Optional<T>>` implementation |
| `OptionalExtensions` | `OptionalExtensions.cs` | Extension methods for converting values to `Optional<T>` |
| `EnumerableExtensions` | `EnumerableExtensions.cs` | LINQ helpers for `IEnumerable<Optional<T>>` |
| `OptionalJsonConverterFactory` | `OptionalJsonConverterFactory.cs` | `System.Text.Json` converter factory |
| `OptionalJsonSerialization` | `OptionalJsonSerialization.cs` | JSON serialization support |

## 📦 Installation

```bash
dotnet add package Xpandables.Optionals
```

## 🚀 Quick Start

### Creating Optionals

```csharp
using System.Optionals;

// From factory
var some = Optional.Some(42);
var empty = Optional.Empty<int>();

// Check state
if (some.IsNotEmpty) Console.WriteLine(some.Value); // 42
if (empty.IsEmpty) Console.WriteLine("No value");
```

### Map / Bind / Match

```csharp
var result = Optional.Some("hello")
    .Map(s => s.ToUpper())                    // Optional<string> "HELLO"
    .Bind(s => s.Length > 3
        ? Optional.Some(s)
        : Optional.Empty<string>())           // Flatten nested optionals
    .Match(
        some: v => $"Got: {v}",
        empty: () => "Nothing");              // "Got: HELLO"
```

### Async Operations

```csharp
var result = await Optional.Some(userId)
    .MapAsync(async id => await FindUserAsync(id))
    .MatchAsync(
        some: async user => await ProcessAsync(user),
        empty: () => Task.FromResult("Not found"));
```

### Safe Access

```csharp
var value = Optional.Some(42).GetValueOrDefault(0);             // 42
var fallback = Optional.Empty<int>().GetValueOrDefault(() => -1); // -1
```

### LINQ Integration

```csharp
// Optional<T> implements IEnumerable<T>
foreach (var item in Optional.Some("hello"))
{
    Console.WriteLine(item); // "hello"
}

// Empty optional yields nothing
foreach (var item in Optional.Empty<string>()) { /* never reached */ }
```

---

## 📁 Project Structure

```
System.Optionals/
├── Optional.cs                    # Core Optional<T> record struct
├── OptionalFactory.cs             # Static factory (Empty, Some)
├── OptionalSync.cs                # Map, Bind, Match, Filter
├── OptionalAsync.cs               # MapAsync, BindAsync, MatchAsync
├── OptionalOperators.cs           # Comparison & conversion operators
├── OptionalComparer.cs            # IComparable implementation
├── OptionalExtensions.cs          # Extension methods
├── EnumerableExtensions.cs        # LINQ helpers
├── OptionalJsonConverterFactory.cs
└── OptionalJsonSerialization.cs
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
