# Introduction 
Provides with useful interfaces contracts in **.Net 9.0** and some implementations mostly following the spirit of SOLID principles, Commands...
The library is strongly-typed, which means it should be hard to make invalid requests and it also makes it easy to discover available methods and properties though IntelliSense.

Feel free to fork this project, make your own changes and create a pull request.


This project is licensed under the Apache License, Version 2.0. See the [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) file for details.


# Getting Started

## Optional

The **Optional< T>** is a C# implementation of an optional value that may or may not be present.
This implementation is part of the [Xpandables.Net](https://www.nuget.org/packages/Xpandables.Net) library and is designed to work with .NET 9.
The **Optional< T>** struct provides a way to represent optional values, similar to the Nullable< T> type but for reference types and value types alike.

## Features
- Is a struct, immutable, a generic type, so it can hold a value of any type.
- Represents an optional value that may or may not be present.
- Provides a way to work with optional values in a functional way.
- Provides a way to create an optional value from a value or from an empty value.
- Supports JSON serialization through **OptionalJsonConverterFactory**.
- Implements **IEnumerable< T>** to allow iteration over the optional value.

## Usage

### Creating an Optional Value

You can create an Optional< T> value using the constructor or implicit conversion.
```csharp

using Xpandables.Net.Optionals;

// Creating an optional with a value
Optional<int> optionalWithValue = Optional.Some(42);

// Creating an empty optional
Optional<int> emptyOptional = Optional.Empty<int>();

```

### Checking for Value Presence

You can check if the optional has a value using the *IsEmpty* or *IsNotEmpty* properties.
```csharp

if (emptyOptional.IsEmpty)
{
    Console.WriteLine("Optional is empty.");
}

if (optionalWithValue.IsNotEmpty)
{
    Console.WriteLine("Optional is not empty.");
}

```

### Accessing the Value

You can access the value of the **Optional** using the Value property.
Note that accessing the value when it is not present will throw an *InvalidOperationException*.
```csharp

try
{
    int value = optionalWithValue.Value;
    Console.WriteLine($"Value: {value}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
}


```

### Iterating over the Value

Since **Optional< T>** implements *IEnumerable< T>*, you can use it in a foreach loop.
```csharp

foreach (var value in optionalWithValue)
{
    Console.WriteLine($"Value: {value}");
}

foreach (var value in emptyOptional)
{
    Console.WriteLine($"Value: {value}"); // This will not execute
}

```

### Using the *Map* Method

The *Map* method allows you to act on the value inside the optional if it is present.
```csharp

Optional<int> optional = 42;
Optional<int> mappedOptional = optional.Map(value => value * 2);

if (mappedOptional.HasValue)
{
    Console.WriteLine($"Mapped Value: {mappedOptional.Value}"); 
    // Output: Mapped Value: 84
}

```

### Using the *Bind* Method

The *Bind* method allows you to transform the value inside the optional to another type and return a new optional.

```csharp

Optional<string> optional = Optional.Some("Hello Word");
Optional<int> boundOptional = optional.Bind(value => value.Length);

if (boundOptional.HasValue)
{
    Console.WriteLine($"Bound Value: {boundOptional.Value}"); 
    // Output: Bound Value: 10
}

```

### Using the *Bind* Method

The *Empty* method allows you to provide a value if the current optional is empty.

```csharp

public string GetName()
{
    Optional<Name> optional = function call;
    return optional
        .Empty("No Name");

    // If the optional has a value, the function value will be returned.
    // Otherwise, the Empty value will be returned.
}

```

### JSON Serialization

The **Optional< T>** struct is decorated with *OptionalJsonConverterFactory* to support JSON serialization.

```csharp

using System.Text.Json;

var optional = Optional.Some(42);
string json = JsonSerializer.Serialize(optional);
Console.WriteLine(json); // Output: {"Value":42}

var deserializedOptional = JsonSerializer.Deserialize<Optional<int>>(json);
Console.WriteLine(deserializedOptional.HasValue); // True
Console.WriteLine($"Deserialized Value: {deserializedOptional.Value}"); 
// Output: Deserialized Value: 42

// anonymous type
var anonymous = Optional.Some(new { Name = "Hello World" });
string anonymousJson = JsonSerializer.Serialize(anonymous);
Console.WriteLine(anonymousJson); // Output: {"Name":"Hello World"}

var deserializedAnonymous = DeserializeAnonymousType(anonymousJson, anonymous);
// or you can use an anonymous instance
// var deserializedAnonymous = result.DeserializeAnonymousType(anonymousJson, Optional.Some(new { Name = string.Empty }));
Console.WriteLine($"Deserialized Anonymous Value: {deserializedAnonymous.Value.Name}"); 
// Output: Deserialized Anonymous Value: Hello World

static T? DeserializeAnonymousType<T>(string json, T _, JsonSerializerOptions? options = default) =>
    JsonSerializer.Deserialize<T>(json, options);


```

