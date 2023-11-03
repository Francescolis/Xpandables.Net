# Introduction 
Provides with useful interfaces contracts in **.Net 8.0** and some implementations mostly following the spirit of SOLID principles, CQRS...
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

## OperationResult

Allows to create methods that return the status of an execution.

>This type contains all properties according to the result of the method execution.
Some of those properties let you determine for example if the result instance is generic, a collection of errors, 
the status code or the value of the execution.
The status code here is the one from the ***System.Net.HttpStatusCode***.
It contains methods to convert from non-generic type to generic and vis-versa.
The type is useful if you want to return a result that can be analyzed even in a web environment 
by using some extensions that can automatically convert an *OperationResult* to *IResult*.

The non generic type has the following properties :

- An *object* **Result**, a nullable property that qualifies or contains information about an operation return if available. You should call the method **HasResult()** before accessing the property to avoid a *NullReferenceException*.
- A *string* **LocationUrl**, a nullable property that contains the URL mostly used with the status code **Created** in the web environment. You should call the method H**asLocationUrl()** before accessing the property to avoid a *NullReferenceException*.
- An *ElementCollection* **Headers** property that contains a collection of headers if available. *ElementCollection* is a predefined record struct that contains a collection of *ElementEntry* with useful methods.
- An *ElementCollection* **Errors** property that stores errors. Each error is a predefined *ElementEntry* struct which contains the error key and the error message and/or exceptions.
- A *HttpStatusCode* **StatusCode** property that contains the status code of the execution. The status code from the ***System.Net.HttpStatusCode***.
- A *boolean* **IsGeneric** to determine whether or not the current instance is generic.
- A *boolean* **IsSuccess** and **IsFailure** to determine whether or not the operation is a success or a failure according to ***System.Net.HttpStatusCode***.
- A *string* **Title** that contains the operation summary problem from the execution operation.
- A *string* **Detail** that contains he operation explanation specific to the execution operation.

The generic type overrides the *object* **Result** to *TResult* type.

Create a method that returns an **OperationResult** :

```csharp

public OperationResult CheckThatValueMatchCriteria(string? value)
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

The method returns an OperationResult* struct.
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

[HttpGet]
public IResult GetUserByName(string? name)
{
    if(CheckThatValueIsNotNull(name) is { isFailure : true} failure)
        return failure.ToMinimalResult();

    // ...get the user
	OperationResult<User> resultUser = DoGetUser(...);
	
    return result.ToMinimalResult();
}

```

In this case, if the *name* is null, the operation result from the method will be converted to an implementation of *IResult* using the extension method **ToMinimalResult**, that will produce a perfect response with all needed information.

>You can also use the **OperationResultException** to throw a specific exception that contains a failure *OperationResult* when you are not able to return an *OperationResult* instance.
All the operation result instances are serializable with a specific case for **Asp.Net Core** application, the produced response Content will contains the serialized *Result* property value if available in the operation result.
You will find the same behavior for all the interfaces that use the *OperationResult* in their method as return value such as : *ICommandHandler< TCommand >*, *IQueryHandler< TQuery, TResult >*, ...*
