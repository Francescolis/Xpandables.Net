# 🧩 System.Composition

[![NuGet](https://img.shields.io/badge/NuGet-10.0.0-blue.svg)](https://www.nuget.org/packages/System.Composition)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **MEF-Based Plugin Service Registration** — Discover and register services from external assemblies using the Managed Extensibility Framework (MEF) with automatic directory scanning and dependency injection integration.

---

## 📋 Overview

`System.Composition` enables modular service registration through MEF (Managed Extensibility Framework). The library provides `IAddServiceExport` for plugin-style service discovery, `RecursiveDirectoryCatalog` for scanning assembly directories, and extension methods for automatic service registration from external DLLs.

Built for .NET 10 with C# 14 extension members, this package enables building extensible applications where plugins can self-register their services without requiring compile-time references.

### ✨ Key Features

- 🔌 **`IAddServiceExport`** — MEF export interface for plugin service registration
- 📂 **`RecursiveDirectoryCatalog`** — Scan directories and subdirectories for assemblies
- ⚙️ **`ExportOptions`** — Configure path, search pattern, and subdirectory scanning
- 🔍 **Assembly Scanning** — Auto-discover `IAddService` implementations from assemblies
- 💉 **DI Integration** — Seamless Microsoft.Extensions.DependencyInjection support
- 📦 **Plugin Architecture** — External libraries register services without host modification

---

## 📦 Installation

```bash
dotnet add package System.Composition
```

Or via NuGet Package Manager:

```powershell
Install-Package System.Composition
```

---

## 🚀 Quick Start

### Create a Plugin (External Library)

```csharp
// In your plugin project (e.g., MyPlugin.dll)
using System.ComponentModel.Composition;
using System.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[Export(typeof(IAddServiceExport))]
public sealed class MyPluginServiceExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register plugin-specific services
        services.AddScoped<IMyPluginService, MyPluginService>();
        services.AddSingleton<IPluginRepository, PluginRepository>();
        
        // Use configuration
        var pluginSettings = configuration.GetSection("MyPlugin");
        services.Configure<MyPluginOptions>(pluginSettings);
    }
}

public interface IMyPluginService
{
    Task<string> GetPluginDataAsync(CancellationToken ct);
}

public sealed class MyPluginService : IMyPluginService
{
    public Task<string> GetPluginDataAsync(CancellationToken ct)
    {
        return Task.FromResult("Data from plugin");
    }
}
```

### Register Plugins in Host Application

```csharp
// In your host application (e.g., ASP.NET Core)
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register all plugin services from current directory
builder.Services.AddXServiceExports(builder.Configuration);

var app = builder.Build();

// Use plugin services
app.MapGet("/plugin-data", async (IMyPluginService pluginService, CancellationToken ct) =>
{
    var data = await pluginService.GetPluginDataAsync(ct);
    return Results.Ok(data);
});

app.Run();
```

---

## 🔌 Plugin Development

### Basic Plugin Export

```csharp
using System.ComponentModel.Composition;
using System.Primitives.Composition;
using Microsoft.Extensions.DependencyInjection;

// Decorate with Export attribute - required for MEF discovery
[Export(typeof(IAddServiceExport))]
public sealed class PaymentPluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();
        services.AddScoped<IPaymentValidator, PaymentValidator>();
    }
}
```

### Plugin with Configuration

```csharp
using System.ComponentModel.Composition;
using System.Primitives.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[Export(typeof(IAddServiceExport))]
public sealed class EmailPluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration section
        var emailSettings = configuration.GetSection("Email");
        services.Configure<EmailOptions>(emailSettings);
        
        // Register services with configured options
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailTemplateEngine, RazorTemplateEngine>();
        
        // Conditional registration based on config
        if (configuration.GetValue<bool>("Email:EnableQueue"))
        {
            services.AddHostedService<EmailQueueProcessor>();
        }
    }
}

public sealed record EmailOptions
{
    public string SmtpHost { get; init; } = "localhost";
    public int SmtpPort { get; init; } = 587;
    public string FromAddress { get; init; } = string.Empty;
    public bool EnableQueue { get; init; }
}
```

### Multiple Exports per Assembly

```csharp
// Multiple plugins can exist in the same assembly
[Export(typeof(IAddServiceExport))]
public sealed class AuthenticationPluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
    }
}

[Export(typeof(IAddServiceExport))]
public sealed class AuthorizationPluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthorizationService, RoleBasedAuthorizationService>();
        services.AddSingleton<IPolicyProvider, DatabasePolicyProvider>();
    }
}
```

---

## ⚙️ Export Options Configuration

### Default Behavior

```csharp
// Scans current application directory for *.dll files
builder.Services.AddXServiceExports(builder.Configuration);
```

### Custom Path and Pattern

```csharp
builder.Services.AddXServiceExports(builder.Configuration, options =>
{
    // Scan specific plugins directory
    options.Path = Path.Combine(AppContext.BaseDirectory, "plugins");
    
    // Only load assemblies matching pattern
    options.SearchPattern = "MyApp.Plugin.*.dll";
    
    // Enable recursive scanning of subdirectories
    options.SearchSubDirectories = true;
});
```

### Platform-Specific Plugin Paths

```csharp
builder.Services.AddXServiceExports(builder.Configuration, options =>
{
    options.Path = OperatingSystem.IsWindows()
        ? @"C:\Program Files\MyApp\Plugins"
        : "/opt/myapp/plugins";
    
    options.SearchPattern = "*.Plugin.dll";
    options.SearchSubDirectories = true;
});
```

---

## 📂 Directory Catalogs

### RecursiveDirectoryCatalog

The `RecursiveDirectoryCatalog` scans a directory and all its subdirectories for assemblies:

```csharp
using System.Primitives.Composition;
using System.ComponentModel.Composition.Hosting;

// Scan plugins folder and all subdirectories
var pluginsPath = Path.Combine(AppContext.BaseDirectory, "plugins");

using var catalog = new RecursiveDirectoryCatalog(pluginsPath, "*.dll");

// catalog.Parts contains all composable parts found
foreach (var part in catalog.Parts)
{
    Console.WriteLine($"Found: {part}");
}
```

### Directory Structure Example

```
/app
├── MyApp.exe
├── appsettings.json
└── plugins/
    ├── payments/
    │   └── MyApp.Plugin.Payments.dll
    ├── notifications/
    │   └── MyApp.Plugin.Notifications.dll
    └── reporting/
        └── MyApp.Plugin.Reporting.dll
```

With `SearchSubDirectories = true`, all three plugin DLLs are discovered.

---

## 🔍 Assembly Scanning (Without MEF)

For scenarios where MEF attributes aren't used, scan assemblies directly:

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

// Scan specific assemblies for IAddService implementations
builder.Services.AddXServiceExports(
    builder.Configuration,
    typeof(PaymentPluginExport).Assembly,
    typeof(EmailPluginExport).Assembly);

// Scan calling assembly (useful in tests)
builder.Services.AddXServiceExports(builder.Configuration);
```

### IAddService without MEF Export

```csharp
using System.Composition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

// No [Export] attribute needed when using assembly scanning
public sealed class LoggingServiceRegistration : IAddService
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
        });
    }
}
```

---

## 💡 Common Patterns

### Feature-Based Plugin Architecture

```csharp
// Base feature interface
public interface IFeaturePlugin
{
    string FeatureName { get; }
    bool IsEnabled(IConfiguration configuration);
}

// Plugin export with feature check
[Export(typeof(IAddServiceExport))]
public sealed class AdvancedReportingPluginExport : IAddServiceExport, IFeaturePlugin
{
    public string FeatureName => "AdvancedReporting";
    
    public bool IsEnabled(IConfiguration configuration)
    {
        return configuration.GetValue<bool>($"Features:{FeatureName}:Enabled");
    }

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        if (!IsEnabled(configuration))
        {
            return; // Skip registration if feature disabled
        }

        services.AddScoped<IReportGenerator, AdvancedReportGenerator>();
        services.AddScoped<IReportExporter, PdfReportExporter>();
        services.AddScoped<IReportScheduler, CronReportScheduler>();
    }
}
```

### Plugin with Health Checks

```csharp
[Export(typeof(IAddServiceExport))]
public sealed class DatabasePluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PluginDatabase");
        
        services.AddDbContext<PluginDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddScoped<IPluginRepository, PluginRepository>();
        
        // Add health check for plugin
        services.AddHealthChecks()
            .AddSqlServer(connectionString, name: "plugin-database");
    }
}
```

### Plugin Dependency Chain

```csharp
// Core plugin (loaded first)
[Export(typeof(IAddServiceExport))]
public sealed class CorePluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICoreService, CoreService>();
        services.AddSingleton<IPluginContext, PluginContext>();
    }
}

// Feature plugin (depends on core)
[Export(typeof(IAddServiceExport))]
public sealed class FeaturePluginExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // This plugin uses services from CorePluginExport
        services.AddScoped<IFeatureService>(sp =>
        {
            var coreService = sp.GetRequiredService<ICoreService>();
            var context = sp.GetRequiredService<IPluginContext>();
            return new FeatureService(coreService, context);
        });
    }
}
```

---

## 📊 API Reference

### IAddServiceExport Interface

```csharp
// Must be decorated with [Export(typeof(IAddServiceExport))]
public interface IAddServiceExport : IAddService
{
}
```

### IAddService Interface

```csharp
public interface IAddService
{
    // Simple registration
    void AddServices(IServiceCollection services);
    
    // Registration with configuration
    void AddServices(IServiceCollection services, IConfiguration configuration);
}
```

### ExportOptions Class

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Path` | `string` | Application directory | Directory to scan for assemblies |
| `SearchPattern` | `string` | `"*.dll"` | File pattern for assembly discovery |
| `SearchSubDirectories` | `bool` | `false` | Enable recursive directory scanning |

### Extension Methods

| Method | Description |
|--------|-------------|
| `AddXServiceExports(IConfiguration)` | Scan current directory with defaults |
| `AddXServiceExports(IConfiguration, Action<ExportOptions>)` | Scan with custom options |
| `AddXServiceExports(IConfiguration, params Assembly[])` | Scan specific assemblies |

---

## ✅ Best Practices

### ✅ Do

- **Decorate exports with `[Export(typeof(IAddServiceExport))]`** — Required for MEF discovery
- **Use `AddServices(IServiceCollection, IConfiguration)` overload** — Access configuration in plugins
- **Organize plugins in subdirectories** — Use `SearchSubDirectories = true`
- **Check configuration before registering** — Conditional feature registration
- **Keep plugins focused** — One plugin per feature area
- **Add health checks in plugins** — Self-contained monitoring

### ❌ Don't

- **Forget the `[Export]` attribute** — MEF won't discover the type
- **Use static state in plugins** — Leads to threading issues
- **Assume load order** — Plugin load order is not guaranteed
- **Reference host internals** — Plugins should be self-contained
- **Throw from `AddServices`** — Wrap in try-catch and log

---

## ⚠️ Notes

- **Requires assembly files** — `RequiresAssemblyFiles` attribute on MEF methods
- **Not AOT compatible** — MEF uses reflection and dynamic code
- **Load order undefined** — Plugins are discovered in directory order

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Primitives** | Core primitives with `IAddService` and `ExportOptions` |
| **System.ComponentModel.Composition** | MEF core library |
| **Microsoft.Extensions.DependencyInjection** | DI abstractions |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).
