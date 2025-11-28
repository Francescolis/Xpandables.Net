# 🔧 System.Primitives

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Core Primitives & Utilities** - Strongly-typed primitive wrappers (Value Objects), extension methods, state patterns, caching, and foundational utilities.

---

## 🎯 Overview

`System.Primitives` provides the foundational abstractions and utilities used throughout the Xpandables.Net ecosystem. It includes the Value Object pattern via `IPrimitive<T>`, state management patterns, memory-aware caching, element collections, and essential extension methods.

This library has **zero dependencies** and serves as the base for all other Xpandables.Net packages.

### ✨ Key Features

- 🎯 **Strongly-Typed Primitives** - Value Object pattern with `IPrimitive<TPrimitive, TValue>`
- 🔄 **State Pattern** - `IState`, `IStateContext`, `IMemento` implementations
- 💾 **Memory-Aware Cache** - Intelligent caching with GC pressure monitoring
- 📦 **Element Collections** - Flexible key-value collections with JSON support
- 🧩 **Extension Methods** - String, Object, HttpStatusCode, Exception utilities
- 🔧 **Disposable Helpers** - Base classes for IDisposable/IAsyncDisposable
- 🌐 **HTTP Utilities** - Status code helpers and extensions
- ⚙️ **Service Decorators** - DI composition and decorator patterns

---

## 📥 Installation

```bash
dotnet add package System.Primitives
```

---

## 🚀 Quick Start

### Strongly-Typed Primitives (Value Objects)

Define domain primitives with built-in validation and type safety:

```csharp
using System;

// Email primitive with validation
[PrimitiveJsonConverter<Email, string>]
public readonly record struct Email : IPrimitive<Email, string>
{
    public required string Value { get; init; }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email format");

        return new Email { Value = value.ToLower() };
    }

    public static string DefaultValue => string.Empty;

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string value) => Create(value);
    
    public static bool operator ==(Email left, Email right) => 
        left.Value == right.Value;
    public static bool operator !=(Email left, Email right) => 
        !(left == right);
}

// Usage
Email email = "john@example.com";  // Implicit conversion + validation
string emailStr = email;            // Implicit back to string
Console.WriteLine($"Email: {email}");  // john@example.com

// Validation happens at construction
try
{
    Email invalid = "notanemail";  // Throws ArgumentException
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// JSON serialization automatically works
var json = JsonSerializer.Serialize(email);  // "john@example.com"
var deserialized = JsonSerializer.Deserialize<Email>(json);
```

### Money Primitive

```csharp
[PrimitiveJsonConverter<Money, decimal>]
public readonly record struct Money : IPrimitive<Money, decimal>
{
    public required decimal Value { get; init; }

    public static Money Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Money cannot be negative");

        return new Money { Value = Math.Round(value, 2) };
    }

    public static decimal DefaultValue => 0m;

    public static implicit operator decimal(Money money) => money.Value;
    public static implicit operator Money(decimal value) => Create(value);

    public static bool operator ==(Money left, Money right) => 
        left.Value == right.Value;
    public static bool operator !=(Money left, Money right) => 
        !(left == right);

    // Domain operations
    public Money Add(Money other) => Value + other.Value;
    public Money Subtract(Money other) => Value - other.Value;
    public Money Multiply(decimal factor) => Value * factor;
}

// Usage
Money price = 19.99m;
Money tax = 2.00m;
Money total = price.Add(tax);  // 21.99

Console.WriteLine($"Total: ${total}");  // Total: $21.99
```

---

## 💡 Why Use Primitives?

### Before (Raw Types)
```csharp
public class User
{
    public string Email { get; set; }  // ❌ No validation
    public string PhoneNumber { get; set; }  // ❌ Can mix up with email
    public decimal Balance { get; set; }  // ❌ Can be negative
}

// Easy to make mistakes
user.Email = user.PhoneNumber;  // ❌ Compiles but wrong!
user.Balance = -100m;  // ❌ Invalid but allowed
```

### After (Strongly-Typed Primitives)
```csharp
public class User
{
    public Email Email { get; init; }  // ✅ Type-safe
    public PhoneNumber PhoneNumber { get; init; }  // ✅ Cannot mix up
    public Money Balance { get; init; }  // ✅ Validated at construction
}

// Type safety prevents errors
user.Email = user.PhoneNumber;  // ✅ Compile error!
user.Balance = Money.Create(-100);  // ✅ Throws at runtime
```

---

## 📦 Element Collections

Flexible key-value collections for errors, headers, metadata:

```csharp
// Create element collection
var errors = ElementCollection.With([
    new ElementEntry("Email", "Email is required"),
    new ElementEntry("Password", ["Password too short", "Password needs uppercase"])
]);

// Iterate
foreach (var entry in errors)
{
    Console.WriteLine($"{entry.Key}:");
    foreach (var value in entry.Values)
    {
        Console.WriteLine($"  - {value}");
    }
}

// Convert to dictionary
Dictionary<string, object> dict = errors.ToDictionaryObject();

// JSON serialization
string json = JsonSerializer.Serialize(errors);
// Output: [{"key":"Email","values":["Email is required"]},...]
```

---

## 🔄 State Pattern

Implement state machines with the State pattern:

```csharp
// Define states
public abstract class OrderState : State<Order>
{
    protected OrderState(string name) : base(name) { }

    public static OrderState Pending => new PendingState();
    public static OrderState Confirmed => new ConfirmedState();
    public static OrderState Shipped => new ShippedState();
    public static OrderState Delivered => new DeliveredState();
}

public class PendingState : OrderState
{
    public PendingState() : base("Pending") { }

    public override void Handle(IStateContext<Order> context)
    {
        // Business logic here
        context.Order.ConfirmOrder();
        context.SetState(OrderState.Confirmed);
    }
}

// Use state context
var order = new Order();
var context = new StateContext<Order>(order, OrderState.Pending);

context.Request();  // Transitions to Confirmed
Console.WriteLine($"State: {context.State.Name}");  // "Confirmed"
```

---

## 💾 Memory-Aware Cache

Cache with automatic eviction under memory pressure:

```csharp
var cache = new MemoryAwareCache<string, User>(maxItems: 1000);

// Add items
cache.Add("user:123", new User { Id = 123, Name = "John" });
cache.Add("user:456", new User { Id = 456, Name = "Jane" });

// Retrieve
if (cache.TryGet("user:123", out var user))
{
    Console.WriteLine($"Found: {user.Name}");
}

// Cache uses weak references
// Items automatically evicted when GC needs memory
```

---

## 🧩 Extension Methods

### String Extensions
```csharp
string text = "  hello  ";
bool isEmpty = text.IsNullOrWhiteSpace();  // false
string trimmed = text.Trim();  // "hello"
```

### Object Extensions
```csharp
var obj = new { Name = "John", Age = 30 };
var dict = obj.ToDictionary();  // Convert to Dictionary<string, object>
```

### Exception Extensions
```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    var executionResult = ex.ToExecutionResult();
    // Converts exception to OperationResult
}
```

### HttpStatusCode Extensions
```csharp
var status = HttpStatusCode.BadRequest;

string title = status.Title;  // "Bad Request"
string detail = status.Detail;  // Description
bool isSuccess = status.IsSuccess();  // false
bool isFailure = status.IsFailure();  // true
bool isValidation = status.IsValidationProblem();  // true
```

---

## 🎯 Real-World Examples

### UserId Primitive

```csharp
[PrimitiveJsonConverter<UserId, Guid>]
public readonly record struct UserId : IPrimitive<UserId, Guid>
{
    public required Guid Value { get; init; }

    public static UserId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");
        return new UserId { Value = value };
    }

    public static Guid DefaultValue => Guid.Empty;
    public static UserId NewId() => Create(Guid.NewGuid());

    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid value) => Create(value);

    public static bool operator ==(UserId left, UserId right) => 
        left.Value == right.Value;
    public static bool operator !=(UserId left, UserId right) => 
        !(left == right);
}

// Usage in domain model
public class User
{
    public UserId Id { get; init; } = UserId.NewId();
    public Email Email { get; init; }
    public string Name { get; init; } = default!;
}
```

### Domain Model with Primitives

```csharp
public class Order
{
    public OrderId Id { get; init; } = OrderId.NewId();
    public UserId CustomerId { get; init; }
    public Money TotalAmount { get; private set; }
    public List<OrderLine> Lines { get; init; } = [];

    public void AddLine(ProductId productId, Quantity quantity, Money unitPrice)
    {
        var line = new OrderLine
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Total = unitPrice.Multiply(quantity.Value)
        };

        Lines.Add(line);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        decimal sum = Lines.Sum(l => l.Total.Value);
        TotalAmount = Money.Create(sum);
    }
}

// Usage
var order = new Order { CustomerId = currentUserId };
order.AddLine(
    productId: ProductId.From("PROD-001"),
    quantity: Quantity.Create(2),
    unitPrice: Money.Create(49.99m)
);

Console.WriteLine($"Order total: {order.TotalAmount}");  // $99.98
```

---

## 🔧 Disposable Helpers

### Synchronous Disposable

```csharp
public class MyResource : Disposable
{
    private readonly Stream _stream;

    public MyResource(Stream stream)
    {
        _stream = stream;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### Async Disposable

```csharp
public class MyAsyncResource : DisposableAsync
{
    private readonly DbConnection _connection;

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && _connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync(disposing);
    }
}
```

---

## ✅ Best Practices

### ✅ Do

- **Use primitives for domain concepts** - Email, Money, ProductId, etc.
- **Validate in Create() method** - Fail fast with clear exceptions
- **Make primitives immutable** - Use `readonly record struct`
- **Add domain operations** - Methods like Add(), Subtract() for Money
- **Use implicit conversions** - Seamless usage with underlying types
- **Decorate with JsonConverter** - Automatic JSON serialization
- **Keep primitives simple** - Single responsibility

### ❌ Don't

- **Create primitives for everything** - Only for important domain concepts
- **Put complex business logic in primitives** - Keep them lightweight
- **Make primitives classes** - Use structs for better performance
- **Skip validation** - Always validate in Create() method
- **Expose public setters** - Primitives should be immutable
- **Throw generic exceptions** - Use specific ArgumentException messages

---

## 📊 Performance Benefits

| Aspect | Classes | Structs (Primitives) |
|--------|---------|----------------------|
| Allocation | Heap | Stack (most cases) |
| GC Pressure | Higher | Lower |
| Copy Cost | Reference | Value copy |
| Memory | Pointer overhead | Direct value |
| Best For | Large objects | Small values |

Primitives use `readonly record struct` for:
- ✅ Stack allocation in most cases
- ✅ Reduced GC pressure
- ✅ Value semantics
- ✅ Immutability guarantees

---

## 📚 Related Packages

- **[System.Primitives.Validation](../System.Primitives.Validation)** - FluentValidation integration
- **[System.Primitives.Composition](../System.Primitives.Composition)** - DI composition utilities
- **[System.ExecutionResults](../System.ExecutionResults)** - Result pattern types
- **[System.Optionals](../System.Optionals)** - Optional value types

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025
