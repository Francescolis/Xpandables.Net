# System.Optionals

[![NuGet](https://img.shields.io/nuget/v/Xpandables.Optionals.svg)](https://www.nuget.org/packages/Xpandables.Optionals)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Xpandables.Optionals.svg)](https://www.nuget.org/packages/Xpandables.Optionals)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

Optional monad for .NET — eliminate null-reference errors with a functional approach.

## 📖 Overview

`System.Optionals` (NuGet: **Xpandables.Optionals**) provides a lightweight `Optional<T>` type that represents a value that may or may not be present. It supports functional-style operations (`Map`, `Bind`, `Match`), comparison, JSON serialization, LINQ integration, and operator overloads. Namespace: `System.Optionals`.

Built for **.NET 10** and **C# 14**. No external dependencies.

## ✨ Features

| Type | File | Description |
|------|------|-------------|
| `Optional<T>` | `Optional.cs` | Read-only record struct — `Value`, `IsEmpty`, `IsNotEmpty`, implements `IEnumerable<T>` |
| `Optional` | `OptionalFactory.cs` | Static factory — `Empty<T>()`, `Some<T>(value)` |
| `OptionalSync` (partial) | `OptionalSync.cs` | Sync operations — `Map`, `Bind`, `Match`, `Filter`, `ToOptional<TU>` |
| `OptionalAsync` (partial) | `OptionalAsync.cs` | Async operations — `MapAsync`, `BindAsync`, `MatchAsync`, `FilterAsync` |
| `OptionalOperators` (partial) | `OptionalOperators.cs` | Comparison operators (`<`, `<=`, `>`, `>=`) and implicit conversions |
| `OptionalComparer` (partial) | `OptionalComparer.cs` | `IComparable<Optional<T>>` implementation |
| `OptionalExtensions` | `OptionalExtensions.cs` | Extension methods for converting values to `Optional<T>` |
| `EnumerableExtensions` | `EnumerableExtensions.cs` | LINQ helpers for `IEnumerable<Optional<T>>` |
| `OptionalJsonConverterFactory` | `OptionalJsonConverterFactory.cs` | `System.Text.Json` converter factory |
| `OptionalJsonSerialization` | `OptionalJsonSerialization.cs` | JSON serialization support |

## 📦 Installation

```bash
dotnet add package Xpandables.Optionals
```

## 🚀 Quick Start

### Creating Optionals

```csharp
using System.Optionals;

// From factory
Optional<int> some = Optional.Some(42);
Optional<int> empty = Optional.Empty<int>();

// Check state
if (some.IsNotEmpty) Console.WriteLine(some.Value); // 42
if (empty.IsEmpty) Console.WriteLine("No value");

// From nullable values via extension method
string? maybeName = GetNameOrNull();
Optional<string> optName = maybeName.ToOptional();
```

### Safe Value Access

```csharp
// GetValueOrDefault with fallback value
int value = Optional.Some(42).GetValueOrDefault(0);            // 42
int fallback = Optional.Empty<int>().GetValueOrDefault(0);     // 0

// GetValueOrDefault with factory
int computed = Optional.Empty<int>().GetValueOrDefault(() => ComputeDefault()); // ComputeDefault()

// TryGetValue pattern
if (optionalUser.TryGetValue(out User user))
{
    Console.WriteLine(user.Name);
}
```

### Map — Transform If Present

```csharp
// Map with transform function
Optional<string> upper = Optional.Some("hello")
    .Map(s => s.ToUpper()); // Optional<string> "HELLO"

// Map does nothing if empty
Optional<string> noop = Optional.Empty<string>()
    .Map(s => s.ToUpper()); // Empty — no exception

// Map with side effect (action)
Optional<Order> order = Optional.Some(myOrder)
    .Map(o => Console.WriteLine($"Processing order {o.Id}"));
```

### Bind — Transform and Flatten

```csharp
// Bind to a different type
Optional<UserProfile> profile = Optional.Some(userId)
    .Bind(id => FindUserById(id))         // Optional<User>
    .Bind(user => user.Profile);          // Optional<UserProfile>

// Bind to Optional (flat map)
Optional<string> email = Optional.Some(userId)
    .Bind<User>(id => userRepository.FindById(id) is User u
        ? Optional.Some(u)
        : Optional.Empty<User>())
    .Bind(user => Optional.Some(user.Email));
```

### Empty — Provide Fallback When Missing

```csharp
// Provide default when empty
Optional<Config> config = Optional.Empty<Config>()
    .Empty(() => Config.Default);  // returns Optional.Some(Config.Default)

// Chain: try primary, then fallback
Optional<User> user = FindUserInCache(userId)
    .Empty(() => FindUserInDb(userId));
```

### ToOptional — Type Conversion

```csharp
// Convert between types
Optional<object> boxed = Optional.Some<object>("hello");
Optional<string> unboxed = boxed.ToOptional<string>(); // Optional<string> "hello"

// If types don't match, returns Empty
Optional<int> wrong = boxed.ToOptional<int>(); // Empty
```

### Async Operations

```csharp
// MapAsync
Optional<UserDto> dto = await Optional.Some(userId)
    .MapAsync(async id =>
    {
        User user = await userService.GetByIdAsync(id);
        return userId;   // stays as the same type
    });

// BindAsync — async flat map to different type
Optional<UserDto> userDto = await Optional.Some(userId)
    .BindAsync(async id =>
    {
        User? user = await db.Users.FindAsync(id);
        return user is not null
            ? Optional.Some(new UserDto(user.Id, user.Name))
            : Optional.Empty<UserDto>();
    });

// EmptyAsync — async fallback
Optional<Config> config = await Optional.Empty<Config>()
    .EmptyAsync(async () => await LoadConfigFromRemoteAsync());
```

### Chained Async Pipeline

```csharp
Optional<OrderConfirmation> confirmation = await Optional.Some(orderId)
    .BindAsync(async id => await orderService.FindAsync(id) is Order o
        ? Optional.Some(o)
        : Optional.Empty<Order>())
    .MapAsync(async order =>
    {
        await paymentService.ChargeAsync(order.CustomerId, order.Total);
        return order;
    })
    .BindAsync(async order =>
    {
        var result = await fulfillmentService.ConfirmAsync(order.Id);
        return result is not null
            ? Optional.Some(result)
            : Optional.Empty<OrderConfirmation>();
    });

if (confirmation.IsNotEmpty)
    Console.WriteLine($"Order confirmed: {confirmation.Value.ConfirmationNumber}");
else
    Console.WriteLine("Order could not be fulfilled");
```

### LINQ Integration

```csharp
// Optional<T> implements IEnumerable<T>
foreach (string item in Optional.Some("hello"))
{
    Console.WriteLine(item); // "hello"
}

// Empty optional yields nothing
foreach (string item in Optional.Empty<string>()) { /* never reached */ }

// Where — filter optional value
Optional<int> positive = Optional.Some(42).Where(x => x > 0);      // Some(42)
Optional<int> none = Optional.Some(-1).Where(x => x > 0);           // Empty

// Select / SelectMany (LINQ query syntax)
Optional<string> result =
    from user in Optional.Some(new User("Alice", "alice@example.com"))
    from profile in Optional.Some(new Profile("Developer"))
    select $"{user.Name} — {profile.Title}";
// result = Optional<string> "Alice — Developer"

// Async Select
Optional<UserDto> userDto = await Optional.Some(userId)
    .SelectAsync(async id => await FetchUserDtoAsync(id));
```

### Collection Helpers

```csharp
// FirstOrEmpty — safe first element
Optional<Product> first = products.FirstOrEmpty();
Optional<Product> match = products.FirstOrEmpty(p => p.Price > 100);

// WhereSome — extract values from a collection of optionals
IEnumerable<Optional<User>> optionalUsers = ids.Select(id => FindUser(id));
IEnumerable<User> validUsers = optionalUsers.WhereSome(); // only non-empty values
```

### Comparison and Equality

```csharp
// Optional<T> supports comparison when T : IComparable<T>
Optional<int> a = Optional.Some(10);
Optional<int> b = Optional.Some(20);

bool less = a < b;     // true
bool greater = a > b;  // false

// Equality
bool equal = Optional.Some(42) == Optional.Some(42); // true
bool notEqual = Optional.Some(42) == Optional.Empty<int>(); // false
```

### JSON Serialization

```csharp
using System.Text.Json;

// Optional<T> serializes as the value itself (or null when empty)
string json = JsonSerializer.Serialize(Optional.Some(42));    // "42"
string emptyJson = JsonSerializer.Serialize(Optional.Empty<int>()); // "null"

// Deserialization
Optional<int> deserialized = JsonSerializer.Deserialize<Optional<int>>("42");
// deserialized.Value == 42
```

### Real-World Example: Repository Pattern

```csharp
public interface IUserRepository
{
    Optional<User> FindById(Guid id);
    Task<Optional<User>> FindByEmailAsync(string email, CancellationToken ct);
}

public class UserAppService(IUserRepository repository)
{
    public async Task<string> GetGreetingAsync(string email, CancellationToken ct)
    {
        return (await repository.FindByEmailAsync(email, ct))
            .Bind(user => Optional.Some($"Hello, {user.Name}!"))
            .GetValueOrDefault("User not found");
    }
}
```

---

## 📁 Project Structure

```
System.Optionals/
├── Optional.cs                    # Core Optional<T> record struct
├── OptionalFactory.cs             # Static factory (Empty, Some)
├── OptionalSync.cs                # Map, Bind, Match, Filter
├── OptionalAsync.cs               # MapAsync, BindAsync, MatchAsync
├── OptionalOperators.cs           # Comparison & conversion operators
├── OptionalComparer.cs            # IComparable implementation
├── OptionalExtensions.cs          # Extension methods
├── EnumerableExtensions.cs        # LINQ helpers
├── OptionalJsonConverterFactory.cs
└── OptionalJsonSerialization.cs
```

---

## 📄 License

Apache License 2.0 — Copyright © Kamersoft 2025
