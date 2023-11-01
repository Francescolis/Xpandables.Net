# Introduction 
Provides with useful interfaces contracts in **.Net 8.0** and some implementations mostly following the spirit of SOLID principles, CQRS...
The library is strongly-typed, which means it should be hard to make invalid requests and it also makes it easy to discover available methods and properties though IntelliSense.

Feel free to fork this project, make your own changes and create a pull request.


# Getting Started

## Optional

The **Optional< T>** type allows to create a named value or a variable that contains a value or not.
An **Optional** has an underlying type and can hold a value of that type, or it might not have a value.

- Is a struct, so it is a value type.
- Is immutable, so it can't be changed once it has been created.
- Is a generic type, so it can hold a value of any type.
- Is a monad, so it can be used in LINQ expressions.
- Is a value object, so it can be used as a key in a dictionary.
- Implements **IEnumerable< T>**, so it can be used in a foreach loop.

- Creating an Optional : Some or Empty

```csharp

var optional = Optional.Some("Hello World");
// optional : Optional<string> = [Some("Hello World")]

```

- Apply a function to the value of an Optional : Map

```csharp

var optional = Optional.Some("Hello World");
var result = optional.Map(value => value.ToUpper());
// result : Optional<string> = [Some("HELLO WORLD")]

```

- Change the type of an Optional : Bind

```csharp

var optional = Optional.Some("Hello World");
var result = optional.Bind(value => value.Length);
// result : Optional<int> = [Some(11)]

```
