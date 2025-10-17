#  Xpandables.Net.Optionals

[![NuGet](https://img.shields.io/badge/NuGet-preview-orange.svg)](https://www.nuget.org/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

> **Null-Safe Optional Values for .NET** - Eliminate null reference exceptions with a type-safe Optional type inspired by Rust's `Option<T>` and F#'s `Option`.

---

##  Overview

`Xpandables.Net.Optionals` provides a robust `Optional<T>` type that represents values that may or may not exist. Say goodbye to null reference exceptions and defensive `null` checks—embrace explicit, type-safe handling of optional values.

### Key Features

-  **Null Safety** - Explicit handling of missing values
-  **Type-Safe** - Compile-time checks for value presence
-  **Functional Operators** - Map, Bind, Match, and more
-  **JSON Serialization** - Seamless System.Text.Json support
-  **Performance** - Zero-allocation struct-based design
-  **LINQ Integration** - Works with standard LINQ operators

---

##  Getting Started

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

// Pattern matching
string message = someUser
    .Map(user => $"Hello, {user.Name}")
    .Empty(() => "No user found");
```

---

##  Core Concepts

### Creating Optionals

```csharp
// From a value
Optional<string> some = Optional.Some("Hello");

// Empty optional
Optional<string> empty = Optional.Empty<string>();

// From a nullable value
string? nullable = GetNullableString();
Optional<string> optional = Optional.FromNullable(nullable);

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
```

---

##  Functional Operations

### Map - Transform the Value

```csharp
Optional<int> number = Optional.Some(5);

Optional<string> text = number.Map(n => n.ToString()); // Optional.Some("5")
Optional<int> doubled = number.Map(n => n * 2);        // Optional.Some(10)

// Empty optionals remain empty
Optional<int> empty = Optional.Empty<int>();
Optional<string> stillEmpty = empty.Map(n => n.ToString()); // Still empty
```

### Bind - Chain Optional Operations

```csharp
Optional<User> GetUser(int id) => /* ... */;
Optional<Address> GetAddress(User user) => /* ... */;

Optional<int> userId = Optional.Some(123);

Optional<Address> address = userId
    .Bind(GetUser)           // Optional<User>
    .Bind(GetAddress);       // Optional<Address>
```

### Match - Pattern Matching

```csharp
Optional<User> user = GetUser(userId);

string greeting = user.Match(
    onSome: u => $"Welcome back, {u.Name}!",
    onEmpty: () => "Please log in"
);

// Execute actions based on state
user.Match(
    onSome: u => Console.WriteLine($"Found: {u.Name}"),
    onEmpty: () => Console.WriteLine("User not found")
);
```

### Filter - Conditional Optionals

```csharp
Optional<int> number = Optional.Some(42);

Optional<int> evenNumber = number.Filter(n => n % 2 == 0); // Some(42)
Optional<int> oddNumber = number.Filter(n => n % 2 != 0);  // Empty
```

---

##  Advanced Examples

### Example 1: Safe Dictionary Lookup

```csharp
Dictionary<string, User> users = GetUsers();

Optional<User> user = users.TryGetOptional("john");

string email = user
    .Map(u => u.Email)
    .GetValueOrDefault("no-reply@example.com");
```

### Example 2: Chaining Database Operations

```csharp
public async Task<Optional<OrderDto>> GetOrderDetailsAsync(Guid orderId)
{
    return await _repository
        .FindByIdAsync(orderId)                    // Optional<Order>
        .BindAsync(order => GetCustomerAsync(order.CustomerId))  // Optional<Customer>
        .MapAsync(customer => new OrderDto
        {
            OrderId = orderId,
            CustomerName = customer.Name
        });
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
    return Optional.FromNullable(data);
}

// Usage
var weather = await GetWeatherAsync("London");

weather.Match(
    onSome: data => Console.WriteLine($"Temperature: {data.Temperature}°C"),
    onEmpty: () => Console.WriteLine("Weather data not available")
);
```

### Example 4: LINQ Integration

```csharp
List<Optional<User>> optionalUsers = GetOptionalUsers();

// Filter out empty optionals and get values
List<User> validUsers = optionalUsers
    .Where(opt => opt.IsNotEmpty)
    .Select(opt => opt.Value)
    .ToList();

// Using extension methods
List<User> users = optionalUsers.Choose(); // Built-in helper
```

---

##  Utility Methods

### GetValueOrDefault

```csharp
Optional<string> name = GetName();

// Provide a default value
string displayName = name.GetValueOrDefault("Anonymous");

// Provide a default value factory
string computed = name.GetValueOrDefault(() => 
    ExpensiveDefaultComputation());
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

##  Best Practices

1. **Prefer Optional over null**: Make optionality explicit
   ```csharp
   // Don't
   User? GetUser(int id);
   
   // Do
   Optional<User> GetUser(int id);
   ```

2. **Use Match for branching**: Avoid direct `.Value` access
   ```csharp
   // Don't
   if (optional.IsNotEmpty)
   {
       DoSomething(optional.Value);
   }
   
   // Do
   optional.Match(
       onSome: value => DoSomething(value),
       onEmpty: () => HandleEmpty()
   );
   ```

3. **Chain operations**: Use Map and Bind for transformations
   ```csharp
   return user
       .Map(u => u.Address)
       .Filter(a => a.IsValid)
       .Map(a => a.ToString());
   ```

4. **Return Empty instead of null**: Be consistent
   ```csharp
   // Explicit and safe
   if (user == null)
       return Optional.Empty<User>();
   ```

---

##  Integration with Other Packages

### With ExecutionResults

```csharp
public async Task<ExecutionResult<User>> GetUserAsync(Guid id)
{
    Optional<User> user = await _repository.FindAsync(id);
    
    return user.Match(
        onSome: u => ExecutionResult.Success(u),
        onEmpty: () => ExecutionResult.NotFound<User>()
            .WithTitle("User not found")
    );
}
```

### With Async Operations

```csharp
// Async Map
Optional<User> user = await GetUserAsync(id);
Optional<UserDto> dto = await user.MapAsync(async u => 
    await ConvertToDtoAsync(u));

// Async Bind
Optional<Profile> profile = await user.BindAsync(async u => 
    await GetProfileAsync(u.Id));
```

---

##  JSON Serialization

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

##  Related Packages

- [`Xpandables.Net.ExecutionResults`](../Xpandables.Net.ExecutionResults/README.md) - Result pattern
- [`Xpandables.Net.Tasks`](../Xpandables.Net.Tasks/README.md) - Mediator pattern

---

##  License

Apache License 2.0 - Copyright © Kamersoft 2025
