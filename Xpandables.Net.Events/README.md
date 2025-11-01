# 🌐 Xpandables.Net.AspNetCore

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **ASP.NET Core Extensions** - Modular service registration, dependency injection enhancements, and endpoint routing utilities for building maintainable ASP.NET Core applications.

---

## 📋 Overview

`Xpandables.Net.AspNetCore` provides a comprehensive set of tools to simplify and enhance ASP.NET Core application development. It enables modular service registration through interfaces and MEF-based exports, supports decorator patterns for cross-cutting concerns, and offers lazy dependency resolution.

### 🎯 Key Features

- 🔌 **Modular Service Registration** - Interface-based service configuration (`IAddService`, `IUseService`)
- 📦 **MEF Integration** - Managed Extensibility Framework (MEF) for plugin-based architectures
- 🎨 **Decorator Pattern** - Apply cross-cutting concerns without modifying existing code
- ⏱️ **Lazy Resolution** - Deferred dependency instantiation with `LazyResolved<T>`
- 🛣️ **Endpoint Routing** - Simplified endpoint configuration with `IEndpointRoute`
- 🏗️ **Assembly Scanning** - Automatic service discovery and registration

---

## 🚀 Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.AspNetCore
```

### Basic Usage

```csharp
using Xpandables.Net;
using Xpandables.Net.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register lazy resolution support
builder.Services.AddXLazyResolved();

// Scan assemblies and register IAddService implementations
builder.Services.AddXServiceExports(
    builder.Configuration,
    typeof(Program).Assembly);

var app = builder.Build();

app.Run();
```

---

## 🏗️ Core Concepts

### Modular Service Registration

#### IAddService - Configure Dependencies

Implement `IAddService` to encapsulate service registration logic in modular components.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net;

public sealed class DatabaseServiceModule : IAddService
{
    public void AddServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // Register database services
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
    }
}

public sealed class CachingServiceModule : IAddService
{
    public void AddServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, InMemoryCacheService>();
    }
}
```

#### IUseService - Configure Middleware

Implement `IUseService` to modularize middleware configuration.

```csharp
using Microsoft.AspNetCore.Builder;
using Xpandables.Net;

public sealed class SecurityMiddlewareModule : IUseService
{
    public void UseServices(WebApplication application)
    {
        application.UseHttpsRedirection();
        application.UseHsts();
        application.UseCors("AllowSpecificOrigins");
    }
}

public sealed class DevelopmentMiddlewareModule : IUseService
{
    public void UseServices(WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            application.UseDeveloperExceptionPage();
            application.UseSwagger();
            application.UseSwaggerUI();
        }
    }
}
```

#### IEndpointRoute - Define Endpoints

Combine service registration, middleware, and endpoint configuration.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net;

public sealed class UserEndpoints : IEndpointRoute
{
    public void AddServices(IServiceCollection services)
    {
        // Register endpoint-specific services
        services.AddScoped<IUserService, UserService>();
    }

    public void UseServices(WebApplication application)
    {
        // Configure middleware if needed
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users");

        users.MapGet("/", async (IUserService service) =>
            await service.GetAllUsersAsync());

        users.MapGet("/{id:guid}", async (Guid id, IUserService service) =>
            await service.GetUserByIdAsync(id));

        users.MapPost("/", async (CreateUserRequest request, IUserService service) =>
            await service.CreateUserAsync(request));

        users.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IUserService service) =>
            await service.UpdateUserAsync(id, request));

        users.MapDelete("/{id:guid}", async (Guid id, IUserService service) =>
            await service.DeleteUserAsync(id));
    }
}
```

---

## 💎 Advanced Features

### Lazy Dependency Resolution

Defer expensive service instantiation until actually needed.

```csharp
using Xpandables.Net;

public sealed class OrderService
{
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<INotificationService> _notificationService;

    public OrderService(
        LazyResolved<IEmailService> emailService,
        LazyResolved<INotificationService> notificationService)
    {
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Email service only instantiated when Value is accessed
        if (order.SendConfirmation)
        {
            await _emailService.Value.SendOrderConfirmationAsync(order);
        }

        // Notification service may never be instantiated
        if (order.IsUrgent)
        {
            await _notificationService.Value.NotifyWarehouseAsync(order);
        }
    }
}

// Registration
builder.Services.AddXLazyResolved();
```

### Decorator Pattern

Apply cross-cutting concerns like logging, caching, or validation without modifying existing implementations.

```csharp
using Xpandables.Net.DependencyInjection;

// Original interface
public interface IProductService
{
    Task<Product> GetProductAsync(Guid id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
}

// Original implementation
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
        => _repository = repository;

    public async Task<Product> GetProductAsync(Guid id)
        => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _repository.GetAllAsync();
}

// Caching decorator
public sealed class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly ICacheService _cache;

    public CachedProductService(
        IProductService inner,
        ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Product> GetProductAsync(Guid id)
    {
        string key = $"product:{id}";
        
        if (_cache.TryGet<Product>(key, out var cached))
            return cached!;

        var product = await _inner.GetProductAsync(id);
        _cache.Set(key, product, TimeSpan.FromMinutes(10));
        
        return product;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _inner.GetAllProductsAsync();
}

// Logging decorator
public sealed class LoggedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly ILogger<LoggedProductService> _logger;

    public LoggedProductService(
        IProductService inner,
        ILogger<LoggedProductService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Product> GetProductAsync(Guid id)
    {
        _logger.LogInformation("Fetching product {ProductId}", id);
        
        try
        {
            var product = await _inner.GetProductAsync(id);
            _logger.LogInformation("Successfully fetched product {ProductId}", id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Fetching all products");
        return await _inner.GetAllProductsAsync();
    }
}

// Registration - decorators are applied in order
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.XTryDecorate<IProductService, CachedProductService>();
builder.Services.XTryDecorate<IProductService, LoggedProductService>();

// Resolved service: LoggedProductService -> CachedProductService -> ProductService
```

#### Decorator with Functions

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();

// Add timing decorator using a function
builder.Services.XTryDecorate<IOrderService>((inner, provider) =>
{
    var logger = provider.GetRequiredService<ILogger<IOrderService>>();
    
    return new TimedOrderService(inner, logger);
});

public sealed class TimedOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly ILogger _logger;

    public TimedOrderService(IOrderService inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await _inner.CreateOrderAsync(request);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation(
                "CreateOrderAsync completed in {ElapsedMs}ms",
                sw.ElapsedMilliseconds);
        }
    }
}
```

---

## 📦 MEF-Based Service Exports

Use Managed Extensibility Framework (MEF) for plugin-based architectures.

### Export Services from External Assemblies

```csharp
using System.ComponentModel.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net;

// In plugin assembly
[Export(typeof(IAddServiceExport))]
public sealed class PluginServiceExport : IAddServiceExport
{
    public void AddServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPluginService, PluginService>();
    }
}

// In host application
using Xpandables.Net.DependencyInjection;

builder.Services.AddXServiceExports(
    builder.Configuration,
    options =>
    {
        options.Path = Path.Combine(AppContext.BaseDirectory, "Plugins");
        options.SearchPattern = "*.Plugin.dll";
        options.SearchSubDirectories = true;
    });
```

### Export Middleware Configuration

```csharp
using System.ComponentModel.Composition;
using Microsoft.AspNetCore.Builder;
using Xpandables.Net;

[Export(typeof(IUseServiceExport))]
public sealed class PluginMiddlewareExport : IUseServiceExport
{
    public void UseServices(WebApplication application)
    {
        application.UseMiddleware<CustomPluginMiddleware>();
    }
}
```

---

## 💡 Complete Example: Modular E-Commerce API

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xpandables.Net;
using Xpandables.Net.DependencyInjection;

// === Service Modules ===

public sealed class DatabaseModule : IAddService
{
    public void AddServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ECommerce")));
    }
}

public sealed class RepositoryModule : IAddService
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
    }
}

public sealed class ServiceModule : IAddService
{
    public void AddServices(IServiceCollection services)
    {
        // Register services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        
        // Add decorators
        services.XTryDecorate<IProductService, CachedProductService>();
        services.XTryDecorate<IOrderService, LoggedOrderService>();
        
        // External services
        services.AddSingleton<IEmailService, SendGridEmailService>();
        services.AddSingleton<IPaymentService, StripePaymentService>();
    }
}

// === Endpoint Routes ===

public sealed class ProductEndpoints : IEndpointRoute
{
    public void AddServices(IServiceCollection services) { }
    
    public void UseServices(WebApplication application) { }
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/products")
            .WithTags("Products");

        products.MapGet("/", async (IProductService service) =>
            Results.Ok(await service.GetAllProductsAsync()));

        products.MapGet("/{id:guid}", async (Guid id, IProductService service) =>
        {
            var product = await service.GetProductAsync(id);
            return product is not null 
                ? Results.Ok(product) 
                : Results.NotFound();
        });

        products.MapPost("/", async (CreateProductRequest request, IProductService service) =>
        {
            var product = await service.CreateProductAsync(request);
            return Results.Created($"/api/products/{product.Id}", product);
        });
    }
}

public sealed class OrderEndpoints : IEndpointRoute
{
    public void AddServices(IServiceCollection services) { }
    
    public void UseServices(WebApplication application) { }
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        orders.MapGet("/", async (IOrderService service, ClaimsPrincipal user) =>
        {
            var customerId = Guid.Parse(user.FindFirst("sub")!.Value);
            return Results.Ok(await service.GetOrdersByCustomerAsync(customerId));
        });

        orders.MapPost("/", async (CreateOrderRequest request, IOrderService service) =>
        {
            var order = await service.CreateOrderAsync(request);
            return Results.Created($"/api/orders/{order.Id}", order);
        });
    }
}

// === Application Setup ===

var builder = WebApplication.CreateBuilder(args);

// Add lazy resolution support
builder.Services.AddXLazyResolved();

// Scan and register all IAddService implementations
builder.Services.AddXServiceExports(
    builder.Configuration,
    typeof(Program).Assembly);

// Add authentication, authorization, etc.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map all IEndpointRoute implementations
app.MapEndpointRoutes();

app.Run();
```

---

## 🔧 Extension Methods Reference

### Service Registration

```csharp
// Lazy resolution
services.AddXLazyResolved();

// Assembly scanning
services.AddXServiceExports(configuration, typeof(Program).Assembly);

// MEF-based exports
services.AddXServiceExports(configuration, options =>
{
    options.Path = "path/to/plugins";
    options.SearchPattern = "*.dll";
    options.SearchSubDirectories = true;
});
```

### Decorator Registration

```csharp
// Type-based decorator
services.XTryDecorate<TService, TDecorator>();

// Function-based decorator
services.XTryDecorate<TService>((inner, provider) => 
    new DecoratorImpl(inner));

// Generic decorator with marker interface
services.XTryDecorate<IHandler<>, LoggingHandler<>, ICommand>();
```

### Endpoint Routing

```csharp
// Map all IEndpointRoute implementations
app.MapEndpointRoutes();
```

---

## 💡 Best Practices

1. **Organize by Feature**: Group related services, endpoints, and middleware in modules
2. **Use Lazy Resolution**: Defer expensive services until needed
3. **Apply Decorators Thoughtfully**: Keep decorator order in mind (first registered wraps the original)
4. **Leverage MEF for Plugins**: Use exports for extensible, modular architectures
5. **Separate Concerns**: Keep service registration, middleware, and routing logic separate

---

## 📚 Related Packages

- **Xpandables.Net.Abstractions** - Core abstractions and interfaces
- **Xpandables.Net.ExecutionResults.AspNetCore** - Result pattern for ASP.NET Core
- **Xpandables.Net.Validators.AspNetCore** - Validation pipeline integration

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
