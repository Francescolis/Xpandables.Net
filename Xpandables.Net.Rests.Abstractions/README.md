# ?? Xpandables.Net.Rests.Abstractions

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **REST Client Abstractions** - Core interfaces and attributes for building type-safe REST clients.

---

## ?? Overview

Provides the foundational abstractions, interfaces, and attributes used by the REST client implementation. Use this package when you want to define REST contracts without taking a dependency on the implementation.

### ? Key Features

- ?? **Interface Definitions** - Core REST client contracts
- ??? **Attribute System** - REST endpoint decoration
- ?? **Content Interfaces** - Multi-format support
- ?? **Authentication Abstractions** - Auth contract definitions

---

## ?? Core Interfaces

```csharp
// Request marker
public interface IRestRequest { }

// Typed request
public interface IRestRequest<TResponse> : IRestRequest { }

// Content providers
public interface IRestString
{
    string GetStringContent();
}

public interface IRestQueryString
{
    IDictionary<string, string?> GetQueryString();
}

public interface IRestPathString
{
    IDictionary<string, string> GetPathString();
}
```

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2024
