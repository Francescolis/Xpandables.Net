# Xpandables.Net.Primitives.SourceGeneration

Source generators for Xpandables.Net.Primitives library, providing AOT-compatible JSON serialization for Optional&lt;T&gt; types.

## Overview

This package contains Roslyn source generators that automatically discover `Optional<T>` usage in your codebase and generate optimized, AOT-compatible JSON converter factories at compile time.

## Features

- **Automatic Discovery**: Scans your code for all `Optional<T>` usages
- **AOT Compatible**: Zero reflection, zero runtime code generation
- **Performance Optimized**: Uses `FrozenDictionary` for O(1) lookup performance
- **Incremental Compilation**: Only regenerates when code changes
- **Zero Configuration**: Works automatically when you reference Xpandables.Net.Primitives

## How It Works

1. The generator scans your compilation for any usage of `Optional<T>`
2. Extracts the type `T` from each usage
3. Filters out primitive types (already handled by `OptionalJsonContext`)
4. Generates a `FrozenDictionary` cache with pre-compiled converters
5. All generation happens at compile-time - zero runtime overhead

## Usage

Simply reference the `Xpandables.Net.Primitives` package in your project. The source generator runs automatically during compilation.

```csharp
public class UserDto
{
    public Optional<string> Name { get; set; }      // ? Primitive, handled by OptionalJsonContext
    public Optional<int> Age { get; set; }          // ? Primitive, handled by OptionalJsonContext
    public Optional<Address> Address { get; set; }  // ? Custom type, auto-discovered by generator!
}

public record Address(string Street, string City);

// The generator automatically creates converters for all custom types
```

## Generated Code

The generator produces a partial class `OptionalJsonConverterFactory` that looks like:

```csharp
public sealed partial class OptionalJsonConverterFactory : JsonConverterFactory
{
    private static readonly FrozenDictionary<Type, Func<JsonConverter>> _converterCache =
        CreateConverterCache();

    private static FrozenDictionary<Type, Func<JsonConverter>> CreateConverterCache()
    {
        var builder = new Dictionary<Type, Func<JsonConverter>>
        {
            [typeof(Address)] = static () => new OptionalJsonConverter<Address>(),
            // ... other discovered types
        };
        
        return builder.ToFrozenDictionary();
    }
}
```

## Requirements

- .NET 10.0 or later
- C# 14 or later
- Xpandables.Net.Primitives package

## Performance

- **Compile-time generation**: Zero runtime overhead
- **FrozenDictionary**: Optimized for read-heavy scenarios
- **Static lambdas**: No delegate allocations
- **AOT-ready**: Works with Native AOT compilation

## License

Apache-2.0

Copyright © Kamersoft 2025
