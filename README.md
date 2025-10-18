<div align="center">

# 📦 Xpandables.Net

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)

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
# Core execution result handling
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
public sealed record GetUserQuery(Guid UserId) : IRequest;

// Handle the request
public sealed class GetUserHandler : IRequestHandler<GetUserQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        // Fetch user with Optional to handle null cases
        Optional<User> user = await _repository.FindByIdAsync(request.UserId);
        
        return user
            .Map(u => ExecutionResult.Success(u))
            .Empty(() => ExecutionResult.NotFound().WithError("userId","key not exists").Build());
    }
}
```

---

## 📚 Documentation

### Package Documentation

Each package has detailed documentation with examples and API references:

#### Foundation Packages
- 🔧 [**Abstractions**](./Xpandables.Net.Abstractions/README.md) - Core abstractions and utilities
- ✅ [**ExecutionResults**](./Xpandables.Net.ExecutionResults/README.md) - Standardized execution result handling
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
- ✅ [**ExecutionResults.AspNetCore**](./Xpandables.Net.ExecutionResults.AspNetCore/README.md) - ExecutionResult to IResult mapping
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
public async Task<ExecutionResult<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    return await ValidateRequest(request)
        .BindAsync(CreateOrder)
        .BindAsync(ProcessPayment)
        .BindAsync(SendConfirmationEmail)
        .Map(order => ExecutionResult.Created(order))
        .Empty(() => ExecutionResult.BadRequest(request.Id,"Failed to create order").Build());
}
```

### 2. Type-Safe REST Client

```csharp
[RestPost("/api/users")]
public sealed record CreateUserRequest(string Name, string Email) 
    : IRestRequest<User>, IRestString;

restResponse<User> response = await _restClient.SendAsync(new CreateUserRequest("John", "john@example.com"));
var user = response.Result;
```

### 3. Fluent Validation with Specifications

```csharp
public sealed class UserValidator : Validator<CreateUserRequest>
{
    public override IReadOnlyCollection<ValidationResult> Validate(CreateUserRequest instance)
    {
        var spec = new Specification<CreateUserRequest>()
            .And(u => !string.IsNullOrEmpty(u.Name), "Name is required")
            .And(u => u.Email.Contains("@"), "Invalid email format");
            
        return spec.IsSatisfiedBy(instance) 
            ? [] 
            : spec.GetErrors();
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

2. Open in Visual Studio 2025 or Rider

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