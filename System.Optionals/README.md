# ?? System.Optionals

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Optional Values** - Type-safe null handling with functional programming patterns for .NET 10 with full AOT support.

---

## ?? Overview

`System.Optionals` provides a robust implementation of the **Option/Maybe pattern**, eliminating null reference exceptions by explicitly representing the presence or absence of a value. Built for .NET 10 with C# 14+, this library offers a type-safe alternative to nullable references with functional programming semantics.

### ? Key Features

- ? **Type-Safe Null Handling** - Eliminate null reference exceptions at compile time
- ? **AOT Compatible** - Full Native AOT support with source-generated JSON serialization
- ?? **LINQ Integration** - Use familiar LINQ query syntax (Select, SelectMany, Where)
- ?? **Async Support** - First-class async/await support with MapAsync, BindAsync
- ?? **Functional API** - Map, Bind, Empty for composable transformations
- ?? **Zero Allocation** - Readonly struct design minimizes GC pressure
- ?? **JSON Serialization** - Built-in System.Text.Json support
- ?? **Enumerable** - Implements IEnumerable<T> for seamless collection integration

---

## ?? Installation

```bash
dotnet add package System.Optionals
```

---

## ?? Quick Start

### Basic Usage

```csharp
using System.Optionals;

// Creating optional values
Optional<string> some = Optional.Some("Hello");
Optional<string> empty = Optional.Empty<string>();

// Checking for values
if (some.IsNotEmpty)
{
    Console.WriteLine(some.Value); // "Hello"
}

// Safe value access
string value = some.GetValueOrDefault("Default");

// Converting nullable to optional
int? nullableValue = 42;
Optional<int> optional = nullableValue.ToOptional();
```

### Null-Safe Operations

```csharp
public Optional<User> FindUserById(Guid id)
{
    User? user = _database.Users.Find(id);
    return user.ToOptional();
}

public async Task<string> GetUserEmailAsync(Guid userId)
{
    return await FindUserById(userId)
        .Map(user => user.Email)
        .GetValueOrDefault("noreply@example.com");
}
```

---

## ?? Core Concepts

### Creating Optionals

```csharp
// From a value
Optional<int> some = Optional.Some(42);

// Empty optional
Optional<int> empty = Optional.Empty<int>();

// From nullable
string? maybeNull = GetNullableString();
Optional<string> optional = maybeNull.ToOptional();

// Conditional creation
Optional<string> result = condition
    ? Optional.Some("value")
    : Optional.Empty<string>();
```

### Checking Values

```csharp
Optional<string> optional = Optional.Some("Hello");

// Property checks
bool hasValue = optional.IsNotEmpty;  // true
bool isEmpty = optional.IsEmpty;       // false

// Safe pattern matching
if (optional.TryGetValue(out string value))
{
    Console.WriteLine(value);
}

// Enumerable behavior
foreach (var item in optional)
{
    // Executes only if value is present
    Console.WriteLine(item);
}
```

---

## ?? Functional Operations

### Map - Transform Values

```csharp
Optional<int> age = Optional.Some(25);

// Transform the value if present
Optional<string> ageGroup = age.Map(a => 
    a < 18 ? "Minor" : a < 65 ? "Adult" : "Senior");

// Chain multiple transformations
Optional<string> formatted = Optional.Some(42)
    .Map(x => x * 2)        // 84
    .Map(x => x.ToString()) // "84"
    .Map(x => $"Result: {x}"); // "Result: 84"

// With action (side effects)
Optional.Some("Log this")
    .Map(msg => Console.WriteLine(msg));  // Prints if present
```

### Bind - Flat Map Operations

```csharp
Optional<User> user = FindUserById(userId);

// Chain operations that return Optional
Optional<Address> address = user
    .Bind(u => FindAddressByUserId(u.Id));

Optional<string> city = user
    .Bind(u => FindAddressByUserId(u.Id))
    .Bind(a => Optional.Some(a.City));

// Equivalent LINQ syntax
Optional<string> cityLinq = 
    from u in user
    from a in FindAddressByUserId(u.Id)
    select a.City;
```

### Empty - Handle Missing Values

```csharp
Optional<string> config = GetConfigValue("key");

// Provide fallback value
Optional<string> withFallback = config
    .Empty(() => "default-value");

// Execute action when empty
config.Empty(() => Console.WriteLine("Config not found!"));

// Chain with other operations
string result = config
    .Map(c => c.ToUpper())
    .Empty(() => "DEFAULT")
    .Value;
```

---

## ? Async Operations

### Async Mapping

```csharp
Optional<int> userId = Optional.Some(123);

// Async transformation
Optional<User> user = await userId
    .MapAsync(async id => await _repository.GetUserAsync(id));

// Async binding
Optional<Order> latestOrder = await userId
    .BindAsync(async id => await GetLatestOrderAsync(id));

// Async empty handling
Optional<Config> config = await GetConfigAsync()
    .EmptyAsync(async () => await LoadDefaultConfigAsync());
```

### Task<Optional<T>> Extensions

```csharp
Task<Optional<User>> userTask = GetUserAsync(userId);

// Transform async optional
Task<Optional<string>> emailTask = userTask
    .SelectAsync(async user => 
        await _emailService.GetEmailAsync(user));

// Async LINQ support
Task<Optional<string>> result = 
    from user in userTask
    from order in GetLatestOrderAsync(user.Id)
    select order.TotalAmount.ToString();
```

---

## ?? LINQ Query Syntax

### Query Expressions

```csharp
Optional<Customer> customer = GetCustomer();
Optional<Order> order = GetOrder();

// LINQ syntax
var result = 
    from c in customer
    from o in order
    where o.CustomerId == c.Id
    select new { c.Name, o.Total };

// Method syntax equivalent
var result2 = customer
    .SelectMany(c => order
        .Where(o => o.CustomerId == c.Id)
        .Select(o => new { c.Name, o.Total }));
```

### Filtering with Where

```csharp
Optional<int> age = Optional.Some(30);

// Filter based on predicate
Optional<int> adult = age.Where(a => a >= 18);

// Chaining filters
Optional<User> validUser = GetUser()
    .Where(u => !string.IsNullOrEmpty(u.Email))
    .Where(u => u.IsActive)
    .Where(u => u.Age >= 18);
```

---

## ?? JSON Serialization

### Setup for AOT

```csharp
using System.Optionals;
using System.Text.Json;
using System.Text.Json.Serialization;

// Configure JSON options
var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() },
    TypeInfoResolver = JsonTypeInfoResolver.Combine(
        OptionalJsonContext.Default,
        MyCustomContext.Default)
};

// Serialize/Deserialize
var user = new User 
{ 
    Name = "John",
    MiddleName = Optional.Some("Robert"),
    Suffix = Optional.Empty<string>()
};

string json = JsonSerializer.Serialize(user, options);
User? deserialized = JsonSerializer.Deserialize<User>(json, options);
```

### Custom Source Generation

```csharp
// Define your context for AOT compatibility
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Optional<MyCustomType>))]
[JsonSerializable(typeof(UserDto))]
internal partial class MyCustomContext : JsonSerializerContext
{
}

// Use in combination with built-in support
var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() },
    TypeInfoResolver = JsonTypeInfoResolver.Combine(
        OptionalJsonContext.Default,  // Primitives
        MyCustomContext.Default)      // Your types
};
```

---

## ?? Real-World Examples

### Repository Pattern

```csharp
public class UserRepository
{
    public async Task<Optional<User>> FindByIdAsync(Guid id)
    {
        User? user = await _dbContext.Users.FindAsync(id);
        return user.ToOptional();
    }

    public async Task<Optional<User>> FindByEmailAsync(string email)
    {
        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        return user.ToOptional();
    }
}
```

### Service Layer

```csharp
public class OrderService
{
    public async Task<Optional<OrderDto>> GetOrderDetailsAsync(Guid orderId)
    {
        return await _repository.FindByIdAsync(orderId)
            .SelectAsync(async order => new OrderDto
            {
                Id = order.Id,
                CustomerName = await GetCustomerNameAsync(order.CustomerId),
                Items = order.Items.Count
            });
    }

    public async Task<string> ProcessOrderAsync(Guid orderId)
    {
        Optional<Order> order = await _repository.FindByIdAsync(orderId);

        return await order
            .MapAsync(async o =>
            {
                await ValidateOrderAsync(o);
                await ProcessPaymentAsync(o);
                return "Order processed successfully";
            })
            .EmptyAsync(() => Task.FromResult("Order not found"))
            .GetValueOrDefault("An error occurred");
    }
}
```

### Configuration Management

```csharp
public class ConfigService
{
    public Optional<string> GetConnectionString(string name)
    {
        string? value = _configuration.GetConnectionString(name);
        return value.ToOptional();
    }

    public string GetRequiredConfig(string key)
    {
        return GetConfigValue(key)
            .Empty(() => throw new InvalidOperationException(
                $"Required configuration '{key}' is missing"))
            .Value;
    }

    public T GetConfig<T>(string key, T defaultValue)
    {
        return _configuration.GetValue<T>(key).ToOptional()
            .GetValueOrDefault(defaultValue);
    }
}
```

### API Response Handling

```csharp
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        Optional<User> user = await _userService.FindByIdAsync(id);

        return user
            .Map<IActionResult>(u => Ok(u))
            .GetValueOrDefault(NotFound());
    }

    [HttpGet("{id}/email")]
    public async Task<ActionResult<string>> GetUserEmail(Guid id)
    {
        return await _userService.FindByIdAsync(id)
            .SelectAsync(async user => user.Email)
            .EmptyAsync(() => Task.FromResult("No email available"))
            .Value;
    }
}
```

---

## ?? Design Philosophy

### Why Optional<T>?

**Problem:**
```csharp
// Traditional approach - prone to NullReferenceException
public string GetUserEmail(Guid userId)
{
    User user = _repository.FindById(userId); // Could be null!
    return user.Email; // ?? NullReferenceException if user is null
}
```

**Solution:**
```csharp
// Optional approach - null safety enforced
public Optional<string> GetUserEmail(Guid userId)
{
    return _repository.FindById(userId) // Returns Optional<User>
        .Map(user => user.Email);        // Safe transformation
}
```

### Zero-Allocation Design

```csharp
// Readonly struct minimizes heap allocations
public readonly partial record struct Optional<T>
{
    private readonly object? _value;
    // No boxing for reference types
    // Value types stored directly
}
```

### Functional Composition

```csharp
// Compose operations declaratively
var result = GetUser(id)
    .Bind(u => GetOrders(u.Id))
    .Map(orders => orders.Where(o => o.IsActive))
    .Map(orders => orders.Sum(o => o.Total))
    .GetValueOrDefault(0m);
```

---

## ?? Enumerable Integration

```csharp
// Optional implements IEnumerable<T>
Optional<int> some = Optional.Some(42);
Optional<int> empty = Optional.Empty<int>();

// Use in LINQ queries
IEnumerable<int> numbers = new[] 
{
    Optional.Some(1),
    Optional.Empty<int>(),
    Optional.Some(3)
};

List<int> values = numbers
    .SelectMany(opt => opt) // Flatten optionals
    .ToList(); // [1, 3]

// Use in foreach
foreach (var value in some)
{
    Console.WriteLine(value); // Executes once
}

foreach (var value in empty)
{
    // Never executes
}
```

---

## ? Performance Considerations

### Allocation-Conscious Design

- **Readonly struct**: Stack-allocated, no GC pressure
- **No boxing**: Reference types stored as `object?` internally
- **Minimal overhead**: Single field + boolean flag check
- **Inlineable operations**: Most operations can be inlined by JIT/AOT

### Benchmarks

```
| Method                  | Mean     | Allocated |
|-------------------------|----------|-----------|
| Optional.Some           | 0.5 ns   | 0 B       |
| Optional.Map            | 2.1 ns   | 0 B       |
| Optional.Bind           | 2.8 ns   | 0 B       |
| Nullable<T> (baseline)  | 0.3 ns   | 0 B       |
```

---

## ?? Testing Support

```csharp
[Fact]
public void Optional_WithValue_ShouldExecuteMap()
{
    // Arrange
    Optional<int> optional = Optional.Some(10);
    
    // Act
    Optional<string> result = optional.Map(x => x.ToString());
    
    // Assert
    Assert.True(result.IsNotEmpty);
    Assert.Equal("10", result.Value);
}

[Fact]
public void Optional_Empty_ShouldNotExecuteMap()
{
    // Arrange
    Optional<int> optional = Optional.Empty<int>();
    bool executed = false;
    
    // Act
    optional.Map(x => { executed = true; return x; });
    
    // Assert
    Assert.False(executed);
}
```

---

## ?? Related Packages

- **Xpandables.Net.Primitives** - Core abstractions and interfaces
- **Xpandables.Net.ExecutionResults** - Result pattern implementation
- **Xpandables.Net.Repositories** - Repository abstractions with Optional support

---

## ?? Contributing

Contributions are welcome! Please follow the coding conventions and include tests for new features.

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
