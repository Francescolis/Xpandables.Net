# Xpandables.Net Libraries

A comprehensive suite of .NET 9.0 utility libraries that provide modern patterns and practices for building robust, maintainable applications. The Xpandables.Net ecosystem consists of three complementary libraries that work together to offer a complete development experience.

## 📚 Libraries Overview

### 🔧 Xpandables.Net (Core)
The foundational library that provides essential patterns and utilities including:
- **ExecutionResult Pattern** - Type-safe result handling with fluent API
- **CQRS & Mediator Pattern** - Command/Query separation with pipeline support  
- **Pipeline Architecture** - Decoratable request/response processing
- **Repository Pattern** - Generic data access abstractions
- **Event Sourcing** - Domain event handling and state management
- **State Pattern** - Memento-based state management
- **Dependency Injection Extensions** - Streamlined service registration

### 🏛️ Xpandables.Net.EntityFramework
Entity Framework Core integration that extends the core library with:
- **EF Core Repository Implementation** - Concrete data access with Entity Framework
- **Unit of Work Pattern** - Transaction management and change tracking
- **DataContext Extensions** - Enhanced DbContext functionality
- **Aggregate Store** - Domain-driven design support for aggregates
- **Event Sourcing Storage** - Persistent event store implementation

### 🌐 Xpandables.Net.AspNetCore
ASP.NET Core integration providing:
- **Minimal API Extensions** - ExecutionResult integration with HTTP responses
- **Dependency Injection Extensions** - Service registration helpers
- **HTTP Result Mapping** - Automatic status code conversion
- **JSON Serialization** - Custom converters for ExecutionResult types

## 🚀 Installation

Install the libraries via NuGet Package Manager:

### Package Manager Console
````````
Install-Package Xpandables.Net
Install-Package Xpandables.Net.EntityFramework
Install-Package Xpandables.Net.AspNetCore

### .NET CLI
````````

### PackageReference
````````
<PackageReference Include="Xpandables.Net" Version="9.4.6.0" />
<PackageReference Include="Xpandables.Net.EntityFramework" Version="9.4.6.0" />
<PackageReference Include="Xpandables.Net.AspNetCore" Version="9.4.6.0" />
````````

## 📋 Requirements

- **.NET 9.0** or later
- **Entity Framework Core 9.0.8** (for EntityFramework package)
- **ASP.NET Core 9.0** (for AspNetCore package)

## 📄 License

This project is licensed under the **Apache License 2.0** - see the [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📚 Documentation

For more detailed documentation and advanced usage examples, visit the [GitHub repository](https://github.com/Francescolis/Xpandables.Net).

## 🆕 Release Notes

**Version 9.4.6.0**
- Add RequestContext to pipeline
- Enhanced ExecutionResult serialization options
- Improved Entity Framework integration
- Performance optimizations

---

## 🎯 ExecutionResult Pattern

The `ExecutionResult` type is a robust implementation of the Result pattern that eliminates exceptions for expected failures and provides a consistent way to handle operation outcomes. It's designed to work seamlessly in both domain logic and API responses.

### ✨ Key Features

- 🏗️ **Builder Pattern** - Fluent, readable result construction
- 🌐 **HTTP Ready** - Built-in support for HTTP status codes and headers  
- 📄 **JSON Compatible** - Seamless serialization/deserialization
- 🚀 **ASP.NET Core Integration** - Automatic response conversion
- 🔒 **Type Safe** - Strongly-typed generic variants available
- 🛡️ **Exception Safe** - Convert exceptions to results automatically
- 📊 **Rich Metadata** - Headers, extensions, errors, and location support

### 🎨 Benefits

**✅ Explicit Error Handling**
- Forces callers to handle both success and failure cases
- Eliminates hidden exception paths
- Makes failure scenarios part of the method signature

**🔄 Composable Operations**  
- Chain operations without exception handling noise
- Railway-oriented programming support
- Clean separation of happy path from error handling

**🌐 HTTP-First Design**
- Direct mapping to HTTP status codes
- Built-in support for REST API patterns
- Consistent response format across endpoints

**⚡ Performance Benefits**
- No exception allocation overhead
- Reduced stack unwinding
- Better performance in failure scenarios

### 🏗️ Creating ExecutionResults

#### ✅ Success Results
```csharp
using Xpandables.Net;
using Xpandables.Net.Executions;

public class UserService 
{ 
    public ExecutionResult<User> GetUser(int id) 
    { 
        // Simple success with data 
        var user = new User { Id = id, Name = "John Doe" }; 
        return ExecutionResult.Success(user); 
    }
    public ExecutionResult CreateUser(User user)
    {
        // Success with custom status code and location
        return ExecutionResult.Created()
            .WithLocation($"/api/users/{user.Id}")
            .WithHeader("X-User-Created", DateTime.UtcNow.ToString())
            .Build();
    }

    public async Task<ExecutionResult<List<User>>> GetUsersAsync()
    {
        var users = await GetUsersFromDatabase();
    
        return ExecutionResult.Ok(users)
            .WithHeader("X-Total-Count", users.Count.ToString())
            .WithExtension("cached", "false")
            .Build();
    }
}
```

#### ❌ Failure Results

```csharp
public class UserService 
{ 
    public ExecutionResult<User> GetUser(int id) 
    { 
        if (id <= 0) 
        { 
            return ExecutionResult
                .BadRequest<User>()
                .WithTitle("Invalid User ID")
                .WithDetail("User ID must be greater than zero")
                .WithError("id", "Value must be positive")
                .Build();
        }
    public ExecutionResult DeleteUser(int id)
    {
        try
        {
            // Business logic here
            if (UserHasActiveOrders(id))
            {
                return ExecutionResult.Conflict()
                    .WithTitle("Cannot Delete User")
                    .WithDetail("User has active orders and cannot be deleted")
                    .WithError("activeOrders", "Complete or cancel orders before deletion")
                    .Build();
            }

            // Delete user logic
            return ExecutionResult.NoContent().Build();
        }
        catch (Exception ex)
        {
            return ExecutionResult.InternalServerError()
                .WithTitle("Deletion Failed")
                .WithException(ex)
                .Build();
        }
    }
}
```

#### 🎭 Variants
```csharp
var cpfResult = ExecutionResult.Success(cpfNumber);
var failureResult = ExecutionResult.Failure("Invalid CPF", "The provided CPF number is invalid.");
var notFoundResult = ExecutionResult.Failure("NotFound", "The requested resource was not found.");
```

### 🔄 Composing ExecutionResults

ExecutionResults can be composed using the `Bind` and `Map` methods, enabling clean and expressive pipelines:

```csharp
var result = await repository
    .GetDataAsync()
    .Bind(ValidateData)
    .Map(TransformData)
    .Execute();
```

#### 🔧 Advanced Builder Usage

For more complex scenarios, the `ExecutionResultBuilder` provides a fluent interface for configuring results:

```csharp
public class OrderService 
{ 
    public async Task<ExecutionResult<Order>> ProcessOrderAsync(CreateOrderRequest request) 
    { 
        // Validate request
        var validationResult = ValidateOrder(request); 
        if (!validationResult.IsSuccessStatusCode) 
        { 
            return validationResult.ToExecutionResult<Order>(); 
        }
        try
        {
            var order = await CreateOrderAsync(request);
        
            return ExecutionResult.Created(order)
                .WithLocation($"/api/orders/{order.Id}")
                .WithTitle("Order Created Successfully")
                .WithDetail($"Order {order.Id} has been created and is being processed")
                .WithHeader("X-Order-Id", order.Id.ToString())
                .WithHeader("X-Processing-Time", "< 1 minute")
                .WithExtension("estimatedDelivery", order.EstimatedDelivery.ToString())
                .WithExtension("trackingEnabled", "true")
                .Build();
        }
        catch (InsufficientInventoryException ex)
        {
            return ExecutionResult.Conflict<Order>()
                .WithTitle("Insufficient Inventory")
                .WithDetail("One or more items are out of stock")
                .WithErrors(ex.OutOfStockItems.ToDictionary(
                    item => item.Sku, 
                    item => $"Only {item.Available} available, requested {item.Requested}"))
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResult.InternalServerError<Order>()
                .WithTitle("Order Processing Failed")
                .WithException(ex)
                .Build();
        }

        private ExecutionResult ValidateOrder(CreateOrderRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (request.Items?.Any() != true)
                errors["items"] = "At least one item is required";

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
                errors["customerEmail"] = "Customer email is required";

            if (errors.Any())
            {
                return ExecutionResult.BadRequest()
                    .WithTitle("Invalid Order Request")
                    .WithErrors(errors)
                    .Build();
            }

            return ExecutionResult.Success();
        }
}
```

### 🔄 Working with Results

#### 🎯 Pattern Matching & Handling
````````csharp
public class OrderController : ControllerBase 
{
    private readonly OrderService _orderService;
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.ProcessOrderAsync(request);

        // Automatic conversion to HTTP response
        return result.IsSuccessStatusCode 
            ? Ok(result.Value)
            : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var result = await _orderService.GetOrderAsync(id);

        // Pattern matching approach
        return result.StatusCode switch
        {
            HttpStatusCode.OK => Ok(result.Value),
            HttpStatusCode.NotFound => NotFound(new { Message = result.Detail }),
            HttpStatusCode.Unauthorized => Unauthorized(),
            _ => StatusCode((int)result.StatusCode, result)
        };
    }
}
````````

#### 🔗 Chaining Operations
````````csharp
public class OrderWorkflow 
{ 
    public async Task<ExecutionResult<Order>> ProcessCompleteOrderAsync(int orderId)
    { 
        // Chain operations - each step can fail independently 
        var orderResult = await GetOrderAsync(orderId); 
        if (!orderResult.IsSuccessStatusCode) 
            return orderResult;
        var paymentResult = await ProcessPaymentAsync(orderResult.Value);
        if (!paymentResult.IsSuccessStatusCode)
            return paymentResult.ToExecutionResult<Order>();

        var inventoryResult = await ReserveInventoryAsync(orderResult.Value);
        if (!inventoryResult.IsSuccessStatusCode)
            return inventoryResult.ToExecutionResult<Order>();

        var shippingResult = await ScheduleShippingAsync(orderResult.Value);
        if (!shippingResult.IsSuccessStatusCode)
            return shippingResult.ToExecutionResult<Order>();

        // All steps succeeded
        return ExecutionResult.Success(orderResult.Value);
    }
}
````````

### 📄 JSON Serialization

ExecutionResult provides seamless JSON serialization for API responses:
````````csharp
public class ExecutionResultController : ControllerBase 
{ 
    [HttpGet("success-example")] 
    public ExecutionResult<ProductDto> GetProduct() 
    { 
        var product = new ProductDto 
        { 
            Id = 1, 
            Name = "Sample Product", 
            Price = 29.99m 
        };

        return ExecutionResult.Ok(product)
            .WithHeader("X-Cache-Status", "MISS")
            .Build();
    }

    [HttpGet("error-example")] 
    public ExecutionResult<ProductDto> GetProductError()
    {
        return ExecutionResult.NotFound<ProductDto>()
            .WithTitle("Product Not Found")
            .WithDetail("The requested product does not exist")
            .WithError("productId", "Invalid product identifier")
            .Build();
    }
}

// JSON Output for success: // { //   "statusCode": 200, //   "value": { //     "id": 1, //     "name": "Sample Product", //     "price": 29.99 //   }, //   "headers": { //     "X-Cache-Status": ["MISS"] //   } // }
// JSON Output for error: // { //   "statusCode": 404, //   "title": "Product Not Found", //   "detail": "The requested product does not exist", //   "errors": { //     "productId": ["Invalid product identifier"] //   } // }

````````

### 🧪 Unit Testing with ExecutionResult

ExecutionResult makes unit testing explicit and straightforward:
````````csharp
[Test]
public async Task ProcessOrder_WithValidRequest_ReturnsCreatedResult()
{
    // Arrange
    var request = new CreateOrderRequest
    {
        CustomerEmail = "test@example.com",
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 1, Quantity = 2 }
        }
    };

    // Act
    var result = await _orderService.ProcessOrderAsync(request);

    // Assert
    result.IsSuccessStatusCode.Should().BeTrue();
    result.StatusCode.Should().Be(HttpStatusCode.Created);
    result.Value.Should().NotBeNull();
    result.Value.CustomerEmail.Should().Be("test@example.com");
    result.Location.Should().NotBeNull();
    result.Headers.Should().ContainKey("X-Order-Id");
}

[Test] 
public async Task ProcessOrder_WithInvalidEmail_ReturnsBadRequest() 
{ 
    // Arrange 
    var request = new CreateOrderRequest { CustomerEmail = "" };

    // Act
    var result = await _orderService.ProcessOrderAsync(request);

    // Assert
    result.IsSuccessStatusCode.Should().BeFalse();
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    result.Title.Should().Be("Invalid Order Request");
    result.Errors.Should().ContainKey("customerEmail");
    result.Value.Should().BeNull();
}
````````

### 🔧 Exception Handling Integration

Convert exceptions to ExecutionResults automatically:
````````csharp
public class UserService
{
    public ExecutionResult<User> GetUser(int id)
    {
        try
        {
            // Your existing code that might throw
            var user = _repository.GetById(id);
            if (user == null)
                throw new UserNotFoundException($"User {id} not found");

            return ExecutionResult.Success(user);
        }
        catch (UserNotFoundException ex)
        {
            return ExecutionResult.NotFound<User>()
                .WithTitle("User Not Found")
                .WithDetail(ex.Message)
                .Build();
        }
        catch (DatabaseException ex) when (ex.IsTemporary)
        {
            return ExecutionResult.ServiceUnavailable<User>()
                .WithTitle("Service Temporarily Unavailable")
                .WithDetail("Please try again in a few moments")
                .WithException(ex)
                .Build();
        }
        catch (Exception ex)
        {
            // Automatic exception conversion
            return ex.ToExecutionResult<User>();
        }
}
````````
---

The ExecutionResult pattern provides a robust, type-safe, and HTTP-friendly way to handle operation outcomes in your applications. It promotes explicit error handling, improves API consistency, and integrates seamlessly with modern .NET development practices.

---


---

*Xpandables.Net - Building better .NET applications with proven patterns and practices.*