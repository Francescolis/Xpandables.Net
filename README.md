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
| ✅ **Execution Results** | Robust error handling with HTTP-aware result types | [ExecutionResults](#executionresults) |
| 🎁 **Optional Values** | Null-safe optional value handling (like Rust's Option) | [Optionals](#optionals) |
| 📡 **Mediator Pattern** | CQRS and request/response pipeline implementation | [Tasks](#tasks) |
| ✔️ **Validation** | Flexible validation framework with specifications | [Validators](#validators) |
| 💾 **Repository Pattern** | Generic repository with unit of work support | [Repositories](#repositories) |
| 🌐 **REST Client** | Type-safe, attribute-based HTTP client | [Rests](#rests) |
| 📝 **Event Sourcing** | Complete event sourcing and CQRS implementation | [Events](#events) |
| 🔄 **Async Utilities** | Asynchronous enumerable extensions and pagination | [Async](#async) |

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
dotnet add package Xpandables.Net.ExecutionResults

# Optional value handling
dotnet add package Xpandables.Net.Optionals

# Mediator and pipeline support
dotnet add package Xpandables.Net.Tasks

# Validation framework
dotnet add package Xpandables.Net.Validators

# Repository pattern
dotnet add package Xpandables.Net.Repositories
dotnet add package Xpandables.Net.Repositories.EntityFramework

# REST client
dotnet add package Xpandables.Net.Rests

# Event sourcing
dotnet add package Xpandables.Net.Events
```

### Quick Example

```csharp
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Optionals;
using Xpandables.Net.Tasks;

// Define a request
public sealed record GetUserQuery(Guid UserId) : IRequest<User>;

// Handle the request
public sealed class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _repository;
    
    public GetUserHandler(IUserRepository repository) 
        => _repository = repository;
    
    public async Task<OperationResult<User>> HandleAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        // Fetch user with Optional to handle null cases
        Optional<User> user = await _repository
            .FindByIdAsync(request.UserId, cancellationToken);
        
        return user
            .Map(u => OperationResult.Success(u))
            .Empty(() => OperationResult
                .NotFound()
                .WithError("userId", "User not found")
                .Build<User>());
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
- 🔧 [**Abstractions**](./Xpandables.Net.Abstractions/README.md) - Core abstractions and utilities
- ✅ [**ExecutionResults**](./Xpandables.Net.ExecutionResults/README.md) - Standardized operation result handling
- 🎁 [**Optionals**](./Xpandables.Net.Optionals/README.md) - Null-safe optional value types

#### Application Layer
- 📡 [**Tasks**](./Xpandables.Net.Tasks/README.md) - Mediator pattern and request handlers
- ✔️ [**Validators**](./Xpandables.Net.Validators/README.md) - Validation framework
- 🔗 [**Validators.Pipelines**](./Xpandables.Net.Validators.Pipelines/README.md) - Pipeline validation decorators

#### Data Access
- 💾 [**Repositories**](./Xpandables.Net.Repositories/README.md) - Generic repository pattern
- 🗄️ [**Repositories.EntityFramework**](./Xpandables.Net.Repositories.EntityFramework/README.md) - EF Core implementation
- 🔗 [**Repositories.Pipelines**](./Xpandables.Net.Repositories.Pipelines/README.md) - Repository pipeline decorators

#### Event Handling
- 📝 [**Events**](./Xpandables.Net.Events/README.md) - Event sourcing and domain events
- 📦 [**Events.Repositories**](./Xpandables.Net.Events.Repositories/README.md) - Event store abstractions
- 🗄️ [**Events.EntityFramework**](./Xpandables.Net.Events.EntityFramework/README.md) - EF Core event store
- 🔗 [**Events.Pipelines**](./Xpandables.Net.Events.Pipelines/README.md) - Event pipeline decorators

#### HTTP & REST
- 🌐 [**Rests.Abstractions**](./Xpandables.Net.Rests.Abstractions/README.md) - REST client abstractions
- 🌐 [**Rests**](./Xpandables.Net.Rests/README.md) - REST client implementation

#### Async & Utilities
- 🔄 [**Async**](./Xpandables.Net.Async/README.md) - Async enumerable utilities and pagination

#### ASP.NET Core Integration
- 🌐 [**AspNetCore**](./Xpandables.Net.AspNetCore/README.md) - ASP.NET Core integrations
- 🔄 [**Async.AspNetCore**](./Xpandables.Net.Async.AspNetCore/README.md) - Async utilities for ASP.NET Core
- ✅ [**ExecutionResults.AspNetCore**](./Xpandables.Net.ExecutionResults.AspNetCore/README.md) - OperationResult to IResult mapping
- ✔️ [**Validators.AspNetCore**](./Xpandables.Net.Validators.AspNetCore/README.md) - ASP.NET Core validation filters

---

## 🏗️ Architecture

Xpandables.Net follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│    (AspNetCore, Async.AspNetCore)       │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│         Application Layer               │
│  (Tasks, Validators, ExecutionResults)  │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│           Domain Layer                  │
│   (Events, Optionals, Abstractions)     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│       Infrastructure Layer              │
│  (Repositories.EF, Events.EF)           │
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

### 1. Execution Results - Railway Oriented Programming

```csharp
public async Task<OperationResult<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateRequest(request)
        .BindAsync(CreateOrder)
        .BindAsync(ProcessPayment)
        .BindAsync(SendConfirmationEmail)
        .Map(order => OperationResult.Created(order))
        .Empty(() => OperationResult
            .BadRequest()
            .WithError("request", "Failed to create order")
            .Build<Order>());
}
```

### 2. Type-Safe REST Client

```csharp
[RestPost("/api/users")]
public sealed record CreateUserRequest(string Name, string Email) 
    : IRestRequest<User>, IRestString;

var response = await _restClient.SendAsync(
    new CreateUserRequest("John", "john@example.com"));
    
var user = response.Result;
```

### 3. Fluent Validation with Specifications

```csharp
public sealed class UserValidator : Validator<CreateUserRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(
        CreateUserRequest instance)
    {
        // Using static factory methods to create specifications
        var nameSpec = Specification
            .IsNotNull<CreateUserRequest, string>(u => u.Name);
        var emailSpec = Specification
            .Contains<CreateUserRequest>(u => u.Email, "@");
        var ageSpec = Specification
            .GreaterThan<CreateUserRequest, int>(u => u.Age, 18);
        
        // Combine specifications
        var combinedSpec = Specification.All(nameSpec, emailSpec, ageSpec);
        
        if (!combinedSpec.IsSatisfiedBy(instance))
        {
            return
            [
                new ValidationResult("Name is required", 
                    [nameof(instance.Name)]),
                new ValidationResult("Invalid email format", 
                    [nameof(instance.Email)]),
                new ValidationResult("Must be 18 or older", 
                    [nameof(instance.Age)])
            ];
        }
        
        return [];
    }
}
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