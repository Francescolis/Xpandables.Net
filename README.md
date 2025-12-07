<div align="center">

# 📦 Xpandables.Net

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://www.nuget.org/)
[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)

**A comprehensive, modular .NET library for building modern, scalable applications with clean architecture patterns**

[Features](#-key-features) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Real-World Example](#-real-world-example) • [Architecture](#-architecture) • [Contributing](#-contributing)

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
| ✅ **Operation Results** | Robust error handling with HTTP-aware result types | [System.Results](#systemresults) |
| 🎁 **Optional Values** | Null-safe optional value handling (like Rust's Option) | [System.Optionals](#systemoptionals) |
| 📡 **Mediator Pattern** | CQRS and request/response pipeline implementation | [System.Results.Tasks](#systemresultstasks) |
| ✔️ **Validation** | Flexible validation framework with specifications | [System.Primitives.Validation](#systemprimitivesvalidation) |
| 💾 **Repository Pattern** | Generic repository with unit of work support | [System.Entities.Data](#systementitiesdata) |
| 🌐 **REST Client** | Type-safe, attribute-based HTTP client | [System.Rests](#systemrests) |
| 📝 **Event Sourcing** | Complete event sourcing and CQRS implementation | [System.Events](#systemevents) |
| 🔄 **Async Paging** | Asynchronous enumerable extensions and pagination | [System.Collections.AsyncPaged](#systemcollectionsasyncpaged) |

---

## 🚀 Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **Visual Studio 2022, 2026** or **JetBrains Rider** (recommended)
- Basic understanding of C# and async/await patterns

### Installation

Install the packages you need via NuGet Package Manager:

```bash
# Core operation result handling
dotnet add package System.Results

# Optional value handling
dotnet add package System.Optionals

# Mediator and pipeline support
dotnet add package System.Results.Tasks

# Pipeline decorators
dotnet add package System.Results.Pipelines

# Validation framework
dotnet add package System.Primitives.Validation

# Repository pattern with EF Core
dotnet add package System.Entities.Data

# REST client
dotnet add package System.Rests

# Event sourcing
dotnet add package System.Events
dotnet add package System.Events.Data

# Async paging
dotnet add package System.Collections.AsyncPaged
dotnet add package System.Linq.AsyncPaged

# ASP.NET Core integration
dotnet add package AspNetCore.Net
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
            .Map(u => Result.Success(u))
            .Empty(() => Result
                .NotFound<User>("userId", "User not found"));
    }
}
```

---

## 💼 Real-World Example

This repository includes a complete **Event Sourcing Banking Sample API** that demonstrates how to build a production-ready application using Xpandables.Net. See the [SampleApi README](./Xpandables.Net.SampleApi/README.md) for a detailed walkthrough.

### Sample API Highlights

The `Xpandables.Net.SampleApi` project showcases:

✅ **Event Sourcing & CQRS** - Complete bank account management with domain events  
✅ **Minimal API Endpoints** - Clean, modular endpoint definitions with `IEndpointRoute`  
✅ **EF Core Integration** - Event store and outbox pattern with SQL Server  
✅ **Swagger Documentation** - Fully documented API with OpenAPI support  
✅ **Docker Support** - Production-ready containerization  

### Running the Sample

```bash
# Clone the repository
git clone https://github.com/Francescolis/Xpandables.Net.git
cd Xpandables.Net

# Navigate to sample project
cd Xpandables.Net.SampleApi

# Update connection string in appsettings.json
# Run migrations (automatic on startup)

# Run the application
dotnet run

# Navigate to Swagger UI
# https://localhost:5001/swagger
```

**Key Files to Explore:**

- `Program.cs` - Application setup with event sourcing
- `BankAccounts/Features/CreateBankAccount/` - Complete CQRS command example
- `EventStorage/` - Event store and outbox configuration

---

## 📚 Documentation

### Package Documentation

Each package has detailed documentation with examples and API references:

#### Foundation Packages
- 🔧 [**System.Primitives**](./System.Primitives/README.md) - Core primitives and utilities
- ✅ [**System.Results**](./System.Results/README.md) - Operation result handling with request/handler pattern
- 🎁 [**System.Optionals**](./System.Optionals/README.md) - Null-safe optional value types

#### Application Layer
- 📡 [**System.Results.Tasks**](./System.Results.Tasks/README.md) - Mediator pattern and request dispatching
- 🔗 [**System.Results.Pipelines**](./System.Results.Pipelines/README.md) - Pipeline decorators for validation, transactions, events
- ✔️ [**System.Primitives.Validation**](./System.Primitives.Validation/README.md) - Specification pattern and rule validators
- 🧩 [**System.Primitives.Composition**](./System.Primitives.Composition/README.md) - MEF-based service composition

#### Data Access
- 💾 [**System.Entities.Data**](./System.Entities.Data/README.md) - EF Core repository with DataContext

#### Event Handling
- 📝 [**System.Events**](./System.Events/README.md) - Event sourcing and domain events
- 🗄️ [**System.Events.Data**](./System.Events.Data/README.md) - EF Core event store implementation

#### HTTP & REST
- 🌐 [**System.Rests**](./System.Rests/README.md) - Type-safe REST client with attribute-based routing

#### Async & Utilities
- 🔄 [**System.Collections.AsyncPaged**](./System.Collections.AsyncPaged/README.md) - Async paged collections
- 🔄 [**System.Linq.AsyncPaged**](./System.Linq.AsyncPaged/README.md) - LINQ extensions for async paging
- 📄 [**System.Text.Json.AsyncPaged**](./System.Text.Json.AsyncPaged/README.md) - JSON serialization for paged data

#### ASP.NET Core Integration
- 🌐 [**AspNetCore.Net**](./AspNetCore.Net/README.md) - ASP.NET Core integrations with endpoint routing

---

## 🏗️ Architecture

Xpandables.Net follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│           (AspNetCore.Net)              │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Application Layer               │
│  (System.Results, System.Results.Tasks, │
│   System.Results.Pipelines)             │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│           Domain Layer                  │
│ (System.Events, System.Optionals,       │
│  System.Primitives, System.Primitives.  │
│  Validation)                            │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│       Infrastructure Layer              │
│  (System.Entities.Data, System.Events.  │
│   Data, System.Rests)                   │
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
| **System.Results** | Operation result types with request/handler pattern |
| **System.Results.Tasks** | Mediator for dispatching requests to handlers |
| **System.Results.Pipelines** | Pipeline decorators (validation, transactions, events) |
| **System.Optionals** | Null-safe optional value handling |
| **System.Primitives** | Core primitives and utilities |
| **System.Primitives.Validation** | Specification pattern and rule validators |
| **System.Primitives.Composition** | MEF-based service composition |
| **System.Events** | Domain events and event sourcing abstractions |
| **System.Events.Data** | EF Core event store implementation |
| **System.Entities.Data** | EF Core repository with DataContext |
| **System.Rests** | Type-safe REST client |
| **System.Collections.AsyncPaged** | Async paged collections |
| **System.Linq.AsyncPaged** | LINQ extensions for async paging |
| **System.Text.Json.AsyncPaged** | JSON serialization for paged data |
| **AspNetCore.Net** | ASP.NET Core integrations |

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/Francescolis/Xpandables.Net.git
   ```

2. Open in Visual Studio 2026 or Rider

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