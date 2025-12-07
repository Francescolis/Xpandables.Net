# ?? System.Primitives.Composition

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **DI Composition** - Service export and composition utilities for Microsoft.Extensions.DependencyInjection with automatic service discovery and registration using MEF (Managed Extensibility Framework).

---

## ?? Overview

`System.Primitives.Composition` provides utilities for automatic service discovery and registration in the Microsoft DI container. It includes the `IAddServiceExport` interface for marking services for auto-registration and extension methods for scanning assemblies using MEF.

### ? Key Features

- ?? **IAddServiceExport** - Interface for MEF-based service exports
- ?? **Assembly Scanning** - Automatic discovery of services implementing IAddService
- ?? **Directory Catalogs** - Scan directories for composable parts
- ?? **Recursive Search** - RecursiveDirectoryCatalog for nested directory scanning
- ?? **Export Options** - Configurable search patterns and paths
- ?? **MEF Integration** - Built on System.ComponentModel.Composition

---

## ?? Quick Start

### Installation

```bash
dotnet add package System.Primitives.Composition
```

### Basic Setup

```csharp
using System.ComponentModel.Composition;
using System.Primitives.Composition;
using Microsoft.Extensions.DependencyInjection;

// Define a service export
[Export(typeof(IAddServiceExport))]
public class MyServiceExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register your services
        services.AddScoped<IMyService, MyService>();
        services.AddSingleton<ICache, MemoryCache>();
    }
}

// Register all exports in Program.cs
var builder = WebApplication.CreateBuilder(args);

// Scan and register all IAddServiceExport implementations
builder.Services.AddXServiceExports(builder.Configuration);
```

---

## ?? Core Concepts

### IAddServiceExport Interface

The `IAddServiceExport` interface marks classes for automatic service registration using MEF:

```csharp
using System.ComponentModel.Composition;
using System.Primitives.Composition;

/// <summary>
/// Registers authentication services.
/// </summary>
[Export(typeof(IAddServiceExport))]
public class AuthenticationServiceExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration
            .GetSection("Authentication")
            .Get<AuthSettings>();
            
        services.AddSingleton(authSettings!);
        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddScoped<ITokenValidator, TokenValidator>();
    }
}
```

### Assembly Scanning

Scan specific assemblies for `IAddService` implementations:

```csharp
// Scan calling assembly
builder.Services.AddXServiceExports(builder.Configuration);

// Scan specific assemblies
builder.Services.AddXServiceExports(
    builder.Configuration,
    typeof(MyServiceExport).Assembly,
    typeof(OtherExport).Assembly);
```

### Directory Catalogs

Use `ExportOptions` to configure directory-based scanning:

```csharp
builder.Services.AddXServiceExports(
    builder.Configuration,
    options =>
    {
        options.Path = "./plugins";
        options.SearchPattern = "*.Plugin.dll";
        options.SearchSubDirectories = true;
    });
```

### RecursiveDirectoryCatalog

For scanning nested directories:

```csharp
using System.Primitives.Composition;

// Recursively scan a directory for composable parts
using var catalog = new RecursiveDirectoryCatalog(
    path: "./modules",
    searchPattern: "*.Module.dll");

// Access discovered parts
foreach (var part in catalog.Parts)
{
    Console.WriteLine($"Found: {part}");
}
```

---

## ?? Real-World Examples

### Modular Application Setup

```csharp
// Infrastructure module
[Export(typeof(IAddServiceExport))]
public class InfrastructureExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));
        
        // Caching
        services.AddDistributedMemoryCache();
        
        // Logging
        services.AddLogging();
    }
}

// Domain module
[Export(typeof(IAddServiceExport))]
public class DomainExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IInventoryService, InventoryService>();
    }
}

// Application startup
builder.Services.AddXServiceExports(builder.Configuration);
// All exports are automatically discovered and registered
```

### Plugin Architecture

```csharp
// Plugin interface
public interface IPlugin
{
    string Name { get; }
    void Execute();
}

// Plugin export
[Export(typeof(IAddServiceExport))]
public class ReportingPluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPlugin, PdfReportPlugin>();
        services.AddScoped<IPlugin, ExcelReportPlugin>();
    }
}

// Load plugins from directory
builder.Services.AddXServiceExports(
    builder.Configuration,
    options =>
    {
        options.Path = Path.Combine(AppContext.BaseDirectory, "plugins");
        options.SearchPattern = "*.Plugin.dll";
        options.SearchSubDirectories = true;
    });
```

---

## ? Best Practices

1. **Use MEF Export attribute** - Always decorate exports with `[Export(typeof(IAddServiceExport))]`
2. **Keep exports focused** - One export per module/feature area
3. **Use configuration** - Leverage IConfiguration for environment-specific settings
4. **Handle missing assemblies** - Wrap scanning in try-catch for plugin scenarios
5. **Document exports** - Add XML comments describing what services are registered

---

## ?? Related Packages

- **System.Primitives** - Core primitives and utilities
- **Microsoft.Extensions.DependencyInjection** - DI container
- **System.ComponentModel.Composition** - MEF framework

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
