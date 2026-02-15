# 🎯 System.Optionals

[![NuGet](https://img.shields.io/badge/NuGet-10.0.1-blue.svg)](https://www.nuget.org/packages/Xpandables.Optionals)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](LICENSE)

> **Functional Optional Type for .NET** — A type-safe alternative to null references with monadic operations, LINQ support, async extensions, and AOT-compatible JSON serialization.

---

## 📋 Overview

`Xpandables.Optionals` provides a robust `Optional<T>` type that explicitly represents the presence or absence of a value without using null references. The library includes functional operations (`Map`, `Bind`, `Empty`), LINQ query syntax support, async extensions, operator overloads, and source-generated JSON serialization for AOT compatibility.

Built for .NET 10 with C# 14 extension members, this package enables writing safer, more expressive code by making the absence of values explicit in your type system.

### ✨ Key Features

- 🎯 **`Optional<T>`** — Value type representing presence (`Some`) or absence (`Empty`) of a value
- 🔄 **Functional Operations** — `Map`, `Bind`, `Empty` for transformations and chaining
- 📝 **LINQ Support** — `Select`, `SelectMany`, `Where` for query syntax
- ⚡ **Async Extensions** — `MapAsync`, `BindAsync`, `EmptyAsync` for async workflows
- 🔢 **Operator Overloads** — Implicit conversions, comparison operators (`<`, `>`, `<=`, `>=`)
- 📦 **Enumerable Extensions** — `FirstOrEmpty`, `WhereSome` for collections
- 🚀 **AOT Compatible** — Source-generated JSON serialization via `OptionalJsonContext`
- 🔧 **IEnumerable Support** — Optional implements `IEnumerable<T>` for foreach iteration

---

## 📦 Installation

```bash
dotnet add package Xpandables.Optionals
```

Or via NuGet Package Manager:

```powershell
Install-Package Xpandables.Optionals
```

---

## 🚀 Quick Start

### Creating Optionals

```csharp
using System.Optionals;

// Create an optional with a value
Optional<string> name = Optional.Some("John");

// Create an empty optional
Optional<string> empty = Optional.Empty<string>();

// Convert from nullable using extension method
string? nullableName = GetNameOrNull();
Optional<string> optionalName = nullableName.ToOptional();

// Implicit conversion from value
Optional<int> age = 25; // Implicitly converts to Optional.Some(25)

// Implicit conversion from null
Optional<string?> nullValue = null; // Becomes Optional.Empty<string>()
```

### Checking and Accessing Values

```csharp
Optional<User> user = GetUserById(userId);

// Check if value is present
if (user.IsNotEmpty)
{
    Console.WriteLine($"Found user: {user.Value.Name}");
}

// Check if empty
if (user.IsEmpty)
{
    Console.WriteLine("User not found");
}

// Get value with default
string userName = user
    .Bind(u => u.Name)
    .GetValueOrDefault("Unknown");

// Get value with factory
string userName = user
    .Bind(u => u.Name)
    .GetValueOrDefault(() => GenerateDefaultName());

// Try pattern
if (user.TryGetValue(out var foundUser))
{
    Console.WriteLine(foundUser.Name);
}
```

---

## 🔄 Functional Operations

### Map — Transform the Value

```csharp
Optional<User> user = GetUserById(userId);

// Map transforms the value if present
Optional<User> updatedUser = user.Map(u =>
{
    u.LastAccessedAt = DateTime.UtcNow;
    return u;
});

// Map with action (side effect)
user.Map(u => Console.WriteLine($"Processing user: {u.Name}"));

// Map returning Optional
Optional<User> validated = user.Map(u =>
    u.IsValid ? Optional.Some(u) : Optional.Empty<User>());
```

### Bind — Transform to Different Type

```csharp
Optional<User> user = GetUserById(userId);

// Bind transforms to a different type
Optional<string> email = user.Bind(u => u.Email);

// Bind with Optional return (flatMap)
Optional<Address> address = user.Bind(u =>
    u.AddressId.HasValue
        ? GetAddressById(u.AddressId.Value)
        : Optional.Empty<Address>());

// Chain multiple binds
Optional<string> city = user
    .Bind(u => u.Address.ToOptional())
    .Bind(a => a.City);
```

### Empty — Handle Missing Values

```csharp
Optional<User> user = GetUserById(userId);

// Provide default value when empty
Optional<User> userOrDefault = user.Empty(() => new User { Name = "Guest" });

// Provide default Optional when empty
Optional<User> fallbackUser = user.Empty(() => GetDefaultUser());

// Execute action when empty
user.Empty(() => LogWarning("User not found"));
```

---

## 📝 LINQ Query Syntax

```csharp
// Use LINQ query syntax with Optional
var result =
    from user in GetUserById(userId)
    from address in user.Address.ToOptional()
    from city in address.City.ToOptional()
    select new { user.Name, City = city };

// Equivalent fluent syntax
var result = GetUserById(userId)
    .SelectMany(user => user.Address.ToOptional(),
        (user, address) => new { user, address })
    .SelectMany(x => x.address.City.ToOptional(),
        (x, city) => new { x.user.Name, City = city });

// Where filtering
Optional<User> activeUser = GetUserById(userId)
    .Where(u => u.IsActive);

// Select projection
Optional<string> userName = GetUserById(userId)
    .Select(u => u.Name);
```

---

## ⚡ Async Operations

### MapAsync — Async Transformations

```csharp
Optional<User> user = GetUserById(userId);

// Async map with value transformation
Optional<User> enriched = await user.MapAsync(async u =>
{
    u.Profile = await LoadProfileAsync(u.Id);
    return u;
});

// Async map with Optional return
Optional<User> validated = await user.MapAsync(async u =>
{
    bool isValid = await ValidateUserAsync(u);
    return isValid ? Optional.Some(u) : Optional.Empty<User>();
});

// Async map with action
await user.MapAsync(async u =>
{
    await SendNotificationAsync(u.Email);
});
```

### BindAsync — Async Type Transformation

```csharp
Optional<User> user = GetUserById(userId);

// Bind to async operation
Optional<Profile> profile = await user.BindAsync(async u =>
    await LoadProfileAsync(u.Id));

// Bind with Optional return
Optional<Order> lastOrder = await user.BindAsync(async u =>
    await GetLastOrderAsync(u.Id)); // Returns Optional<Order>
```

### EmptyAsync — Async Fallback

```csharp
Optional<User> user = GetUserById(userId);

// Async fallback when empty
Optional<User> userOrDefault = await user.EmptyAsync(async () =>
    await CreateGuestUserAsync());

// Async action when empty
await user.EmptyAsync(async () =>
    await LogMissingUserAsync(userId));
```

### Chaining Async Operations

```csharp
// Chain async operations on Task<Optional<T>>
Task<Optional<User>> userTask = GetUserByIdAsync(userId);

var result = await userTask
    .MapAsync(async u => await EnrichUserAsync(u))
    .BindAsync(async u => await LoadOrdersAsync(u.Id))
    .EmptyAsync(async () => await GetDefaultOrdersAsync());

// LINQ-style async chaining
var orderSummary = await userTask
    .SelectAsync(async u => await LoadOrdersAsync(u.Id))
    .SelectManyAsync(
        async orders => await GetLatestOrderAsync(orders),
        async (orders, latest) => await CreateSummaryAsync(orders, latest));
```

---

## 📦 Collection Extensions

### FirstOrEmpty

```csharp
List<User> users = GetAllUsers();

// Get first or empty (sync)
Optional<User> firstUser = users.FirstOrEmpty();

// Get first matching or empty
Optional<User> activeUser = users.FirstOrEmpty(u => u.IsActive);

// Async version
IAsyncEnumerable<User> usersAsync = GetAllUsersAsync();
Optional<User> firstAsync = await usersAsync.FirstOrEmptyAsync();
Optional<User> matchingAsync = await usersAsync.FirstOrEmptyAsync(u => u.IsActive);
```

### WhereSome — Filter Non-Empty Optionals

```csharp
IEnumerable<Optional<User>> optionalUsers = userIds
    .Select(id => GetUserById(id));

// Extract only present values
IEnumerable<User> presentUsers = optionalUsers.WhereSome();

// Async version
IAsyncEnumerable<Optional<User>> asyncOptionals = GetUsersAsync();
IAsyncEnumerable<User> presentAsync = asyncOptionals.WhereSomeAsync();
```

---

## 🔢 Operators and Conversions

### Implicit Conversions

```csharp
// Value to Optional
Optional<int> age = 25; // Implicit conversion to Some(25)

// Optional to value (throws if empty)
int value = age; // Implicit conversion, throws InvalidOperationException if empty

// Nested Optional flattening
Optional<Optional<string>> nested = Optional.Some(Optional.Some("value"));
Optional<string> flattened = nested; // Implicitly flattens
```

### Comparison Operators

```csharp
Optional<int> a = Optional.Some(5);
Optional<int> b = Optional.Some(10);
Optional<int> empty = Optional.Empty<int>();

// Compare optionals
bool less = a < b;      // true
bool greater = b > a;   // true
bool lessEq = a <= 5;   // true (compares with value)
bool greaterEq = b >= 10; // true

// Empty comparisons
bool emptyLess = empty < a; // true (empty is always less than any value)
```

### Enumeration Support

```csharp
Optional<User> user = GetUserById(userId);

// Optional implements IEnumerable<T>
foreach (var u in user)
{
    Console.WriteLine(u.Name); // Executes only if present
}

// Use with LINQ methods
bool hasAdminRole = user.Any(u => u.Role == "Admin");
```

---

## 🚀 JSON Serialization (AOT Compatible)

### Basic Serialization

```csharp
using System.Text.Json;
using System.Optionals;

var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() }
};

// Serialize - Some becomes value, Empty becomes null
Optional<string> name = Optional.Some("John");
string json = JsonSerializer.Serialize(name, options); // "John"

Optional<string> empty = Optional.Empty<string>();
string nullJson = JsonSerializer.Serialize(empty, options); // null

// Deserialize
Optional<string> deserialized = JsonSerializer.Deserialize<Optional<string>>(json, options);
// Returns Optional.Some("John")

Optional<string> fromNull = JsonSerializer.Deserialize<Optional<string>>("null", options);
// Returns Optional.Empty<string>()
```

### AOT-Compatible Configuration

```csharp
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Optionals;

// Use source-generated context for AOT compatibility
var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() },
    TypeInfoResolver = JsonTypeInfoResolver.Combine(
        OptionalJsonContext.Default,  // Built-in primitive types
        MyCustomContext.Default)      // Your custom types
};

// Supported primitive types in OptionalJsonContext:
// - Optional<string>, Optional<int>, Optional<long>, Optional<float>
// - Optional<double>, Optional<decimal>, Optional<short>, Optional<ushort>
// - Optional<byte>, Optional<bool>, Optional<DateTime>, Optional<DateTimeOffset>
// - Optional<Guid>
```

### Custom Types

```csharp
// Define your own JsonSerializerContext for custom types
[JsonSerializable(typeof(Optional<User>))]
[JsonSerializable(typeof(Optional<Order>))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Order))]
public partial class MyCustomContext : JsonSerializerContext { }

// Usage
var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() },
    TypeInfoResolver = JsonTypeInfoResolver.Combine(
        OptionalJsonContext.Default,
        MyCustomContext.Default)
};

var user = Optional.Some(new User { Name = "John" });
string json = JsonSerializer.Serialize(user, options);
// {"name":"John"}
```

---

## 💡 Common Patterns

### Repository Pattern

```csharp
public interface IUserRepository
{
    Optional<User> GetById(Guid id);
    Task<Optional<User>> GetByIdAsync(Guid id);
    Task<Optional<User>> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    public Optional<User> GetById(Guid id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        return user.ToOptional();
    }

    public async Task<Optional<User>> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user.ToOptional();
    }
}
```

### Service Layer

```csharp
public class UserService(IUserRepository repository)
{
    public async Task<Optional<UserDto>> GetUserProfileAsync(Guid userId)
    {
        return await repository.GetByIdAsync(userId)
            .BindAsync(async user =>
            {
                var profile = await LoadProfileAsync(user.Id);
                return new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Profile = profile
                };
            });
    }

    public async Task<string> GetUserDisplayNameAsync(Guid userId)
    {
        var user = await repository.GetByIdAsync(userId);
        return user
            .Bind(u => u.DisplayName ?? u.Email)
            .GetValueOrDefault("Anonymous");
    }
}
```

### Validation Chains

```csharp
public Optional<Order> ValidateOrder(Order order)
{
    return Optional.Some(order)
        .Where(o => o.Items.Any())
        .Where(o => o.Total > 0)
        .Where(o => o.CustomerId != Guid.Empty)
        .Map(o =>
        {
            o.ValidatedAt = DateTime.UtcNow;
            return o;
        });
}

// Usage
var result = ValidateOrder(order);
if (result.IsEmpty)
{
    return BadRequest("Order validation failed");
}
return Ok(result.Value);
```

---

## 📊 API Reference

### Optional Factory Methods

| Method | Description |
|--------|-------------|
| `Optional.Some<T>(T value)` | Creates an Optional containing a value |
| `Optional.Empty<T>()` | Creates an empty Optional |

### Optional<T> Properties

| Property | Description |
|----------|-------------|
| `Value` | Gets the value (throws if empty) |
| `IsEmpty` | Returns true if no value is present |
| `IsNotEmpty` | Returns true if a value is present |

### Optional<T> Methods

| Method | Description |
|--------|-------------|
| `GetValueOrDefault(T)` | Returns value or specified default |
| `GetValueOrDefault(Func<T>)` | Returns value or factory result |
| `TryGetValue(out T)` | Safely tries to get the value |
| `ToOptional<TU>()` | Converts to Optional of different type |
| `Map(Func<T, T>)` | Transforms value if present |
| `Bind<TU>(Func<T, TU>)` | Transforms to different type |
| `Bind<TU>(Func<T, Optional<TU>>)` | FlatMaps to Optional |
| `Empty(Func<T>)` | Provides fallback when empty |

### Extension Methods

| Method | Description |
|--------|-------------|
| `ToOptional()` | Converts nullable to Optional |
| `Select<TU>()` | LINQ projection |
| `SelectMany<TU>()` | LINQ flat-map |
| `Where()` | LINQ filtering |
| `FirstOrEmpty()` | First element as Optional |
| `WhereSome()` | Filters non-empty Optionals |

---

## ✅ Best Practices

### ✅ Do

- **Use `Optional<T>` for potentially absent values** — Make absence explicit in your API
- **Chain operations with `Map` and `Bind`** — Avoid nested if-checks
- **Use `GetValueOrDefault()` with meaningful defaults** — Never return null from Optional methods
- **Leverage LINQ syntax** — `from x in optional select ...` is readable
- **Use `TryGetValue` for performance-critical paths** — Avoids exceptions
- **Configure JSON with `OptionalJsonContext`** — Ensures AOT compatibility

### ❌ Don't

- **Access `Value` without checking `IsNotEmpty`** — Throws `InvalidOperationException`
- **Return `Optional<T?>` (nullable inside Optional)** — Confusing, pick one approach
- **Use Optional for error handling** — Use `Result<T>` pattern instead
- **Create `Optional<Optional<T>>`** — Flatten immediately with implicit conversion

---

## 📚 Related Packages

| Package | Description |
|---------|-------------|
| **System.Results** | Result pattern for success/failure outcomes |
| **System.Primitives** | Core primitives and value objects |

---

## 📄 License

Apache License 2.0 - Copyright © Kamersoft 2025

Contributions welcome at [Xpandables.Net on GitHub](https://github.com/Francescolis/Xpandables.Net).
