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

You can create an Optional< T> value using helpers or implicit conversion.
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

if (mappedOptional.IsNotEmpty)
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

### Using the *Empty* Method

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
Console.WriteLine(deserializedOptional.IsNotEmpty); // True
Console.WriteLine($"Deserialized Value: {deserializedOptional.Value}"); 
// Output: Deserialized Value: 42

// anonymous type
var anonymous = Optional.Some(new { Name = "Hello World" });
string anonymousJson = JsonSerializer.Serialize(anonymous);
Console.WriteLine(anonymousJson); // Output: {"Name":"Hello World"}

var deserializedAnonymous = DeserializeAnonymousType(anonymousJson, anonymous);
// or you can use an anonymous instance
// var deserializedAnonymous = DeserializeAnonymousType(
//      anonymousJson, 
//      Optional.Some(new { Name = string.Empty }));
Console.WriteLine($"Deserialized Anonymous Value: {deserializedAnonymous.Value.Name}"); 
// Output: Deserialized Anonymous Value: Name: Hello World

static T? DeserializeAnonymousType<T>(
    string json, T _, JsonSerializerOptions? options = default) =>
    JsonSerializer.Deserialize<T>(json, options);


```

### Chaining Methods in a Fluent Manner

You can chain the methods of **Optional< T>** in a fluent manner to produce the expected result.
```csharp

Optional<int> optional = Optional.Some(42);

Optional<string> result = optional
    .Map(value => value * 2) // Double the value
    .Bind(value => Optional.Some(value.ToString())) // Convert to string
    .Empty(() => "Default Value"); // Provide a default value if empty

if (result.IsNotEmpty)
{
    Console.WriteLine($"Result: {result.Value}"); // Output: Result: 84
}
else
{
    Console.WriteLine("Result is empty.");
}


```

## IOperationResult and OperationResults

### Overview

The `IOperationResult` and `OperationResults` classes are part of the `Xpandables.Net.Operations` namespace. They provide a structured way to handle the results of operations, encapsulating both success and failure scenarios with detailed information.

#### IOperationResult

The `IOperationResult` interface represents the result of an operation. It includes properties for status code, title, detail, location, result, errors, headers, and extensions. It also provides methods to check if the operation was successful and to retrieve any associated exceptions.

#### OperationResults

The `OperationResults` class provides static methods to create instances of `IOperationResult` for both success and failure scenarios. It includes methods to set various HTTP status codes and to include additional details like titles, details, locations, and errors.

### Usage

#### Creating a Success Operation Result

To create a success operation result, you can use the `Success` method from the `OperationResults` class. You can specify the status code, result, and other details.

```csharp

using System.Net;
using Xpandables.Net.Operations;

public class SampleUsage
{
    public IOperationResult CreateSuccessResult()
    {
        return OperationResults.Success(HttpStatusCode.OK)
            .WithTitle("Operation Successful")
            .WithDetail("The operation completed successfully.")
            .WithLocation(new Uri("http://example.com"))
            .Build();
    }

    public IOperationResult<string> CreateSuccessResultWithData()
    {
        return OperationResults.Success("Success Data", HttpStatusCode.OK)
            .WithTitle("Operation Successful")
            .WithDetail("The operation completed successfully with data.")
            .WithLocation(new Uri("http://example.com"))
            .Build();
    }
}

```

#### Creating a Failure Operation Result

To create a failure operation result, you can use the `Failure` method from the `OperationResults` class. You can specify the status code, errors, and other details.

```csharp

using System.Net;
using Xpandables.Net.Operations;

public class SampleUsage
{
    public IOperationResult CreateFailureResult()
    {
        return OperationResults.Failure(HttpStatusCode.BadRequest)
            .WithTitle("Operation Failed")
            .WithDetail("The operation failed due to bad request.")
            .WithError("ErrorKey", "ErrorMessage")
            .Build();
    }

    public IOperationResult<string> CreateFailureResultWithData()
    {
        return OperationResults.Failure<string>(HttpStatusCode.BadRequest)
            .WithTitle("Operation Failed")
            .WithDetail("The operation failed due to bad request with data.")
            .WithError("ErrorKey", "ErrorMessage")
            .Build();
    }
}

```

#### Using Predefined Methods

The `OperationResults` class also provides predefined methods for common HTTP status codes like `Ok`, `Created`, `NoContent`, `NotFound`, `BadRequest`, `Conflict`, `Unauthorized`, `InternalServerError`, and `ServiceUnavailable`.

```csharp

using Xpandables.Net.Operations;

public class SampleUsage
{
    public IOperationResult CreateOkResult()
    {
        return OperationResults.Ok()
            .WithTitle("Operation Successful")
            .WithDetail("The operation completed successfully.")
            .Build();
    }

    public IOperationResult<string> CreateNotFoundResult()
    {
        return OperationResults.NotFound<string>()
            .WithTitle("Resource Not Found")
            .WithDetail("The requested resource was not found.")
            .Build();
    }
}

```

The `IOperationResult` and `OperationResults` classes provide a flexible and structured way to handle operation results in your application. By using these classes, you can ensure that your operations return consistent and detailed results, making it easier to handle both success and failure scenarios.

## IHttpClientDispatcher and Related Classes

### Overview

The `IHttpClientDispatcher` interface and related classes in the `Xpandables.Net.Http` namespace provide a structured way to handle HTTP client requests and responses. These classes and interfaces allow you to configure, send, and process HTTP requests with detailed options and builders.

#### IHttpClientDispatcher

The `IHttpClientDispatcher` interface provides methods to handle HTTP client requests using a typed client HTTP client. It supports sending requests that do not return a response, requests that return a response of a specific type, and requests that return a stream that can be async-enumerated.

#### IHttpClientRequestOptionsBuilder

The `IHttpClientRequestOptionsBuilder` interface defines a builder for creating `HttpClientRequestOptionsAttribute`. This interface takes priority over the `HttpClientRequestOptionsAttribute`.

#### HttpClientRequestOptionsAttribute

The `HttpClientRequestOptionsAttribute` class is an attribute used to configure options for HTTP client requests. It should decorate implementations of `IHttpClientRequest`, `IHttpClientAsyncRequest<TResponse>`, or `IHttpClientRequest<TResponse>` to be used with `IHttpClientDispatcher`.

### Usage

#### Creating and Sending a Simple Request

To create and send a simple request using `IHttpClientDispatcher`, you can define a request class and decorate it with `HttpClientRequestOptionsAttribute`.

```csharp

using System.Net; 
using Xpandables.Net.Http;
[HttpClientRequestOptions(Path = "/api/data", 
    Method = Method.GET)] 
public class GetDataRequest : IHttpClientRequest { }

public class SampleUsage 
{ 
    private readonly IHttpClientDispatcher _dispatcher;
    public SampleUsage(IHttpClientDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task SendRequestAsync()
    {
        var request = new GetDataRequest();
        var response = await _dispatcher.SendAsync(request);
    
        if (response.IsValid)
        {
            Console.WriteLine("Request was successful.");
        }
        else
        {
            Console.WriteLine("Request failed.");
        }
    }
}

```


#### Creating and Sending a Request with a Response

To create and send a request that returns a response of a specific type, you can define a request class and a response class.

```csharp

using System.Net; 
using Xpandables.Net.Http;

[HttpClientRequestOptions(Path = "/api/data", 
Method = Method.GET)] 
public class GetDataRequest : IHttpClientRequest<string> { }

public class SampleUsage 
{ 
    private readonly IHttpClientDispatcher _dispatcher;
    public SampleUsage(IHttpClientDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task SendRequestWithResponseAsync()
    {
        var request = new GetDataRequest();
        var response = await _dispatcher.SendAsync(request);
    
        if (response.IsValid)
        {
            Console.WriteLine($"Response data: {response.Result}");
        }
        else
        {
            Console.WriteLine("Request failed.");
        }
    }
}

```


#### Using a Custom Request Options Builder

To use a custom request options builder, implement the `IHttpClientRequestOptionsBuilder` interface in your request class.

```csharp

using System.Net; 
using Xpandables.Net.Http;

public class CustomRequestOptionsBuilder : IHttpClientRequestOptionsBuilder 
{ 
    public HttpClientRequestOptionsAttribute Build(HttpClientOptions options) 
    { 
        return new HttpClientRequestOptionsAttribute 
            { 
                Path = "/api/custom", 
                Method = Method.POST, 
                ContentType = "application/json" 
            }; 
    } 
}

public class CustomRequest : CustomRequestOptionsBuilder;

public class SampleUsage 
{ 
    private readonly IHttpClientDispatcher _dispatcher;
    public SampleUsage(IHttpClientDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task SendCustomRequestAsync()
    {
        var request = new CustomRequest();
        var response = await _dispatcher.SendAsync(request);
    
        if (response.IsValid)
        {
            Console.WriteLine("Custom request was successful.");
        }
        else
        {
            Console.WriteLine("Custom request failed.");
        }
    }
}

```


The `IHttpClientDispatcher` interface and related classes provide a flexible and structured way to handle HTTP client requests and responses in your application. By using these classes, you can ensure that your HTTP operations are consistent and detailed, making it easier to handle various HTTP scenarios.
