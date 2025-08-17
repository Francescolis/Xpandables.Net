# Xpandables.Net

**Provides useful interfaces and contracts in .NET 9.0** with implementations following SOLID principles, functional programming patterns, and modern web development practices. The library is strongly-typed and designed to eliminate invalid states while providing excellent IntelliSense support.

**✨ Key Features:**
- 🚀 **Zero Reflection** - All operations use compile-time type checking
- 📦 **Strongly Typed** - Catch errors at compile time, not runtime
- 🔄 **JSON Serializable** - Full serialization/deserialization support
- 🌐 **ASP.NET Core Ready** - Automatic response conversion
- 🏗️ **Builder Pattern** - Fluent and intuitive API design
- ⚡ **High Performance** - Minimal allocations and optimized execution

Feel free to fork this project, make your own changes and create a pull request.

This project is licensed under the Apache License, Version 2.0. See the [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) file for details.

## 📦 Installation

Install the required NuGet packages based on your needs:

### Core Library

```bash
dotnet add package Xpandables.Net
```

### For ASP.NET Core

```bash
dotnet add package Xpandables.Net.AspNetCore
```

### For HTTP Client Extensions

```bash
dotnet add package Xpandables.Net.Executions
```

### For Functional Programming Extensions

```bash
dotnet add package Xpandables.Net.Functional
```

### For Validation Extensions

```bash
dotnet add package Xpandables.Net.Validation
```

### For Unit Testing Extensions

```bash
dotnet add package Xpandables.Net.Testing
```

## Getting Started

---

## 🎯 ExecutionResult - Structured Result Handling

### Overview

The `ExecutionResult` and `ExecutionResult<TResult>` classes provide a robust, type-safe way to handle operation results in your applications. They encapsulate success and failure scenarios with detailed HTTP status codes, error information, headers, and metadata - **all without using reflection**.

**🔥 Key Benefits:**
- ✅ **Zero Reflection** - Compile-time type safety and optimal performance
- 🏗️ **Builder Pattern** - Fluent, readable result construction
- 🌐 **HTTP Ready** - Built-in support for HTTP status codes and headers
- 📄 **JSON Compatible** - Seamless serialization/deserialization
- 🚀 **ASP.NET Core Integration** - Automatic response conversion
- 🔒 **Type Safe** - Strongly-typed generic variants available

### 🏗️ Creating ExecutionResults

#### ✅ Success Results

```csharp
using System.Net;
using Xpandables.Net.Executions;

public class SampleUsage
{
    public ExecutionResult CreateSuccessResult()
    {
        return ExecutionResult.Success(HttpStatusCode.OK)
            .WithLocation(new Uri("http://example.com"))
            .Build();
    }

    public ExecutionResult<string> CreateSuccessResultWithData()
    {
        return ExecutionResult.Success("Success Data", HttpStatusCode.OK)
            .WithLocation(new Uri("http://example.com"))
            .Build();
    }
}
```

#### ❌ Failure Results

```csharp
using System.Net;
using Xpandables.Net.Executions;

public class SampleUsage
{
    public ExecutionResult CreateFailureResult()
    {
        return ExecutionResult.Failure(HttpStatusCode.BadRequest)
            .WithTitle("Execution Failed")
            .WithDetail("The execution failed due to bad request.")
            .WithError("ErrorKey", "ErrorMessage")
            .Build();
    }

    public ExecutionResult<string> CreateFailureResultWithData()
    {
        return ExecutionResult.Failure<string>(HttpStatusCode.BadRequest)
            .WithTitle("Execution Failed")
            .WithDetail("The execution failed due to bad request with data.")
            .WithError("ErrorKey", "ErrorMessage")
            .Build();
    }
}
```

#### ⚙️ Using Predefined Methods

The `ExecutionResult` class also provides predefined methods for common HTTP status codes like `Ok`, `Created`, `NoContent`, `NotFound`, `BadRequest`, `Conflict`, `Unauthorized`, `InternalServerError`, and `ServiceUnavailable`.

```csharp
using Xpandables.Net.Executions;

public class SampleUsage
{
    public ExecutionResult CreateOkResult()
    {
        return ExecutionResult.Ok()
            .Build();
    }

    public ExecutionResult<string> CreateNotFoundResult()
    {
        return ExecutionResult.NotFound<string>()
            .WithTitle("Resource Not Found")
            .WithDetail("The requested resource was not found.")
            .Build();
    }
}
```

The `ExecutionResult` and `ExecutionResult` classes provide a flexible and structured way to handle execution results in your application. By using these classes, you can ensure that your operations return consistent and detailed results, making it easier to handle both success and failure scenarios.

### IRestClient and Related Classes

#### Overview

The `IRestClient` interface and related classes in the `Xpandables.Net.Executions.Rests` namespace provide a structured way to handle HTTP client requests and responses. These classes and interfaces allow you to configure, send, and process HTTP requests with detailed options and builders.

##### IRestClient

The `IRestClient` interface provides methods to handle HTTP client requests using a typed client HTTP client. It supports sending requests that do not return a response, requests that return a response of a specific type, and requests that return a stream that can be async-enumerated.

##### IRestAttributeBuilder

The `IRestAttributeBuilder` interface defines a builder for creating `RestAttribute`. This interface takes priority over the `RestAttribute`.

##### RestAttribute

The `RestAttribute` class is an attribute used to configure options for HTTP client requests. It should decorate implementations of `IRestRequest`, `IRestRequest<TResponse>`, or `IRestRequestStream<TResponse>` to be used with `IRestClient`.

#### Usage

##### Creating and Sending a Simple Request

To create and send a simple request using `IRestClient`, you can define a request class and decorate it with `RestAttribute`.

```csharp
using System.Net; 
using Xpandables.Net.Executions.Rests;

[RestGet("/api/data")] 
public sealed record GetDataRequest : IRestString; // IRestString inherits IRestRequest


public class SampleUsage 
{ 
    private readonly IRestClient _restClient;
    public SampleUsage(IRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task SendRequestAsync()
    {
        var request = new GetDataRequest();
        RestResponse response = await _restClient.SendAsync(request);
    
        if (response.IsSuccess)
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


##### Creating and Sending a Request with a Response

To create and send a request that returns a response of a specific type, you can define a request class and a response class.

```csharp
using System.Net; 
using Xpandables.Net.Executions.Rests;

[RestGet("/api/data")] 
public sealed record GetDataRequest : IRestRequest<string>, IRestString;

public class SampleUsage 
{ 
    private readonly IRestClient _restClient;
    public SampleUsage(IRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task SendRequestWithResponseAsync()
    {
        var request = new GetDataRequest();
        RestResponse<string> response = await _restClient.SendAsync(request);
    
        if (response.IsSuccess)
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

To create and send a request that returns a response of stream type, you can define a request class and a response class.

```csharp
using System.Net; 
using Xpandables.Net.Executions.Rests;

public sealed record Result(string Data);

[RestGet("/api/data")] 
public sealed record GetDataRequest : IRestRequestStream<Result>, IRestString;

public class SampleUsage 
{ 
    private readonly IRestClient _restClient;
    public SampleUsage(IRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task SendRequestWithResponseAsync()
    {
        var request = new GetDataRequest();
        RestResponse<IAsyncEnumerable<Result>> response = await _restClient.SendAsync(request);
        // response will be of type IAsyncEnumerable<Result>
        // You can use response.Result to access the stream of results.

        if (response.IsSuccess)
        {
            // iterate over the stream
        }
        else
        {
            Console.WriteLine("Request failed.");
        }
    }
}
```

##### Using a Custom Request Options Builder

To use a custom request options builder, implement the `IRestAttributeBuilder` interface in your request class.

```csharp
using System.Net; 
using Xpandables.Net.Http;

public class CustomRequestAttributeBuilder : IRestAttributeBuilder 
{ 
    public RestAttribute Build(IServiceProvider serviceProvider) 
    { 
        return new RestAttribute 
            { 
                Path = "/api/custom", 
                Method = Method.POST, 
                ContentType = "application/json" 
            }; 
    } 
}

public sealed record CustomRequest : IRestRequest, CustomRequestAttributeBuilder;

public class SampleUsage 
{ 
    private readonly IRestClient _restClient;
    public SampleUsage(IRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task SendCustomRequestAsync()
    {
        var request = new CustomRequest();
        RestResponse response = await _restClient.SendAsync(request);
    
        if (response.IsSuccess)
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


The `IRestClient` interface and related classes provide a flexible and structured way to handle HTTP client requests and responses in your application. By using these classes, you can ensure that your HTTP operations are consistent and detailed, making it easier to handle various HTTP scenarios.

### Entity Framework Support

### 📄 JSON Serialization

ExecutionResult is fully JSON serializable without reflection:

### 🌐 ASP.NET Core Integration

ExecutionResult automatically converts to HTTP responses in ASP.NET Core applications.

#### Registration and Configuration

#### Minimal API Usage

#### Controller Usage

### 🔍 Advanced Usage

#### Merging ExecutionResults

#### Working with Headers and Extensions

#### Ensuring Success

The `ExecutionResult` classes provide a comprehensive, type-safe, and performance-optimized way to handle operation results in your .NET applications. With zero reflection usage and seamless ASP.NET Core integration, they offer a robust foundation for building reliable APIs and services.
