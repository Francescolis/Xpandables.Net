# System.Composition

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Composition.svg)](https://www.nuget.org/packages/Xpandables.Composition)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Composition.svg)](https://www.nuget.org/packages/Xpandables.Composition)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

MEF-based modular service composition for .NET dependency injection.

## 📖 Overview

`System.Composition` (NuGet: **Xpandables.Composition**) provides `IAddServiceExport` and MEF catalog types for discovering and registering services from external assemblies using the Managed Extensibility Framework (`System.ComponentModel.Composition`). Namespace: `System.Composition`.

Built for **.NET 10** and **C# 14**.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `IAddServiceExport` | `IAddServiceExport.cs` | MEF export interface — decorate with `[Export(typeof(IAddServiceExport))]` |
| `EmptyCatalog` | `ExportCatalogs.cs` | Empty `ComposablePartCatalog` |
| `RecursiveDirectoryCatalog` | `ExportCatalogs.cs` | Recursively scans directories for assemblies containing exports |

### ⚙️ Dependency Injection

C# 14 extension members on `IServiceCollection`:

```csharp
services.AddXServiceExports(configuration);
services.AddXServiceExports(configuration, options =>
{
    options.SearchPattern = "*.dll";
    options.Path = "/plugins";
});
```

## 📦 Installation

```bash
dotnet add package Xpandables.Composition
```

**Dependencies:** `System.ComponentModel.Composition`  
**Project References:** `Xpandables.Primitives`

## 🚀 Quick Start

### Define an Export

```csharp
using System.ComponentModel.Composition;
using System.Composition;

[Export(typeof(IAddServiceExport))]
public class MyModuleExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMyService, MyService>();
    }
}
```

### Register at Startup

```csharp
builder.Services.AddXServiceExports(builder.Configuration);
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
