# ?? AspNetCore.Net.Composition

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **MEF Integration for ASP.NET Core** - Managed Extensibility Framework (MEF) support for service discovery and composition in ASP.NET Core applications.

---

## ?? Overview

`AspNetCore.Net.Composition` enables plugin-based architectures in ASP.NET Core using MEF (Managed Extensibility Framework). It provides automatic discovery and registration of services from external assemblies, making applications more modular and extensible.

### ? Key Features

- ?? **IUseServiceExport** - MEF-based service export interface for automatic discovery
- ?? **Assembly Scanning** - Automatic service registration from referenced assemblies
- ?? **Plugin Architecture** - Load services from external DLLs
- ?? **UseXServiceExports** - Apply discovered services to WebApplication
- ??? **Modular Design** - Build extensible applications with clear contracts

---

## ?? Quick Start

### Installation

```bash
dotnet add package AspNetCore.Net.Composition
dotnet add package System.Composition.AttributedModel
```

### Basic Setup

```csharp
using AspNetCore.Net;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Automatically discover and apply all IUseServiceExport implementations
app.UseXServiceExports();

app.Run();
```

---

## ?? Core Features

### Creating Service Exports

```csharp
using System.ComponentModel.Composition;
using AspNetCore.Net;
using Microsoft.AspNetCore.Builder;

// Define a service export
[Export(typeof(IUseServiceExport))]
public class MyServiceExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        // Configure middleware
        app.UseRouting();
        
        // Add custom middleware
        app.UseMiddleware<MyCustomMiddleware>();
        
        // Map endpoints
        app.MapGet("/api/hello", () => "Hello from export!");
    }
}
```

### Multiple Service Exports

```csharp
// Authentication export
[Export(typeof(IUseServiceExport))]
public class AuthenticationExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}

// API endpoints export
[Export(typeof(IUseServiceExport))]
public class ApiEndpointsExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        var api = app.MapGroup("/api");
        
        api.MapGet("/users", GetUsers);
        api.MapPost("/users", CreateUser);
        api.MapGet("/users/{id}", GetUserById);
    }
    
    private async Task<IResult> GetUsers(
        [FromServices] IUserService service) =>
        Results.Ok(await service.GetAllAsync());
}
```

### Configuration Options

```csharp
using Microsoft.AspNetCore.Builder;

var app = builder.Build();

// Use exports with configuration
app.UseXServiceExports(options =>
{
    // Configure export discovery options
    options.Assemblies = [typeof(MyPlugin).Assembly];
    options.SearchPattern = "*.Plugin.dll";
});

app.Run();
```

---

## ?? Advanced Scenarios

### Plugin-Based Architecture

```csharp
// Plugin interface in shared library
namespace MyApp.Plugins;

public interface IPlugin : IUseServiceExport
{
    string Name { get; }
    string Version { get; }
}

// Plugin implementation in external assembly
[Export(typeof(IUseServiceExport))]
[Export(typeof(IPlugin))]
public class PaymentPlugin : IPlugin
{
    public string Name => "Payment Plugin";
    public string Version => "1.0.0";
    
    public void UseServices(WebApplication app)
    {
        app.MapGroup("/api/payments")
            .MapGet("/", GetPayments)
            .MapPost("/", ProcessPayment);
    }
    
    private async Task<IResult> GetPayments(
        [FromServices] IPaymentService service) =>
        Results.Ok(await service.GetAllAsync());
    
    private async Task<IResult> ProcessPayment(
        PaymentRequest request,
        [FromServices] IPaymentService service) =>
        Results.Ok(await service.ProcessAsync(request));
}
```

### Assembly Scanning

```csharp
using System.Reflection;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Scan specific assemblies
app.UseXServices(
    typeof(Plugin1).Assembly,
    typeof(Plugin2).Assembly,
    Assembly.LoadFrom("MyApp.Extensions.dll"));

app.Run();
```

### Conditional Service Registration

```csharp
[Export(typeof(IUseServiceExport))]
public class ConditionalExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        if (app.Configuration
            .GetValue<bool>("Features:EnableHealthChecks"))
        {
            app.MapHealthChecks("/health");
        }
    }
}
```

### Complex Endpoint Export

```csharp
[Export(typeof(IUseServiceExport))]
public class OrderEndpointsExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        var orders = app.MapGroup("/api/orders")
            .RequireAuthorization()
            .WithTags("Orders");
        
        orders.MapGet("/", GetOrders)
            .WithName("GetOrders")
            .Produces<List<Order>>();
        
        orders.MapGet("/{id}", GetOrderById)
            .WithName("GetOrder")
            .Produces<Order>()
            .ProducesProblem(404);
        
        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .Accepts<CreateOrderRequest>("application/json")
            .Produces<Order>(201);
        
        orders.MapPut("/{id}", UpdateOrder)
            .WithName("UpdateOrder");
        
        orders.MapDelete("/{id}", DeleteOrder)
            .WithName("DeleteOrder");
    }
    
    private async Task<IResult> GetOrders(
        [FromServices] IOrderService service,
        CancellationToken cancellationToken)
    {
        var orders = await service.GetAllAsync(cancellationToken);
        return Results.Ok(orders);
    }
    
    private async Task<IResult> GetOrderById(
        Guid id,
        [FromServices] IOrderService service,
        CancellationToken cancellationToken)
    {
        var order = await service.GetByIdAsync(id, cancellationToken);
        return order != null 
            ? Results.Ok(order) 
            : Results.NotFound();
    }
    
    // Other handlers...
}
```

---

## ??? Real-World Example

### Multi-Module Application

```csharp
// Module 1: User Management
[Export(typeof(IUseServiceExport))]
public class UserModuleExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        var users = app.MapGroup("/api/users")
            .WithTags("Users");
        
        users.MapGet("/", GetUsers);
        users.MapPost("/", CreateUser);
        users.MapGet("/{id}", GetUser);
        users.MapPut("/{id}", UpdateUser);
        users.MapDelete("/{id}", DeleteUser);
    }
}

// Module 2: Product Catalog
[Export(typeof(IUseServiceExport))]
public class ProductModuleExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        var products = app.MapGroup("/api/products")
            .WithTags("Products");
        
        products.MapGet("/", GetProducts);
        products.MapGet("/{id}", GetProduct);
        products.MapPost("/", CreateProduct);
        products.MapPut("/{id}", UpdateProduct);
        products.MapDelete("/{id}", DeleteProduct);
        products.MapGet("/search", SearchProducts);
    }
}

// Module 3: Order Processing
[Export(typeof(IUseServiceExport))]
public class OrderModuleExport : IUseServiceExport
{
    public void UseServices(WebApplication app)
    {
        var orders = app.MapGroup("/api/orders")
            .RequireAuthorization()
            .WithTags("Orders");
        
        orders.MapGet("/", GetOrders);
        orders.MapPost("/", CreateOrder);
        orders.MapGet("/{id}/status", GetOrderStatus);
        orders.MapPost("/{id}/cancel", CancelOrder);
    }
}

// Main application
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// All modules are automatically discovered and registered
app.UseXServiceExports();

app.Run();
```

---

## ?? Best Practices

1. **Use clear naming** - Name exports according to their purpose (e.g., `AuthenticationExport`, `ApiEndpointsExport`)
2. **Keep exports focused** - Each export should handle one concern
3. **Version your plugins** - Include version information in plugin exports
4. **Document dependencies** - Clearly specify what services exports require
5. **Test in isolation** - Test each export independently
6. **Handle missing dependencies** - Check for required services before using them

---

## ?? Trimming Considerations

When publishing with trimming enabled, ensure your service exports are preserved:

```xml
<ItemGroup>
  <TrimmerRootAssembly Include="MyApp.Plugins" />
</ItemGroup>
```

Or use dynamic attributes:

```csharp
[RequiresUnreferencedCode("Service exports use reflection")]
app.UseXServiceExports();
```

---

## ?? Related Packages

- **AspNetCore.Net** - Core ASP.NET integration
- **System.Primitives.Composition** - Core composition abstractions
- **System.Composition.AttributedModel** - MEF attributes

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
