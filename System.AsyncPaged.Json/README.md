# System.AsyncPaged.Json

[![NuGet](https://img.shields.io/nuget/v/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.AsyncPaged.Json.svg)](https://www.nuget.org/packages/Xpandables.AsyncPaged.Json)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

JSON serialization and deserialization for `IAsyncPagedEnumerable<T>` using `System.Text.Json`.

## 📖 Overview

`System.AsyncPaged.Json` (NuGet: **Xpandables.AsyncPaged.Json**) provides C# 14 extension members on `JsonSerializer` for streaming paged async enumerables to/from JSON via `PipeWriter` and `Stream`. It also includes `HttpContent` deserialization helpers. Namespace: `System.Text.Json`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `JsonSerializerExtensions` | `JsonSerializerExtensions.cs` | `SerializeAsyncPaged` — writes `IAsyncPagedEnumerable<T>` as JSON to `PipeWriter` (with `JsonTypeInfo<T>` or `JsonSerializerOptions`) |
| `JsonDeserializerExtensions` | `JsonDeserializerExtensions.cs` | `DeserializeAsyncPaged` — reads JSON stream back into `IAsyncPagedEnumerable<T>` |
| `HttpContentExtensions` | `HttpContentExtensions.cs` | `ReadAsAsyncPagedEnumerable` — deserialize from `HttpContent` |

## 📦 Installation

```bash
dotnet add package Xpandables.AsyncPaged.Json
```

**Project References:** `Xpandables.AsyncPaged`

## 🚀 Quick Start

### Serialize to PipeWriter

```csharp
using System.Text.Json;
using System.IO.Pipelines;

await JsonSerializer.SerializeAsyncPaged(
    pipeWriter,
    pagedEnumerable,
    jsonSerializerOptions,
    cancellationToken);
```

### Deserialize from HttpContent

```csharp
IAsyncPagedEnumerable<Product> products =
    await httpContent.ReadAsAsyncPagedEnumerable<Product>(options, ct);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
