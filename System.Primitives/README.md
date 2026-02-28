# System.Primitives

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Primitives.svg)](https://www.nuget.org/packages/Xpandables.Primitives)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Primitives.svg)](https://www.nuget.org/packages/Xpandables.Primitives)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Core primitives and foundational utilities for the Xpandables.Net ecosystem.

## 📖 Overview

`System.Primitives` (NuGet: **Xpandables.Primitives**) provides strongly-typed primitive value objects, collections, dispose patterns, state machine abstractions, caching, text generation/cryptography, and shared DI extensions used across all Xpandables packages. Root namespace: `System`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

### 🏷️ Primitive Value Objects

| Type | File | Description |
|------|------|-------------|
| `IPrimitive` | `IPrimitive.cs` | Base interface exposing `object Value` |
| `IPrimitive<TValue>` | `IPrimitive.cs` | Strongly-typed value wrapper |
| `IPrimitive<TPrimitive, TValue>` | `IPrimitive.cs` | Full generic — creation, comparison, formatting, implicit conversions |

### 📦 Collections

| Type | File | Description |
|------|------|-------------|
| `ElementCollection` | `Collections/ElementCollection.cs` | Immutable record struct with O(1) key lookup and JSON support |
| `ElementEntry` | `Collections/ElementEntry.cs` | Key + `StringValues` pair |
| `EnumerableExtensions` | `Collections/EnumerableExtensions.cs` | Extension methods on `IEnumerable<T>` |
| `HttpHeadersExtensions` | `Collections/HttpHeadersExtensions.cs` | HTTP header conversion helpers |
| `ValidationResultExtensions` | `Collections/ValidationResultExtensions.cs` | `ValidationResult` collection helpers |
| `QueryableEmpty<T>` | `Collections/QueryableEmpty.cs` | Empty `IQueryable<T>` implementation |

### ♻️ Dispose Patterns

| Type | File | Description |
|------|------|-------------|
| `Disposable` | `Disposable.cs` | Abstract `IDisposable` base with `IsDisposed` guard |
| `DisposableAsync` | `DisposableAsync.cs` | Abstract `IAsyncDisposable` base with `IsDisposed` guard |

### 🔄 State Machine

| Type | File | Description |
|------|------|-------------|
| `IState` / `IState<TStateContext>` | `States/IState.cs` | State with `EnterStateContext` / `ExitStateContext` |
| `IStateContext` | `States/IStateContext.cs` | Context for state transitions |
| `StateContext` | `States/StateContext.cs` | Default implementation |
| `IOriginator` / `IMemento` | `States/` | Memento pattern support |
| `State` | `States/State.cs` | Base state implementation |

### 🔐 Text Utilities

| Type | File | Description |
|------|------|-------------|
| `TextGenerator` | `TextGenerator.cs` | Cryptographically secure random string generation |
| `TextCryptography` | `TextCryptography.cs` | AES encrypt/decrypt and secure comparison |
| `EncryptedValue` | `TextCryptography.cs` | Record struct — Key, Value, Salt |

### 💾 Caching

| Type | File | Description |
|------|------|-------------|
| `MemoryAwareCache<TKey, TValue>` | `Cache/MemoryAwareCache.cs` | Thread-safe weak-reference cache with auto-cleanup |
| `ICacheTypeResolver` / `CacheTypeResolver` | `Cache/` | Cache type discovery |

### 🛠️ Utilities

| Type | File | Description |
|------|------|-------------|
| `LazyResolved<T>` | `LazyResolved.cs` | `Lazy<T>` resolved from `IServiceProvider` |
| `IHttpStatusCodeExtension` | `IHttpStatusCodeExtension.cs` | Custom HTTP status code mapping |
| `StringExtensions` | `StringExtensions.cs` | String helpers |
| `ObjectExtensions` | `ObjectExtensions.cs` | Object helpers |
| `ExceptionExtensions` | `ExceptionExtensions.cs` | Exception helpers |
| `JsonSerializerExtensions` | `JsonSerializerExtensions.cs` | `System.Text.Json` helpers |

### 🔧 Composition

| Type | File | Description |
|------|------|-------------|
| `IAddService` | `Composition/IAddService.cs` | Interface for modular service registration |
| `ExportOptions` | `Composition/ExportOptions.cs` | MEF export configuration |

### ⚙️ Dependency Injection

C# 14 extension members on `IServiceCollection` (namespace: `Microsoft.Extensions.DependencyInjection`).

```csharp
services.AddXHttpStatusCodeExtension();       // IHttpStatusCodeExtension (Singleton)
services.AddXHttpStatusCodeExtension<T>();     // Custom implementation
services.AddXCacheTypeResolver();             // ICacheTypeResolver (Singleton)
services.AddXCacheTypeResolver<T>();           // Custom implementation
```

## 📦 Installation

```bash
dotnet add package Xpandables.Primitives
```

**Dependencies:** `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Configuration.EnvironmentVariables`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Primitives`

## 🚀 Quick Start

### ElementCollection

```csharp
using System.Collections;

var errors = ElementCollection.With("Name", "Name is required")
    .Add("Email", "Invalid email");

var merged = errors.Merge(ElementCollection.With("Age", "Must be 18+"));
```

### Text Cryptography

```csharp
EncryptedValue encrypted = TextCryptography.Encrypt("my-secret");
string decrypted = TextCryptography.Decrypt(encrypted);
bool match = TextCryptography.Compare(encrypted, "my-secret");
```

### MemoryAwareCache

```csharp
using System.Cache;

var cache = new MemoryAwareCache<string, MyService>(
    cleanupInterval: TimeSpan.FromMinutes(5),
    maxAge: TimeSpan.FromHours(1));

cache.TryAdd("key", myService);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
