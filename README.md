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
## 🚀 Mediator Pattern & Pipeline Architecture

The Xpandables.Net Mediator implementation provides a robust, high-performance messaging pattern that eliminates reflection, ensures compile-time type safety, and supports rich pipeline processing. Built on the foundation of ExecutionResult, it delivers consistent error handling across your application.

### ✨ Key Features

- 🚫 **No Reflection** - Direct generic method resolution for maximum performance
- ✅ **Compile-Time Safety** - Type checking ensures correct handler registration
- 🔄 **Single Method Interface** - One `SendAsync` method handles all request types
- 🛠️ **Rich Pipeline Support** - Composable decorators for cross-cutting concerns
- 🎯 **ExecutionResult Integration** - Seamless result handling and HTTP mapping
- ⚡ **High Performance** - Optimized for production workloads
- 🔧 **Extensible Architecture** - Easy to customize and extend

### 🎨 Benefits

**🚀 Performance Excellence**
- Zero reflection overhead during request processing
- Direct method resolution with generic constraints
- Minimal memory allocations and garbage collection pressure

**🛡️ Type Safety**
- Compile-time verification of handler registrations
- Generic constraints prevent runtime type errors
- Strongly-typed request and response handling

**🏗️ Clean Architecture**
- Clear separation of concerns with pipeline decorators
- Consistent error handling across all operations
- Testable and maintainable code structure

**🔄 Pipeline Power**
- Composable decorators for cross-cutting concerns
- Configurable execution order for fine-grained control
- Built-in decorators for common scenarios (validation, transactions, logging)

### 🏛️ Core Interfaces

#### IMediator
````````csharp
public interface IMediator 
{ 
    Task<ExecutionResult> SendAsync<TRequest>(
        TRequest request, 
        CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}
````````

#### IRequest
```csharp
public interface IRequest
{
    DateTime CreatedAt => DateTime.Now;
}
```
#### IRequestHandler
```csharp
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task<ExecutionResult> HandleAsync(
    TRequest request, CancellationToken cancellationToken);
}
```

#### IPipelineDecorator
```csharp
public interface IPipelineDecorator<TRequest> where TRequest : class, IRequest
{
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken);
}
```


### 🔧 Service Registration

#### For Web APIs (Controllers)
```csharp
using Xpandables.Net.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Register core services
builder.Services.AddControllers();

// Register mediator with all default pipeline decorators
builder.Services
    .AddXMediator()
    .AddXHandlers(typeof(Program).Assembly);

// Optional: Add additional pipeline decorators
builder.Services
    .AddXPipelineDecorator(typeof(CustomLoggingDecorator<>));

var app = builder.Build();
app.MapControllers();
app.Run();
```

#### For Minimal APIs
```csharp
using Xpandables.Net.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Register mediator and minimal API support
builder.Services
    .AddXMediator()
    .AddXHandlers(typeof(Program).Assembly)
    .AddXMinimalApi();

var app = builder.Build();

// Use minimal API extensions
app.UseXMinimalMiddleware();

// Map endpoints with pipeline integration
app.MapPost("/api/users", async (CreateUserRequest request, IMediator mediator) =>
{
    return await mediator.SendAsync(request);
})
.WithXMinimalApi(); // Applies validation and result conversion

app.Run();
```

### 🏗️ Basic Usage Examples

#### Simple Request/Response
```csharp
// Define a request
public sealed record GetUserRequest(int UserId) : IRequest;

// Implement the handler
public sealed class GetUserHandler : IRequestHandler<GetUserRequest>
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ExecutionResult> HandleAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    
        if (user == null)
        {
            return ExecutionResult.NotFound()
                .WithTitle("User Not Found")
                .WithDetail($"No user found with ID {request.UserId}")
                .Build();
        }

        return ExecutionResult.Ok()
            .WithResult(user)
            .WithHeader("X-User-Found", DateTime.UtcNow.ToString())
            .Build();
    }
}

// Use in controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var request = new GetUserRequest(id);
        var result = await _mediator.SendAsync(request);
    
        return result; // Automatically converts ExecutionResult to IActionResult
    }
}
```

#### Command with Validation
```csharp
// Request with validation marker interface
public sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName) : IRequest, IRequiresValidation;

// Custom validator
public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public override ValueTask<ExecutionResult> ValidateAsync(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(request.Email))
            errors.Add("Email format is invalid");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("First name is required");

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("Last name is required");

        if (errors.Any())
        {
            var result = ExecutionResult.BadRequest()
                .WithTitle("Validation Failed")
                .WithErrors(errors.ToDictionary((e, i) => $"error_{i}", e => e))
                .Build();
            return ValueTask.FromResult(result);
        }

        return ValueTask.FromResult(ExecutionResult.Success());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// Handler implementation
public sealed class CreateUserHandler : IRequestHandler<CreateUserRequest>
{
    private readonly IUserRepository _userRepository;

    public CreateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ExecutionResult> HandleAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userRepository
            .GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return ExecutionResult.Conflict()
                .WithTitle("User Already Exists")
                .WithDetail($"A user with email {request.Email} already exists")
                .WithError("email", "Email address is already in use")
                .Build();
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return ExecutionResult.Created()
            .WithResult(user)
            .WithLocation($"/api/users/{user.Id}")
            .WithTitle("User Created Successfully")
            .Build();
    }
}

// Registration
builder.Services.AddXValidators();
```

### 🛠️ Built-in Pipeline Decorators

#### 1. Validation Decorator
Automatically validates requests that implement `IRequiresValidation`:

```csharp
public sealed record UpdateUserRequest( int UserId, string Email) : IRequest, IRequiresValidation;

// Validation happens automatically in the pipeline
// Handler only receives validated requests
public sealed class UpdateUserHandler : IRequestHandler<UpdateUserRequest>
{
    public async Task<ExecutionResult> HandleAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Request is guaranteed to be valid when handler is reached
        // Implementation here...
        return ExecutionResult.Success();
    }
}
```

#### 2. Unit of Work Decorator
Automatically manages database transactions for requests implementing `IRequiresUnitOfWork`:
```csharp
public sealed record CreateOrderRequest( int CustomerId, List<OrderItem> Items) : IRequest, IRequiresUnitOfWork;

// Transaction management is handled automatically
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderRequest>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryService _inventoryService;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IInventoryService inventoryService)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
    }

    public async Task<ExecutionResult> HandleAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Multiple repository operations within a single transaction
        var order = new Order(request.CustomerId, request.Items);
    
        await _orderRepository.AddAsync(order, cancellationToken);
        await _inventoryService.ReserveItemsAsync(request.Items, cancellationToken);
    
        // SaveChanges called automatically by UnitOfWork decorator
        return ExecutionResult.Created(order);
    }
}
```

#### 3. Exception Decorator
Converts unhandled exceptions to ExecutionResults:
```csharp
public sealed class GetUserHandler : IRequestHandler<GetUserRequest>
{
    public async Task<ExecutionResult> HandleAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Any unhandled exception is automatically converted to ExecutionResult
        var data = await SomeExternalServiceCall(request.UserId);
        // If this throws, it becomes an ExecutionResult with InternalServerError status
        return ExecutionResult.Success(data);
    }
}
```

### 🎯 Custom Pipeline Decorators

Create custom decorators for specific cross-cutting concerns:

#### Logging Decorator
```csharp
public sealed class LoggingDecorator<TRequest> : IPipelineDecorator<TRequest> where TRequest : class, IRequest
{
    private readonly ILogger<LoggingDecorator<TRequest>> _logger;

    public LoggingDecorator(ILogger<LoggingDecorator<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Executing request {RequestName} at {Timestamp}",
            requestName, DateTime.UtcNow);

        try
        {
            var result = await next(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Completed request {RequestName} in {ElapsedMilliseconds}ms with status {StatusCode}",
                requestName, stopwatch.ElapsedMilliseconds, result.StatusCode);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Request {RequestName} failed after {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
// Register the custom decorator 
builder.Services.AddXPipelineDecorator(typeof(LoggingDecorator<>));

```

#### Caching Decorator
```csharp
public sealed class CachingDecorator<TRequest> : IPipelineDecorator<TRequest> where TRequest : class, IRequest, ICacheableRequest
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingDecorator<TRequest>> _logger;

    public CachingDecorator(IMemoryCache cache, ILogger<CachingDecorator<TRequest>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        var cacheKey = context.Request.GetCacheKey();

        if (_cache.TryGetValue(cacheKey, out ExecutionResult cachedResult))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResult;
        }

        var result = await next(cancellationToken).ConfigureAwait(false);

        if (result.IsSuccessStatusCode)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = context.Request.CacheExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, result, cacheOptions);
            _logger.LogDebug("Cached result for key: {CacheKey}", cacheKey);
        }

        return result;
    }
}

// Marker interface for cacheable requests
public interface ICacheableRequest
{
    string GetCacheKey();
    TimeSpan CacheExpiration { get; }
}

// Usage
public sealed record GetUserRequest(int UserId) : IRequest, ICacheableRequest
{
    public string GetCacheKey() => $"user_{UserId}";
    public TimeSpan CacheExpiration => TimeSpan.FromMinutes(5);
};
```


### 🔄 Custom Pipeline Request Handler

Create a custom orchestrator for complex request processing:
```csharp
public sealed class CustomPipelineRequestHandler<TRequest> : IPipelineRequestHandler<TRequest> 
    where TRequest : class, IRequest 
{
    private readonly IRequestHandler<TRequest> _handler; 
    private readonly IServiceProvider _serviceProvider; 
    private readonly ILogger<CustomPipelineRequestHandler<TRequest>> _logger;

    public CustomPipelineRequestHandler(
        IRequestHandler<TRequest> handler,
        IServiceProvider serviceProvider,
        ILogger<CustomPipelineRequestHandler<TRequest>> logger)
    {
        _handler = handler;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ExecutionResult> HandleAsync(
        TRequest request, 
        CancellationToken cancellationToken = default)
    {
        var context = new RequestContext<TRequest>(request);
    
        // Custom pre-processing logic
        var preProcessResult = await PreProcessAsync(context, cancellationToken);
        if (!preProcessResult.IsSuccessStatusCode)
        {
            return preProcessResult;
        }

        try
        {
            // Execute the main handler
            var result = await _handler.HandleAsync(request, cancellationToken);
        
            // Custom post-processing logic
            await PostProcessAsync(context, result, cancellationToken);
        
            return result;
        }
        catch (Exception ex)
        {
            // Custom exception handling
            return await HandleExceptionAsync(context, ex, cancellationToken);
        }
    }

    private async Task<ExecutionResult> PreProcessAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken)
    {
        // Custom validation, authorization, etc.
        _logger.LogDebug("Pre-processing request {RequestType}", typeof(TRequest).Name);
    
        // Example: Custom authorization check
        if (context.Request is IRequiresAuthorization authRequest)
        {
            var authService = _serviceProvider.GetRequiredService<IAuthorizationService>();
            if (!await authService.IsAuthorizedAsync(authRequest, cancellationToken))
            {
                return ExecutionResult.Unauthorized()
                    .WithTitle("Access Denied")
                    .WithDetail("You don't have permission to perform this action")
                    .Build();
            }
        }

        return ExecutionResult.Success();
    }

    private async Task PostProcessAsync(
        RequestContext<TRequest> context,
        ExecutionResult result,
        CancellationToken cancellationToken)
    {
        // Custom audit logging, notifications, etc.
        _logger.LogDebug("Post-processing request {RequestType}", typeof(TRequest).Name);
    
        if (result.IsSuccessStatusCode && context.Request is IAuditableRequest)
        {
            var auditService = _serviceProvider.GetRequiredService<IAuditService>();
            await auditService.LogActionAsync(context.Request, result, cancellationToken);
        }
    }

    private async Task<ExecutionResult> HandleExceptionAsync(
        RequestContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error processing request {RequestType}", typeof(TRequest).Name);
    
        // Custom exception handling logic
        return exception switch
        {
            UnauthorizedAccessException => ExecutionResult.Unauthorized()
                .WithTitle("Access Denied")
                .WithException(exception)
                .Build(),
            ValidationException validationEx => ExecutionResult.BadRequest()
                .WithTitle("Validation Error")
                .WithException(validationEx)
                .Build(),
            _ => ExecutionResult.InternalServerError()
                .WithTitle("Internal Server Error")
                .WithException(exception)
                .Build()
        };
    }
}
// Register the custom pipeline handler
builder.Services.AddXPipelineRequestHandler(typeof(CustomPipelineRequestHandler<>));
```

### 🌐 API Integration Examples

#### Controller Integration
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var result = await _mediator.SendAsync(request);
    
        // ExecutionResult automatically maps to appropriate HTTP responses
        return result;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var request = new GetOrderRequest(id);
        var result = await _mediator.SendAsync(request);
    
        return result;
    }
}
```

#### Minimal API Integration
```csharp
using Xpandables.Net.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddXMediator()
    .AddXHandlers(typeof(Program).Assembly)
    .AddXMinimalApi();

var app = builder.Build();

// Orders group with automatic validation and result conversion
var ordersGroup = app
    .MapGroup("/api/orders")
    .WithXMinimalApi(); // Applies validation and ExecutionResult conversion

ordersGroup.MapPost("/", 
    async (CreateOrderRequest request, IMediator mediator) => 
    { 
        return await mediator.SendAsync(request); 
    });

ordersGroup.MapGet("/{id}", 
    async (int id, IMediator mediator) => 
{
    var request = new GetOrderRequest(id);
    return await mediator.SendAsync(request);
});

ordersGroup.MapPut("/{id}", 
    async (int id, UpdateOrderRequest request, IMediator mediator) => 
{
    request = request with { OrderId = id };
    return await mediator.SendAsync(request);
});

app.Run();
```


### 🧪 Testing Examples

#### Unit Testing Request Handlers
```csharp
[Test]
public async Task CreateUserHandler_WithValidRequest_ReturnsCreatedResult()
{
    // Arrange
    var userRepository = new Mock<IUserRepository>();
    userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
        .ReturnsAsync((User)null);
    var handler = new CreateUserHandler(userRepository.Object);
    var request = new CreateUserRequest("test@example.com", "John", "Doe");

    // Act
    var result = await handler.HandleAsync(request);

    // Assert
    result.IsSuccessStatusCode.Should().BeTrue();
    result.StatusCode.Should().Be(HttpStatusCode.Created);
    result.Value.Should().NotBeNull();
    userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
}

[Test]
public async Task CreateUserHandler_WithExistingEmail_ReturnsConflictResult()
{
    // Arrange
    var existingUser = new User { Email = "test@example.com" };
    var userRepository = new Mock<IUserRepository>();
    userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);
    var handler = new CreateUserHandler(userRepository.Object);
    var request = new CreateUserRequest("test@example.com", "John", "Doe");

    // Act
    var result = await handler.HandleAsync(request);

    // Assert
    result.IsSuccessStatusCode.Should().BeFalse();
    result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    result.Errors.Should().ContainKey("email");
}
```

#### Integration Testing with Pipeline
```csharp
[Test]
public async Task Mediator_WithValidationPipeline_RejectsInvalidRequests()
{
    // Arrange
    var services = new ServiceCollection()
        .AddXMediator()
        .AddXHandlers(typeof(CreateUserHandler).Assembly)
        .AddXValidator<CreateUserValidator>()
        .BuildServiceProvider();

    var mediator = services.GetRequiredService<IMediator>();
    var invalidRequest = new CreateUserRequest("", "", ""); // Invalid data

    // Act
    var result = await mediator.SendAsync(invalidRequest);

    // Assert
    result.IsSuccessStatusCode.Should().BeFalse();
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    result.Errors.Should().NotBeEmpty();
    result.Title.Should().Be("Validation Failed");
}
```

### 📊 Performance Benefits

The Xpandables.Net Mediator provides significant performance advantages:

- **Zero Reflection**: Direct generic method resolution eliminates reflection overhead
- **Compile-Time Safety**: Type errors caught at build time, not runtime
- **Optimized Pipeline**: Efficient decorator chain execution with minimal allocations
- **Memory Efficient**: Reuses request contexts and minimizes object creation
- **Scalable Architecture**: Designed for high-throughput production applications

The Mediator pattern in Xpandables.Net provides a powerful, type-safe, and performant way to handle application requests while maintaining clean architecture principles and enabling rich pipeline processing for cross-cutting concerns.

---

## 📄 IAsyncPagedEnumerable - Intelligent Pagination System

The `IAsyncPagedEnumerable<T>` interface extends `IAsyncEnumerable<T>` to provide intelligent pagination capabilities with automatic metadata extraction, seamless ExecutionResult integration, and zero-configuration JSON serialization for APIs. This powerful abstraction automatically handles pagination concerns while maintaining streaming performance and memory efficiency.

### ✨ Key Features

- 🔍 **Automatic Pagination Detection** - Extracts Skip/Take from IQueryable expressions automatically
- 🚫 **Zero Configuration** - Works seamlessly with ExecutionResult, Mediator, and Repository patterns
- 🌐 **Seamless API Integration** - Automatic JSON serialization with comprehensive pagination metadata
- ⚡ **EF Core Optimized** - Intelligent database queries with smart count operations and memory management  
- 🔄 **Streaming Support** - Memory-efficient enumeration for large datasets with deferred execution
- 📊 **Rich Metadata** - Complete pagination information including page numbers, totals, and navigation flags
- 🎯 **Mediator Compatible** - Direct integration with SendAsync results and pipeline processing

### 🎨 Benefits

**⚡ Performance Excellence**
- Deferred execution with intelligent count optimization and caching
- Memory-efficient streaming prevents loading large result sets into memory
- Smart EF Core integration with optimized CountAsync usage
- Automatic cleanup prevents memory leaks in long-running applications

**🔄 Developer Experience** 
- Zero additional setup required for API pagination functionality
- Automatic JSON response formatting with comprehensive metadata
- Seamless integration with existing LINQ query patterns and Entity Framework
- Type-safe pagination information access with compile-time verification

**🌐 API-First Design**
- Automatic HTTP response formatting with pagination in body or headers
- Compatible with OpenAPI/Swagger documentation generation
- RESTful pagination standards compliance out of the box
- Configurable serialization strategies for different API requirements

### 🏛️ Core Interface & Types

```csharp
public interface IAsyncPagedEnumerable<out T> : IAsyncEnumerable<T> 
    where T : allows ref struct
{
    Pagination Pagination { get; }
    Task<Pagination> GetPaginationAsync();
}

public sealed record Pagination
{
    public int? Skip { get; init; }           // Items to skip from start
    public int? Take { get; init; }           // Items per page
    public long TotalCount { get; init; }     // Total items across all pages
    public int? PageNumber { get; }           // Current page number (1-based)
    public int? PageSize { get; }             // Items per page
    public int? TotalPages { get; }           // Total number of pages
    public bool HasPreviousPage { get; }      // Can navigate backwards
    public bool HasNextPage { get; }          // Can navigate forwards
    public bool IsPaginated { get; }          // Was pagination detected
}
```

### 🔧 Repository Integration

The Repository pattern automatically returns `IAsyncPagedEnumerable<T>` from `FetchAsync`, providing seamless pagination support:

```csharp
public interface IRepository : IAsyncDisposable
{
    IAsyncPagedEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default) 
        where TEntity : class, IEntity;
}
```

### 🏗️ Basic Usage Examples

#### Simple Paginated Query
```csharp
// Repository automatically detects pagination from Skip/Take
public class UserService
{
    private readonly IRepository _repository;

    public UserService(IRepository repository)
    {
        _repository = repository;
    }

    public IAsyncPagedEnumerable<UserDto> GetUsersAsync(int page, int pageSize)
    {
        return _repository.FetchAsync<User, UserDto>(query => 
            query
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email))
                .Skip((page - 1) * pageSize)  // Automatically detected
                .Take(pageSize)               // Automatically detected
        );
    }
}

// Request/Response pattern with Mediator integration
public sealed record GetUsersQuery(int Page, int PageSize) : IRequest;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery>
{
    private readonly IRepository _repository;

    public GetUsersHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExecutionResult> HandleAsync(
        GetUsersQuery request,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
        {
            return ExecutionResult.BadRequest()
                .WithTitle("Invalid Pagination Parameters")
                .WithDetail("Page must be >= 1 and PageSize must be between 1 and 100")
                .Build();
        }

        var users = _repository.FetchAsync<User, UserDto>(query =>
            query
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email))
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
        );
    
        // IAsyncPagedEnumerable automatically integrates with ExecutionResult
        return ExecutionResult.Ok(users)
            .WithHeader("X-Data-Source", "Database")
            .WithHeader("X-Query-Time", DateTime.UtcNow.ToString("O"))
            .Build();
    }
}
```

#### Advanced Query with Search and Filtering
```csharp
public sealed record SearchProductsQuery(
    string? SearchTerm, 
    ProductCategory? Category, 
    decimal? MinPrice, 
    decimal? MaxPrice, 
    string? SortBy, 
    bool SortDescending, 
    int Page, 
    int PageSize) : IRequest;

public sealed class SearchProductsHandler : IRequestHandler<SearchProductsQuery>
{
    private readonly IRepository _repository;

    public SearchProductsHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExecutionResult> HandleAsync(
        SearchProductsQuery request,
        CancellationToken cancellationToken = default)
    {
        var products = _repository.FetchAsync<Product, ProductSearchResultDto>(query =>
        {
            // Start with base query
            var filteredQuery = query.Where(p => p.IsActive && !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                filteredQuery = filteredQuery.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower) ||
                    p.Tags.Any(t => t.Name.ToLower().Contains(searchLower)));
            }

            // Apply category filter
            if (request.Category.HasValue)
            {
                filteredQuery = filteredQuery.Where(p => p.Category == request.Category.Value);
            }

            // Apply price range filters
            if (request.MinPrice.HasValue)
            {
                filteredQuery = filteredQuery.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                filteredQuery = filteredQuery.Where(p => p.Price <= request.MaxPrice.Value);
            }

            // Apply sorting
            filteredQuery = request.SortBy?.ToLowerInvariant() switch
            {
                "name" => request.SortDescending 
                    ? filteredQuery.OrderByDescending(p => p.Name)
                    : filteredQuery.OrderBy(p => p.Name),
                "price" => request.SortDescending
                    ? filteredQuery.OrderByDescending(p => p.Price) 
                    : filteredQuery.OrderBy(p => p.Price),
                "category" => request.SortDescending
                    ? filteredQuery.OrderByDescending(p => p.Category)
                    : filteredQuery.OrderBy(p => p.Category),
                "created" => request.SortDescending
                    ? filteredQuery.OrderByDescending(p => p.CreatedAt)
                    : filteredQuery.OrderBy(p => p.CreatedAt),
                _ => filteredQuery.OrderBy(p => p.Name) // Default sort
        };

        // Project to DTO and apply pagination
        return filteredQuery
            .Select(p => new ProductSearchResultDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.ImageUrl,
                p.IsInStock,
                p.StockQuantity,
                p.Rating,
                p.ReviewCount))
            .Skip((request.Page - 1) * request.PageSize) // Automatically detected
            .Take(request.PageSize);                      // Automatically detected
        });

        return ExecutionResult.Ok(products)
            .WithExtension("searchTerm", request.SearchTerm ?? "")
            .WithExtension("totalFilters", CountActiveFilters(request).ToString())
            .Build();
    }

    private static int CountActiveFilters(SearchProductsQuery request)
    {
        int count = 0;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) count++;
        if (request.Category.HasValue) count++;
        if (request.MinPrice.HasValue) count++;
        if (request.MaxPrice.HasValue) count++;
        return count;
    }
}
```

### 🌐 API Integration - Automatic JSON Serialization

#### Minimal API - Zero Configuration Required
```csharp
using Xpandables.Net.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register services with automatic pagination support 
builder.Services
    .AddXMediator()
    .AddXHandlers(typeof(Program).Assembly)
    .AddXMinimalApi()  // Includes IAsyncPagedEnumerable support 
    .AddXDataContext<AppDbContext>(options => 
        options.UseSqlServer(
            builder.Configuration.GetConnectionString
            ("DefaultConnection")))
    .AddXRepositoryDefault<AppDbContext>();

var app = builder.Build();

// Product search endpoint with automatic pagination 
app.MapGet("/api/products", 
    async (
        [FromQuery] string? search, 
        [FromQuery] ProductCategory? category, 
        [FromQuery] decimal? minPrice, 
        [FromQuery] decimal? maxPrice, 
        [FromQuery] string sortBy = "name", 
        [FromQuery] bool sortDesc = false, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        IMediator mediator) => 
    {
        var query = new SearchProductsQuery( search, category, minPrice, maxPrice, sortBy, sortDesc, page, pageSize);
        return await mediator.SendAsync(query);
    })
    .WithXMinimalApi()  // Applies validation and automatic result conversion
    .WithName("SearchProducts")
    .WithOpenApi(operation => new(operation) 
    { 
        Summary = "Search products with pagination", 
        Description = "Search and filter products with automatic pagination metadata",
        Tags = new[] { new OpenApiTag { Name = "Products" } } 
    });

    // User management endpoint
    app.MapGet("/api/users", async (
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        IMediator mediator) =>
    {
        var query = new GetUsersQuery(page, pageSize);
        return await mediator.SendAsync(query);
    })
    .WithXMinimalApi()
    .WithName("GetUsers")
    .ProducesOK<IAsyncPagedEnumerable<UserDto>>();

    app.Run();

```

**Automatic JSON Response Format:**
```json
{ 
    "pagination": { 
        "skip": 20, 
        "take": 20, 
        "totalCount": 1247, 
        "pageNumber": 2, 
        "pageSize": 20, 
        "totalPages": 63, 
        "hasPreviousPage": true, 
        "hasNextPage": true, 
        "isPaginated": true 
    }, 
    "data": [ 
    { 
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890", 
        "name": "Premium Wireless Headphones", 
        "description": "High-quality noise-cancelling wireless headphones", 
        "price": 299.99, 
        "category": "Electronics", 
        "imageUrl": "/images/headphones-premium.jpg", 
        "isInStock": true, 
        "stockQuantity": 45, 
        "rating": 4.7, 
        "reviewCount": 328 
    } // ... more products ] }
```

### 🔄 Extension Methods for Advanced Scenarios

#### Direct IQueryable Integration
```csharp
public class AnalyticsService 
{ 
    private readonly AppDbContext _dbContext; 
    private readonly IMemoryCache _cache;

    public AnalyticsService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public IAsyncPagedEnumerable<SalesReportDto> GetSalesReportAsync(
        DateOnly startDate,
        DateOnly endDate,
        int page,
        int pageSize)
    {
        var cacheKey = $"sales_report_{startDate}_{endDate}_{page}_{pageSize}";
    
        return _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderDate >= startDate.ToDateTime(TimeOnly.MinValue) &&
                       o.OrderDate <= endDate.ToDateTime(TimeOnly.MaxValue) &&
                       o.Status == OrderStatus.Completed)
            .GroupBy(o => new { 
                Date = DateOnly.FromDateTime(o.OrderDate),
                ProductId = o.OrderItems.First().ProductId,
                ProductName = o.OrderItems.First().Product.Name 
            })
            .Select(g => new SalesReportDto(
                g.Key.Date,
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity)),
                g.Sum(o => o.TotalAmount),
                g.Count()))
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.TotalRevenue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .WithPagination(); // Extension method - automatic detection and optimization
    }

    public IAsyncPagedEnumerable<ProductPerformanceDto> GetTopProductsAsync(
        int take = 50)
    {
        return _dbContext.Products
            .AsNoTracking()
            .Include(p => p.OrderItems)
            .Where(p => p.IsActive)
            .Select(p => new ProductPerformanceDto(
                p.Id,
                p.Name,
                p.OrderItems.Sum(oi => oi.Quantity),
                p.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice),
                p.OrderItems.Count(),
                p.Rating ?? 0))
            .OrderByDescending(p => p.TotalRevenue)
            .Take(take)
            .WithPagination(); // No Skip/Take = returns all matching with count
    }
}
```

### 📈 Advanced Features & Performance

#### Memory-Efficient Streaming for Large Datasets
```csharp
public class ReportingService 
{ 
    private readonly IRepository _repository; 
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(IRepository repository, ILogger<ReportingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public IAsyncPagedEnumerable<TransactionReportDto> GetLargeTransactionReportAsync(
        DateOnly startDate,
        DateOnly endDate)
    {
        _logger.LogInformation(
            "Generating transaction report for period {StartDate} to {EndDate}", 
            startDate, endDate);

        // Stream large datasets efficiently without loading all into memory
        return _repository.FetchAsync<Transaction, TransactionReportDto>(query =>
            query
                .Where(t => t.TransactionDate >= startDate.ToDateTime(TimeOnly.MinValue) &&
                           t.TransactionDate <= endDate.ToDateTime(TimeOnly.MaxValue))
                .Include(t => t.Customer)
                .Include(t => t.Product)
                .OrderBy(t => t.TransactionDate)
                .Select(t => new TransactionReportDto(
                    t.Id,
                    t.TransactionDate,
                    t.Customer.Name,
                    t.Product.Name,
                    t.Amount,
                    t.Quantity,
                    t.Status))
                // No Skip/Take = streams all results with automatic count
        );
    }
}
// Usage in endpoint that handles potentially millions of records app.MapGet("/api/reports/transactions", async ( [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, IMediator mediator) => { var query = new GetTransactionReportQuery(startDate, endDate); return await mediator.SendAsync(query); }) .WithXMinimalApi() .WithName("GetTransactionReport") .WithDescription("Streams large transaction datasets efficiently");
```

### 🔧 Performance Optimizations

The `IAsyncPagedEnumerable<T>` implementation includes several advanced performance features:

- **Smart Count Optimization**: Automatically uses Entity Framework's `CountAsync` when available, falls back to efficient synchronous counting for LINQ-to-Objects
- **Memory-Aware Caching**: Intelligent caching of pagination metadata with automatic cleanup to prevent memory leaks
- **Deferred Execution**: Query execution is deferred until enumeration begins, allowing for further query composition
- **Expression Tree Analysis**: Efficiently extracts Skip/Take values from LINQ expressions without performance overhead
- **Streaming Architecture**: Large result sets are processed in streaming fashion to maintain low memory footprint
- **Connection Pool Friendly**: Designed to work efficiently with Entity Framework connection pooling

### 🌟 Key Integration Points

1. **Repository Pattern**: Automatic return of `IAsyncPagedEnumerable<T>` from `FetchAsync` methods
2. **Mediator Integration**: Direct support in request handlers with `ExecutionResult` wrapping
3. **API Serialization**: Automatic JSON conversion in both Minimal APIs and MVC Controllers
4. **Entity Framework**: Optimized integration with EF Core for efficient database operations
5. **Caching Support**: Built-in support for various caching strategies (in-memory, distributed)
6. **Testing Framework**: Comprehensive support for unit and integration testing scenarios

The `IAsyncPagedEnumerable<T>` interface represents a powerful evolution of pagination in .NET applications, providing developer-friendly APIs, automatic optimizations, and seamless integration across the entire application stack from database queries to JSON API responses.

---
*Xpandables.Net - Building better .NET applications with proven patterns and practices.*
