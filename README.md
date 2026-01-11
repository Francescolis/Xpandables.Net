<div align="center">

# 📦 Xpandables.Net

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)

**A comprehensive, modular .NET library for building modern, scalable applications with clean architecture patterns**

[Features](#-key-features) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Architecture](#-architecture) • [Contributing](#-contributing)

</div>

---

## 📋 Overview

**Xpandables.Net** is a powerful, production-ready library ecosystem designed to accelerate .NET application development while promoting best practices, clean architecture, and maintainable code. Built with **.NET 10** and leveraging the latest C# language features, it provides a rich set of tools for enterprise-grade applications.

### 💡 Philosophy

- **🧩 Modular Design**: Pick only what you need - each package is independently versioned and maintained
- **⚡ Performance First**: Built for high-performance scenarios with AOT compatibility
- **🔒 Type Safety**: Leverage strong typing to catch errors at compile time
- **✅ Testability**: Designed with unit testing and integration testing in mind
- **📖 Well Documented**: Comprehensive XML documentation for IntelliSense support

---

## 🎯 Key Features

### Core Capabilities

| Feature | Description | Package |
|---------|-------------|---------|
| ✅ **Result Pattern** | Robust error handling with HTTP-aware result types | [Xpandables.Results](./System.Results/README.md) |
| 🎁 **Optional Values** | Null-safe optional value handling (like Rust's Option) | [Xpandables.Optionals](./System.Optionals/README.md) |
| 📡 **Mediator Pattern** | CQRS and request/response pipeline implementation | [Xpandables.Results.Tasks](./System.Results.Tasks/README.md) |
| 🔗 **Pipeline Decorators** | Validation, transaction, and event decorators | [Xpandables.Results.Pipelines](./System.Results.Pipelines/README.md) |
| ✔️ **Validation** | Flexible validation framework with specifications | [Xpandables.Validation](./System.Validation/README.md) |
| 💾 **Repository Pattern** | Generic repository with unit of work support | [Xpandables.Entities.Data](./System.Entities.Data/README.md) |
| 🌐 **REST Client** | Type-safe, attribute-based HTTP client | [Xpandables.Rests](./System.Rests/README.md) |
| 📝 **Event Sourcing** | Complete event sourcing with W3C trace context support | [Xpandables.Events](./System.Events/README.md) |
| 🗄️ **Event Store** | EF Core event store with outbox pattern | [Xpandables.Events.Data](./System.Events.Data/README.md) |
| 🔄 **Async Paging** | Asynchronous enumerable extensions and pagination | [Xpandables.AsyncPaged](./System.AsyncPaged/README.md) |
| 🌐 **W3C Trace Context** | Distributed tracing with traceparent header support | [AspNetCore.Events](./AspNetCore.Events/README.md) |

---

## 🚀 Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **Visual Studio 2026 (or later)** or **JetBrains Rider 2025.3+** (recommended)
- Basic understanding of C# and async/await patterns

### Installation

Install the packages you need via NuGet Package Manager:

```bash
# Core operation result handling
dotnet add package Xpandables.Results

# Optional value handling
dotnet add package Xpandables.Optionals

# Mediator and pipeline support
dotnet add package Xpandables.Results.Tasks

# Pipeline decorators
dotnet add package Xpandables.Results.Pipelines

# Validation framework
dotnet add package Xpandables.Validation

# Repository pattern with EF Core
dotnet add package Xpandables.Entities.Data

# REST client
dotnet add package Xpandables.Rests

# Event sourcing
dotnet add package Xpandables.Events
dotnet add package Xpandables.Events.Data

# Async paging
dotnet add package Xpandables.AsyncPaged
dotnet add package Xpandables.AsyncPaged.Linq
dotnet add package Xpandables.AsyncPaged.Json

# ASP.NET Core integration
dotnet add package Xpandables.AspNetCore.Net
dotnet add package Xpandables.AspNetCore.AsyncPaged
dotnet add package Xpandables.AspNetCore.Events
dotnet add package Xpandables.AspNetCore.Results
dotnet add package Xpandables.AspNetCore.Composition
```

### Quick Example

```csharp
using System.Results;
using System.Results.Requests;
using System.Optionals;

// Define a request
public sealed record GetUserQuery(Guid UserId) : IRequest<User>;

// Handle the request
public sealed class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _repository;
    
    public GetUserHandler(IUserRepository repository) 
        => _repository = repository;
    
    public async Task<Result<User>> HandleAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        // Fetch user with Optional to handle null cases
        Optional<User> user = await _repository
            .FindByIdAsync(request.UserId, cancellationToken);
        
        return user
            .Bind(u => Result.Success(u).Build())
            .Empty(() => Result
                .NotFound<User>("userId", "User not found"));
    }
}
```

---

## 📚 Documentation

### Package Documentation

Each package has detailed documentation with examples and API references:

#### Foundation Packages
- 🔧 [**Xpandables.Primitives**](./System.Primitives/README.md) - Core primitives and utilities
- ✅ [**Xpandables.Results**](./System.Results/README.md) - Result pattern handling with request/handler pattern
- 🎁 [**Xpandables.Optionals**](./System.Optionals/README.md) - Null-safe optional value types

#### Application Layer
- 📡 [**Xpandables.Results.Tasks**](./System.Results.Tasks/README.md) - Mediator pattern and request dispatching
- 🔗 [**Xpandables.Results.Pipelines**](./System.Results.Pipelines/README.md) - Pipeline decorators for validation, transactions, events
- ✔️ [**Xpandables.Validation**](./System.Validation/README.md) - Specification pattern and rule validators
- 🧩 [**Xpandables.Composition**](./System.Composition/README.md) - MEF-based service composition

#### Data Access
- 💾 [**Xpandables.Entities.Data**](./System.Entities.Data/README.md) - EF Core repository with DataContext

#### Event Handling
- 📝 [**Xpandables.Events**](./System.Events/README.md) - Event sourcing and domain events
- 🗄️ [**Xpandables.Events.Data**](./System.Events.Data/README.md) - EF Core event store implementation

#### HTTP & REST
- 🌐 [**Xpandables.Rests**](./System.Rests/README.md) - Type-safe REST client with attribute-based routing

#### Async & Utilities
- 🔄 [**Xpandables.AsyncPaged**](./System.AsyncPaged/README.md) - Async paged collections
- 🔄 [**Xpandables.AsyncPaged.Linq**](./System.AsyncPaged.Linq/README.md) - LINQ extensions for async paging
- 📄 [**Xpandables.AsyncPaged.Json**](./System.AsyncPaged.Json/README.md) - JSON serialization for paged data

#### ASP.NET Core Integration
- 🌐 [**Xpandables.AspNetCore.Net**](./AspNetCore.Net/README.md) - ASP.NET Core minimal API with endpoint routing
- 📄 [**Xpandables.AspNetCore.AsyncPaged**](./AspNetCore.AsyncPaged/README.md) - Async paged response formatters
- 🌐 [**Xpandables.AspNetCore.Events**](./AspNetCore.Events/README.md) - W3C trace context middleware
- ✅ [**Xpandables.AspNetCore.Results**](./AspNetCore.Results/README.md) - Result type HTTP integrations
- 🔌 [**Xpandables.AspNetCore.Composition**](./AspNetCore.Composition/README.md) - MEF-based service composition

---

## 🏗️ Architecture

Xpandables.Net follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (AspNetCore.Net, AspNetCore.Events,    │
│   AspNetCore.AsyncPaged,                │
│   AspNetCore.Results)                   │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Application Layer               │
│  (Xpandables.Results, Xpandables.Results.Tasks, │
│   Xpandables.Results.Pipelines,         │
│   Xpandables.Composition)               │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│           Domain Layer                  │
│ (Xpandables.Events, Xpandables.Optionals,       │
│  Xpandables.Primitives, Xpandables.Validation,  │
│  Xpandables.AsyncPaged)                 │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│       Infrastructure Layer              │
│  (Xpandables.Entities.Data, Xpandables.Events.  │
│   Data, Xpandables.Rests,               │
│   Xpandables.AsyncPaged.Linq)           │
└─────────────────────────────────────────┘
```

### Design Patterns Implemented

- 📂 **Repository Pattern** - Abstraction over data access
- 🔄 **Unit of Work** - Transaction management
- 📡 **Mediator Pattern** - Decoupled request handling
- 🎨 **Decorator Pattern** - Pipeline behaviors
- 📋 **Specification Pattern** - Business rule encapsulation
- 📝 **Event Sourcing** - Event-driven architecture
- 🔀 **CQRS** - Command Query Responsibility Segregation
- ✅ **Result Pattern** - Functional error handling

---

## 💎 Highlights

### 1. Operation Results - Railway Oriented Programming

```csharp
using System.Results;

public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    // Validate
    if (string.IsNullOrEmpty(request.CustomerId))
        return Result.Failure<Order>("customerId", "Customer ID is required");

    // Create order
    var order = new Order { CustomerId = request.CustomerId };
    await _repository.AddAsync(order);

    return Result.Created(order)
        .WithLocation($"/api/orders/{order.Id}");
}
```

### 2. Type-Safe REST Client

```csharp
using System.Rests;

[RestPost("/api/users")]
public sealed record CreateUserRequest(string Name, string Email) 
    : IRestRequest<User>, IRestString;

var response = await _restClient.SendAsync(
    new CreateUserRequest("John", "john@example.com"));
    
var user = response.Result;
```

### 3. Fluent Validation with Specifications

```csharp
using System.ComponentModel.DataAnnotations;

// Create specifications using factory methods
var nameSpec = Specification.IsNotNull<User, string>(u => u.Name);
var emailSpec = Specification.Contains<User>(u => u.Email, "@");
var ageSpec = Specification.GreaterThan<User, int>(u => u.Age, 18);

// Combine specifications
var validUser = Specification.All(nameSpec, emailSpec, ageSpec);

// Check if satisfied
if (validUser.IsSatisfiedBy(user))
{
    Console.WriteLine("User meets all criteria");
}

// Use in LINQ queries
var activeAdults = users.Where(validUser.Expression.Compile());
```

### 4. Event Sourcing with Aggregates

```csharp
using System.Events.Aggregates;

public sealed class OrderAggregate : Aggregate
{
    public string OrderNumber { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }

    public static OrderAggregate Create(string orderNumber, decimal amount)
    {
        var aggregate = new OrderAggregate();
        aggregate.AppendEvent(new OrderCreatedEvent
        {
            OrderNumber = orderNumber,
            Amount = amount
        });
        return aggregate;
    }

    public void AddItem(string productId, decimal price)
    {
        AppendEvent(new ItemAddedEvent { ProductId = productId, Price = price });
    }

    // Event handlers (called automatically)
    private void On(OrderCreatedEvent evt)
    {
        OrderNumber = evt.OrderNumber;
        TotalAmount = evt.Amount;
    }

    private void On(ItemAddedEvent evt)
    {
        TotalAmount += evt.Price;
    }
}

// Usage
var order = OrderAggregate.Create("ORD-001", 100m);
order.AddItem("PROD-1", 25m);
await _aggregateStore.AppendAsync(order);
```

### 5. W3C Trace Context for Distributed Tracing

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register event context with W3C trace support
builder.Services.AddXEventContext();
builder.Services.AddXEventContextMiddleware();

var app = builder.Build();

// Middleware reads traceparent header (W3C format) and establishes EventContext
app.UseXEventContextMiddleware();

app.MapPost("/api/orders", async (
    CreateOrderRequest request,
    IEventContextAccessor context,
    CancellationToken ct) =>
{
    // CorrelationId is automatically populated from traceparent header
    // or Activity.Current.Id, or generated as GUID string
    var correlationId = context.Current.CorrelationId; // W3C trace ID string

    // Events inherit correlation/causation for distributed tracing
    var order = OrderAggregate.Create(request.OrderNumber, request.Amount);
    await orderStore.SaveAsync(order, ct);

    return Results.Created($"/api/orders/{order.StreamId}", order);
});

app.Run();
```

---

## 🧪 Testing

The library includes comprehensive unit tests demonstrating usage patterns:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Xpandables.Net.UnitTests
```

---

## 📦 All Packages

| Package | Description |
|---------|-------------|
| **Xpandables.Results** | Result pattern types with request/handler pattern |
| **Xpandables.Results.Tasks** | Mediator for dispatching requests to handlers |
| **Xpandables.Results.Pipelines** | Pipeline decorators (validation, transactions, events) |
| **Xpandables.Optionals** | Null-safe optional value handling |
| **Xpandables.Primitives** | Core primitives and utilities |
| **Xpandables.Validation** | Specification pattern and rule validators |
| **Xpandables.Composition** | MEF-based service composition |
| **Xpandables.Events** | Domain events and event sourcing with W3C trace IDs |
| **Xpandables.Events.Data** | EF Core event store with outbox pattern |
| **Xpandables.Entities.Data** | EF Core repository with DataContext |
| **Xpandables.Rests** | Type-safe REST client |
| **Xpandables.AsyncPaged** | Async paged collections |
| **Xpandables.AsyncPaged.Linq** | LINQ extensions for async paging |
| **Xpandables.AsyncPaged.Json** | JSON serialization for paged data |
| **Xpandables.AspNetCore.Net** | ASP.NET Core minimal API integrations |
| **Xpandables.AspNetCore.AsyncPaged** | ASP.NET Core async paged response formatters |
| **Xpandables.AspNetCore.Events** | W3C trace context middleware for event correlation |
| **Xpandables.AspNetCore.Results** | ASP.NET Core result type integrations |
| **Xpandables.AspNetCore.Composition** | MEF-based ASP.NET Core service composition |

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/Francescolis/Xpandables.Net.git
   ```

2. Open in Visual Studio 2026 (or later) or Rider 2025.3+

3. Build the solution
   ```bash
   dotnet build
   ```

4. Run tests
   ```bash
   dotnet test
   ```

---

## 📄 License

This project is licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE) file for details.

```
Copyright © Kamersoft 2025
Licensed under the Apache License, Version 2.0
```

---

## 👤 Author

**Francescolis**
- GitHub: [@Francescolis](https://github.com/Francescolis)
- Company: Kamersoft

---

## 💬 Support

If you find this library useful, please consider:
- ⭐ Starring the repository
- 🐛 Reporting issues
- 💡 Suggesting new features
- 📝 Improving documentation

---

<div align="center">

**Built with ❤️ using .NET 10**

[⬆ back to top](#-xpandablesnet)

</div>