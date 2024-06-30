# Introduction 
Provides with useful interfaces contracts in **.Net 8.0** and some implementations mostly following the spirit of SOLID principles, Commands...
The library is strongly-typed, which means it should be hard to make invalid requests and it also makes it easy to discover available methods and properties though IntelliSense.

Feel free to fork this project, make your own changes and create a pull request.


# Getting Started

## Optional

The **Optional< T>** type allows to create a named value or a variable that contains a value or not.
An **Optional** has an underlying type and can hold a value of that type, or it might not have a value.

- Is a struct, immutable, a generic type, so it can hold a value of any type.
- Is a monad, implements **IEnumerable< T>**, so it can be used in a foreach loop.

Creating an Optional : Some or Empty

```csharp

var optional = Optional.Some("Hello World");
// optional : Optional<string> = [Some("Hello World")]

```

Apply a function to the value of an Optional : Map

```csharp

var optional = Optional.Some("Hello World");
var result = optional.Map(value => value.ToUpper());
// result : Optional<string> = [Some("HELLO WORLD")]

```

Change the type of an Optional : Bind

```csharp

var optional = Optional.Some("Hello World");
var result = optional.Bind(value => value.Length);
// result : Optional<int> = [Some(11)]

```

Change the return value of a function : Empty

```csharp

public string GetName()
{
    Optional<Name> optional = function call;
    return optional
        .Empty("No Name");

    // If the optional has a value, the function value will be returned.
    // If the optional is empty, the Empty value will be returned.
}

```

Apply serialization to an Optional : Serialize/Deserialize

```csharp

// value type
readonly record struct Name(string Value);

var optional = Optional.Some(new Name("Hello World"));
var result = JsonSerializer.Serialize(optional);
// result : Value = "Hello World"

// reference type
sealed record class Name(string Value);

var optional = Optional.Some(new Name("Hello World"));
var result = JsonSerializer.Serialize(optional);
// result : string = "{\"Value\":\"Hello World\"}"

// anonymous type
var optional = Optional.Some(new { Name = "Hello World" });
var result = JsonSerializer.Serialize(optional);
// result : string = "{\"Name\":\"Hello World\"}"

var deserialized = result.DeserializeAnonymousType(Optional.Some(new { Name = string.Empty }));
// or you can use the same anonymous variable
// var deserialized = result.DeserializeAnonymousType(optional);

// DeserializeAnonymousType is an extension method that allows to deserialize an anonymous type
// the method is defined in the Xpandables.Net.Extensions namespace

```

## IOperationResult

Allows to create methods that return the status of an execution.

>This interface contains all properties according to the result of the method execution.
Some of those properties let you determine for example if the result instance is generic, a collection of errors, 
the status code or the value of the execution.
The status code here is the one from the ***System.Net.HttpStatusCode***.
It contains methods to convert from non-generic type to generic and vis-versa.
The type is useful if you want to return a result that can be analyzed even in a web environment 
by using some extensions that can automatically convert an *OperationResult* to *IResult*.

The non generic type has the following properties :

- An optional *object* **Result**, a property that qualifies or contains information about an operation return if available. 
- An optiona *string* **LocationUrl**, a property that contains the URL mostly used with the status code **Created** in the web environment. 
- An *ElementCollection* **Headers** property that contains a collection of headers if available. *ElementCollection* is a predefined record struct that contains a collection of *ElementEntry* with useful methods.
- An *ElementCollection* **Errors** property that stores errors. Each error is a predefined *ElementEntry* struct which contains the error key and the error message and/or exceptions.
- A *HttpStatusCode* **StatusCode** property that contains the status code of the execution. The status code from the ***System.Net.HttpStatusCode***.
- A *boolean* **IsGeneric** to determine whether or not the current instance is generic.
- A *boolean* **IsSuccess** and **IsFailure** to determine whether or not the operation is a success or a failure according to ***System.Net.HttpStatusCode***.
- An optional *string* **Title** that contains the operation summary problem from the execution operation.
- An optional *string* **Detail** that contains he operation explanation specific to the execution operation.

The generic type overrides the *object* **Result** to *TResult* type.

Create a method that returns an **IOperationResult** :

```csharp

public IOperationResult CheckThatValueMatchCriteria(string? value)
{
    if(string.IsNullOrEmpty(value))
    {
        return OperationResults
            .BadRequest()
            .WithError(nameof(value), "value can not be null")
            .Build();
    }

    return OperationResults
        .Ok()
        .Build();
}

```

The method returns an IOperationResult* implementation type.
To do so, you can use the specific extension method according to your needs :

- **OperationResults** which is a factory to create specifics results from *Ok* to *InternalServerError*.
- build your owns using the **IOperationResult.ISuccessBuilder** or **IOperationResult.IFailureBuilder**

Each extension method allows you to add errors, headers, Uri or a value to the target operation result.
The key here in error can be the name of the member that has the error.
The caller of this method can check if the return operation is a success or a failure result.

>When used in an **Asp.Net Core** application, you will need to add the :
[Xpandables.Net.AspNetCore](https://www.nuget.org/packages/Xpandables.Net.AspNetCore)  NuGet package that will provides helpers to automatically manage *IResult* responses.
It also provides with a middleware that will automatically convert a failure *OperationResult* to *ValidationProblem*, *Problem* or result, according to the StatusCode.

```csharp
// Minimal Api
// You need to the following code in the Program.cs file

builder.Services
    .AddXOperationResultMiddleware()
    .AddXOperationResultResponseBuilder();
...
app.UseXOperationResultMiddleware();

app.MapGet("/api/users", (string name) =>
{
    if(CheckThatValueIsNotNull(name) is { IsFailure : true} failure)
        return failure;

    // ...get the user
	IOperationResult<User> resultUser = DoGetUser(...);
	
    // return the result
    return result;
})
.WithXOperationResultFilter()
.WithXValidatorFilter();

// WithXValidatorFilter is an extension method that allows to use the validator filter
// to automatically validate the request according to the specified type.
// and allows you to use custom validation implementation using **IValidator** interface.
// WithXOperationResultFilter is an extension method that allows to use the operation result filter
// to automatically convert the result to a specific response according to the status code.

```

```csharp

// Controller
// You need to the following code in the Program.cs file

builder.Services
    .AddXOperationResultMiddleware()
    .AddXOperationResultResponseBuilder()
    .AddXOperationResultConfigureMvcOptions();
...
app.UseXOperationResultMiddleware();

[HttpGet]
public object GetUserByName(string name)
{
    if(CheckThatValueIsNotNull(name) is { isFailure : true} failure)
        return failure;

    // ...get the user
	IOperationResult<User> resultUser = DoGetUser(...);
	
    return result;
}

```

In the Minimal Api case, if the *name* is null, the operation result from the method will be converted to an implementation of *IResult*, that will produce a perfect response with all needed information.

>You can also use the **OperationResultException** to throw a specific exception that contains a failure *IOperationResult* when you are not able to return an *IOperationResult* instance.
All the operation result instances are serializable with a specific case for **Asp.Net Core** application, the produced response Content will contains the serialized *Result* property value if available in the operation result.
You will find the same behavior for all the interfaces that use the *IOperationResult* in their method as return value such as : *ICommandHandler< TCommand >*, *IQueryHandler< TQuery, TResult >*, ...*

## Decorator pattern
You can use the extension methods to apply the decorator pattern to your types.

>This method and its extensions ensure that the supplied TDecorator" decorator is returned, wrapping the original registered "TService", by injecting that service type into the constructor of the supplied "TDecorator". Multiple decorators may be applied to the same "TService". By default, a new "TDecorator" instance will be returned on each request, independently of the lifestyle of the wrapped service. Multiple decorators can be applied to the same service type. The order in which they are registered is the order they get applied in. This means that the decorator that gets registered first, gets applied first, which means that the next registered decorator, will wrap the first decorator, which wraps the original service type.

```c#
 services.XTryDecorate<TService, TDecorator, TMarker>();   
```

Suppose you have a command and a command handler defined like this :

```c#
public sealed record AddPersonCommand : ICommand;

public sealed class AddPersonCommandHandler : ICommandHandler<AddPersonCommand>
{
    public Task<IOperationResult> HandleAsync(
        AddPersonCommand command, 
        CancellationToken cancellationToken = default)
    {
        // your code ...

        return OperationResults.Ok().Build();
    }
}
```

```c#
Suppose you want to add logging for the AddPersonCommandHandler, you just need to define the decorator class that will use the logger and the handler.

```c#
public sealed class AddPersonCommandHandlerLoggingDecorator : 
    ICommandHandler<AddPersonCommand>
{
    private readonly ICommandHandler<AddPersonCommand> _decoratee;
    private readonly ILogger<AddPersonCommandHandler> _logger;
    
    public AddPersonCommandHandlerLoggingDecorator(
        ILogger<AddPersonCommandHandler> logger,
        ICommandHandler<AddPersonCommand> decoratee)
        => (_logger, _decoratee) = (logger, decoratee);

    public async Task<IOperationResult> HandleAsync(
        AddPersonCommand command, 
        CancellationToken cancellationToken = default)
    {
        _logger.Information(...);
        
        var response = await _decoratee
            .HandleAsync(command, cancellationToken)
            .configureAwait(false);
        
        _logger.Information(...)
        
        return response;
    }
}
```

And to register the decorator, you just need to call the specific extension method :

```c#
services
    .AddXHandlers()
    .XTryDecorate<AddPersonCommandHandler, AddPersonCommandHandlerLoggingDecorator>();
```

Sometimes you want to use a generic decorator. You can do so for all commands that implement *ICommand*
interface or something else.
ILoggerDecorator is a marker interface that allows to apply the logger decorator to the command.

```c#

public sealed record AddPersonCommand : ICommand, ILoggerDecorator;

public sealed class CommandLoggingDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : notnull, ICommand, ILoggerDecorator // you can add more constraints
{
    private readonly ICommandHandler<TCommand> _ decoratee;
    private readonly ILogger<TCommand> _logger;
    
    public CommandLoggingDecorator(
        ILogger<TCommand> logger, 
        ICommandHandler<TCommand> decoratee)
        => (_logger, _ decoratee) = (logger, decoratee);

    public async Task<IOperationResult> HandleAsync(
         TCommand command, 
         CancellationToken cancellationToken = default)
    {
        _logger.Information(...);
        
        var response = await _decoratee
            .HandleAsync(command, cancellationToken)
            .configureAwait(false);
        
        _logger.Information(...)
        
        return response;
    }
}
```
And for registration the **CommandLoggingDecorator** will be applied to all command handlers whose commands meet the decorator's constraints : To be a *notnull* and implement *ICommand* interface.

```c#
services
    .AddXHandlers()
    .XTryDecorate(typeof(ICommandHandler<>), typeof(CommandLoggingDecorator<>), typeof(ILoggerDecorator));
```

## Commands Pattern

>Commands stands for Command and Query Responsibility Segregation, a pattern that separates read and update operations for a data store.

The following interfaces are used to apply command and query operations :
```c#
public interface IQuery<TResult> {}
public interface IAsyncQuery<TResult> {}
public interface ICommand {}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : notnull, IQuery<TResult> 
{
    Task<IOperationResult<TResult>> HandleAsync(
        TQuery query, 
        CancellationToken cancellationToken = default);
}
public interface IAsyncQueryHandler<TQuery, TResult>
    where TQuery : notnull, IAsyncQuery<TResult> 
{
    IAsyncEnumerable<TResult> HandleAsync(
        TQuery query, 
        CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand>
    where TCommand : notnull, ICommand
{
    Task<IOperationResult> HandleAsync(
        TCommand command, 
        CancellationToken cancellationToken = default);
}

```

So let's create a command and its handler. A command to add a new product for example.

```c#
public sealed record AddProductCommand(
    [property : StringLength(byte.MaxValue, MinimumLength = 3)] string Name,
    [property : StringLength(short.MaxValue, MinimumLength = 3)] string Description) :
    ICommand, IPersistenceDecorator;
```

*ICommand* already contain an **Id** property of type *Guid* and the *IPersistenceDecorator* interface is to allow the command to be persisted at the end of the control flow when there is no exception. **Entity** is a base class that contains common properties for entities.

```c#
public sealed class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    private readonly ProductContext _uow;
    public AddProductCommandHandler(ProductContext uow) => _uow = uow;

    public async Task<IOperationResult> HandleAsync(
        AddProductCommand command, 
        CancellationToken cancellationToken)
    {
        // create the new product instance : 'With' is static method to build a product
        var product = Product.With(command.Id, command.Name, command.Description);

        // insert the new product in the collection of products
        await _uow.Products
            .AddAsync(product, cancellationToken)
            .ConfigureAwait(false);

        //...

        // you can return the product id or the product itself
        return OperationResults
			.Ok(product)
			.Build();
        // or 
        return OperationResults.Ok().Build();
    }
}
```

The validation of the command, the validation of command duplication and persistence will happen during the control flow using decorators.

```c#
public sealed class AddProductCommandValidator<AddProductCommand> :
    Validator<AddProductCommand>
{
     private readonly ProductContext _uow;
    public AddProductCommandValidator(ProductContext uow, IServiceProvider sp)
        :base(sp) => _uow = uow;

     public async Task<IOperationResult> ValidateAsync(AddProductCommand argument)
    {
        // validate the command using attributes
       if(Validate(command) is { isFailure : true } failure)
            return failure;

        // check for duplication
        // You can stop here because if a duplication error occurs while saving, 
        // the final operation result will contain this error.

        // this is just for demo
        // we just need to know if a record with
        // the specified id already exist
        var isFound = await _uow.Products
            .CountAsync(x => x.Id == command.Id, cancellationToken)
            .ConfigureAwait(false) > 0;

        if( isFound ) // duplicate
        {
            // the result can directly be used in a web environment
            return OperationResults
                .Conflict()
                .WithError(nameof(command.Id), $"Command identifier '{command.Id}' already exist")
                .Build();
        }

        return OperationResults.Ok().Build();
    }
}
```

And now let's create a query and its handler to request a product.

```c#
public readonly record struct ProductDTO(string Id, string Name, string Description);

public sealed record GetProductQuery(Guid Id) : IQuery<ProductDTO?>;

// You can use a class and apply a filter directly on that class :
public sealed record GetProductQuery(Guid Id) : 
    QueryExpression<Product>, IQuery<ProductDTO?>
{
    public override Expression<Func<Product, bool>> GetExpression()
        => x => x.Id == Id;
}

public sealed class GetProductQueryHandler : 
    IQueryHandler<GetProductQuery, ProductDTO?>
{
    private readonly ProductContext _uow;

    public GetProductQueryHandler(ProductContext uow) => _uow = uow;

    public async Task<IOperationResult<ProductDTO?>> HandleAsync(
        GetProductQuery query, 
        CancellationToken cancellationToken = default)
    {
        // You can make a search using a query or the key

        if( await _uow.Products
            .Where(query)
            .OrderBy(o => o.Id)
            .Select(s => new ProductDTO(x.Id, x.Name, x.Description))
            .FirstOrDefaultAsync(canellationToken)
            .ConfigureAwait(false)
            is { } productDTO)
        {
            return OperationResults
                .Ok<ProductDTO?>(productDTO)
                .Build();
        }

        // create a key for search --------------
        var key = ProductId.With(query.Id);

        if(await _uow.Products
            .FindAsync(key, cancellationToken)
            .ConfigureAwait(false) is { } product)
        {
            ProductDTO productDTO = new(product.Id, product.Name, product.Description);
            return OperationResults
                .OkResult<ProductDTO?>()
                .WithResult(productDTO)
                .Build();
        }

        return OperationResults
            .NotFound<ProductDTO?>()
            .Build();
    }
}
```
Finally, we need to use the dependency injection to put it all together :

```c#
var serviceProvider = new ServiceCollection()
    .AddXDataContext<ProductContext>(define options)
    .AddXCommandQueryHandlers(
        options => options.UsePersistence().UseValidator())
    .AddXDispatcher()
    .BuildServiceprovider();

    // Add a product
    var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
    var addProduct = new AddProductCommand("Kamersoft 8", "Kamersoft.Net Library");
    IOperationResult result = await dispatcher
        .SendAsync(addProduct)
        .ConfigureAwait(false);

    // check the result
    ...
```

The **AddXDataContext** registers the specified data context using the options provided.  
The **AddXCommandQueryHandlers** registers all handlers found in the executing application, and apply persistence decorator and validation decorator to all the commands according to the constraints.  
The **AddXDispatcher** registers the internal implementation of *IDispatcher* to resolve handlers.

## Features

Usually, when registering types, we are forced to reference the libraries concerned and we end up with a very coupled set.
To avoid this, you can register these types by calling an export extension method, which uses **MEF: Managed Extensibility Framework**.

In your api program class

> **AddXServiceExport(IConfiguration, Action{ExportServiceOptions})** adds and configures registration of services using the *IAddServiceExport* interface implementation found in the target libraries according to the export options. You can use configuration file to set up the libraries to be scanned.
```c#
    ....
    builder.Services
        .AddXServiceExport(
            Configuration, 
            options => options.SearchPattern = "your-search-pattern-dll");
    ...
```
In the library you want types to be registered

```c#
[Export(typeof(IAddServiceExport))]
public sealed class RegisterServiceExport : IAddServiceExport
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // you can register your services here
        ....
    }
}
```


## IAggregate

Libraries also provide with DDD model implementation *IAggregate< TAggregateId>* using event sourcing and out-box pattern.