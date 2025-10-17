# ?? Xpandables.Net.Optionals

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Null-Safe Optional Values for .NET** - Eliminate null reference exceptions with a type-safe Optional type inspired by Rust's `Option<T>` and F#'s `Option`.

---

## ?? Overview

`Xpandables.Net.Optionals` provides a robust `Optional<T>` type that represents values that may or may not exist. Say goodbye to null reference exceptions and defensive `null` checks—embrace explicit, type-safe handling of optional values.

### ?? Key Features

- ?? **Null Safety** - Explicit handling of missing values
- ?? **Type-Safe** - Compile-time checks for value presence
- ?? **Functional Operators** - Map, Bind, Empty, and more
- ?? **JSON Serialization** - Seamless System.Text.Json support
- ? **Performance** - Zero-allocation struct-based design
- ?? **LINQ Integration** - Works with standard LINQ operators

---

## ?? Getting Started

### Installation

```bash
dotnet add package Xpandables.Net.Optionals
```

### Basic Usage

```csharp
using Xpandables.Net.Optionals;

// Creating optionals
Optional<User> someUser = Optional.Some(new User { Name = "John" });
Optional<User> noUser = Optional.Empty<User>();

// Checking for values
if (someUser.IsNotEmpty)
{
    Console.WriteLine(someUser.Value.Name); // Safe access
}

// Pattern matching with Map and Empty
string message = someUser
    .Map(user => $"Hello, {user.Name}")
    .Empty(() => "No user found");
```

---

## ??? Core Concepts

### Creating Optionals

```csharp
// From a value
Optional<string> some = Optional.Some("Hello");

// Empty optional
Optional<string> empty = Optional.Empty<string>();

// From a nullable value using extension method
string? nullable = GetNullableString();
Optional<string> optional = nullable.ToOptional();

// Implicit conversion
Optional<int> number = 42; // Automatically wrapped
```

### Checking State

```csharp
Optional<User> user = GetUser();

// Check if value exists
if (user.IsNotEmpty)
{
    // Value is guaranteed to exist
    Console.WriteLine(user.Value.Name);
}

if (user.IsEmpty)
{
    // Handle missing value
    Console.WriteLine("User not found");
}

// Try to get value safely
if (user.TryGetValue(out User value))
{
    Console.WriteLine(value.Name);
}
```

---

## ?? Functional Operations

### Map - Transform the Value

```csharp
Optional<int> number = Optional.Some(5);

// Map to a different value of the same type
Optional<int> doubled = number.Map(n => n * 2); // Optional.Some(10)

// Empty optionals remain empty
Optional<int> empty = Optional.Empty<int>();
Optional<int> stillEmpty = empty.Map(n => n * 2); // Still empty

// Execute action on value
Optional<User> user = Optional.Some(new User { Name = "John" });
user.Map(u => Console.WriteLine(u.Name)); // Prints "John"
```

### Bind - Chain Optional Operations and Transform Types

```csharp
Optional<User> GetUser(int id) => /* ... */;
Optional<Address> GetAddress(User user) => /* ... */;

Optional<int> userId = Optional.Some(123);

// Bind allows you to transform to a different type
Optional<Address> address = userId
    .Bind(GetUser)           // Transform int to User
    .Bind(GetAddress);       // Transform User to Address

// Simple type transformation
Optional<int> number = Optional.Some(42);
Optional<string> text = number.Bind(n => n.ToString()); // Optional.Some("42")
```

### Empty - Handle Missing Values (Pattern Matching)

```csharp
Optional<User> user = GetUser(userId);

// Provide a default value when empty
string greeting = user
    .Map(u => $"Welcome back, {u.Name}!")
    .Empty(() => "Please log in");

// Execute action when empty
user
    .Map(u => Console.WriteLine($"Found: {u.Name}"))
    .Empty(() => Console.WriteLine("User not found"));

// Return alternative optional when empty
Optional<User> foundUser = user
    .Empty(() => GetDefaultUser());
```

### Where - Conditional Optionals

```csharp
Optional<int> number = Optional.Some(42);

Optional<int> evenNumber = number.Where(n => n % 2 == 0); // Some(42)
Optional<int> oddNumber = number.Where(n => n % 2 != 0);  // Empty
```

---

## ?? Advanced Examples

### Example 1: Safe Dictionary Lookup

```csharp
Dictionary<string, User> users = GetUsers();

Optional<User> user = users.TryGetValue("john", out User foundUser)
    ? Optional.Some(foundUser)
    : Optional.Empty<User>();

string email = user
    .Bind(u => u.Email)
    .Empty(() => "no-reply@example.com");
```

### Example 2: Chaining Database Operations

```csharp
public async Task<Optional<OrderDto>> GetOrderDetailsAsync(Guid orderId)
{
    Optional<Order> order = await _repository.FindByIdAsync(orderId);
    
    return await order
        .BindAsync(o => GetCustomerAsync(o.CustomerId))  // Optional<Customer>
        .BindAsync(customer => Task.FromResult(new OrderDto
        {
            OrderId = orderId,
            CustomerName = customer.Name
        }));
}
```

### Example 3: Handling Null API Responses

```csharp
public async Task<Optional<WeatherData>> GetWeatherAsync(string city)
{
    var response = await _httpClient.GetAsync($"/weather?city={city}");
    
    if (!response.IsSuccessStatusCode)
        return Optional.Empty<WeatherData>();
    
    var data = await response.Content.ReadFromJsonAsync<WeatherData>();
    return data.ToOptional();
}

// Usage
var weather = await GetWeatherAsync("London");

weather
    .Map(data => Console.WriteLine($"Temperature: {data.Temperature}°C"))
    .Empty(() => Console.WriteLine("Weather data not available"));
```

### Example 4: LINQ Integration

```csharp
List<Optional<User>> optionalUsers = GetOptionalUsers();

// Filter out empty optionals and get values
List<User> validUsers = optionalUsers
    .Where(opt => opt.IsNotEmpty)
    .Select(opt => opt.Value)
    .ToList();

// Using LINQ Select and SelectMany with optionals
var userNames = optionalUsers
    .Select(opt => opt.Bind(u => u.Name))
    .Where(opt => opt.IsNotEmpty)
    .Select(opt => opt.Value)
    .ToList();
```

---

## ??? Utility Methods

### GetValueOrDefault

```csharp
Optional<string> name = GetName();

// Provide a default value
string displayName = name.GetValueOrDefault("Anonymous");

// Provide a default value factory
string computed = name.GetValueOrDefault(() => 
    ExpensiveDefaultComputation());
```

### TryGetValue

```csharp
Optional<User> user = GetUser();

if (user.TryGetValue(out User foundUser))
{
    Console.WriteLine($"Found: {foundUser.Name}");
}
else
{
    Console.WriteLine("No user found");
}
```

### Operators

```csharp
Optional<int> a = Optional.Some(5);
Optional<int> b = Optional.Some(10);

// Equality
bool equal = a == b; // false

// Comparison
bool greater = a > b; // false
```

---

## ?? Best Practices

1. **Prefer Optional over null**: Make optionality explicit
   ```csharp
   // Don't
   User? GetUser(int id);
   
   // Do
   Optional<User> GetUser(int id);
   ```

2. **Use Map().Empty() for branching**: Chain operations explicitly
   ```csharp
   // Don't
   if (optional.IsNotEmpty)
   {
       DoSomething(optional.Value);
   }
   
   // Do
   optional
       .Map(value => DoSomething(value))
       .Empty(() => HandleEmpty());
   ```

3. **Chain operations**: Use Map and Bind for transformations
   ```csharp
   return user
       .Bind(u => u.Address)
       .Where(a => a.IsValid)
       .Bind(a => a.ToString());
   ```

4. **Return Empty instead of null**: Be consistent
   ```csharp
   // Explicit and safe
   if (user == null)
       return Optional.Empty<User>();
   ```

5. **Use Bind for type transformations**: Use Map for same-type operations
   ```csharp
   // Bind changes the type
   Optional<string> text = number.Bind(n => n.ToString());
   
   // Map keeps the same type
   Optional<int> doubled = number.Map(n => n * 2);
   ```

---

## ?? Integration with Other Packages

### With ExecutionResults

```csharp
public async Task<ExecutionResult<User>> GetUserAsync(Guid id)
{
    Optional<User> user = await _repository.FindAsync(id);
    
    return user
        .Map(u => ExecutionResult.Success(u))
        .Empty(() => ExecutionResult.NotFound<User>()
            .WithTitle("User not found"));
}
```

### With Async Operations

```csharp
// Async Map - transform the value asynchronously
Optional<User> user = await GetUserAsync(id);
Optional<User> processed = await user.MapAsync(async u => 
{
    await ProcessUserAsync(u);
    return u;
});

// Async Bind - transform to a different type asynchronously
Optional<Profile> profile = await user.BindAsync(async u => 
    await GetProfileAsync(u.Id));

// Async Empty - provide default value asynchronously
Optional<User> userWithDefault = await user.EmptyAsync(async () =>
    await GetDefaultUserAsync());

// Chain async operations
var result = await user
    .MapAsync(async u => await ValidateUserAsync(u))
    .BindAsync(async u => await GetUserDetailsAsync(u))
    .EmptyAsync(async () => await CreateDefaultDetailsAsync());
```

---

## ?? JSON Serialization

Optionals are automatically serialized/deserialized:

```csharp
public class UserDto
{
    public Optional<string> MiddleName { get; set; }
    public Optional<int> Age { get; set; }
}

var dto = new UserDto 
{ 
    MiddleName = Optional.Some("James"),
    Age = Optional.Empty<int>()
};

string json = JsonSerializer.Serialize(dto);
// { "middleName": "James", "age": null }
```

---

## ?? Complete API Reference

### Optional<T> Methods

| Method | Description |
|--------|-------------|
| `Map(Func<T, T>)` | Transform the value to the same type |
| `Map(Func<T, Optional<T>>)` | Transform the value to an optional of the same type |
| `Map(Action<T>)` | Execute an action on the value |
| `Map(Action)` | Execute an action if value exists |
| `Bind<TU>(Func<T, TU>)` | Transform to a different type |
| `Bind<TU>(Func<T, Optional<TU>>)` | Transform to an optional of a different type |
| `Empty(Func<T>)` | Provide value when empty |
| `Empty(Func<Optional<T>>)` | Provide optional when empty |
| `Empty(Action)` | Execute action when empty |
| `Where(Func<T, bool>)` | Filter based on predicate |
| `GetValueOrDefault(T)` | Get value or default |
| `GetValueOrDefault(Func<T>)` | Get value or compute default |
| `TryGetValue(out T)` | Try to get the value safely |
| `MapAsync(...)` | Async version of Map |
| `BindAsync<TU>(...)` | Async version of Bind |
| `EmptyAsync(...)` | Async version of Empty |

### Extension Methods (LINQ)

| Method | Description |
|--------|-------------|
| `ToOptional<T>()` | Convert nullable to Optional |
| `Select<TU>(...)` | LINQ projection (same as Bind) |
| `SelectMany<TU>(...)` | LINQ monadic binding |
| `Where(...)` | LINQ filtering |

---

## ?? Related Packages

- [`Xpandables.Net.ExecutionResults`](../Xpandables.Net.ExecutionResults/README.md) - Result pattern
- [`Xpandables.Net.Tasks`](../Xpandables.Net.Tasks/README.md) - Mediator pattern

---

## ?? License

Apache License 2.0 - Copyright © Kamersoft 2025
