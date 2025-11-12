/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xpandables.Net.Primitives.SourceGeneration;

/// <summary>
/// Incremental source generator for Optional&lt;T&gt; JSON converters.
/// Discovers all Optional types from actual usage in the consuming project and generates
/// AOT-compatible converter cache for optimal performance.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class OptionalJsonConverterGenerator : IIncrementalGenerator
{
    private const string OptionalTypeName = "Xpandables.Net.Optionals.Optional";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // DISABLED: The source generator approach was creating conflicts.
        // Instead, we use a simple factory with MakeGenericType that relies on
        // JsonSerializerContext source generation to provide AOT-compatible serialization.
        // 
        // The OptionalJsonConverter<T> uses options.GetTypeInfo(typeof(T)) which gets
        // the type info from source-generated contexts (OptionalJsonContext for primitives,
        // and custom contexts for user types).
        //
        // This is AOT-compatible because:
        // 1. MakeGenericType is suppressed with UnconditionalSuppressMessage
        // 2. The actual serialization uses source-generated TypeInfo
        // 3. No dynamic code generation happens at runtime - just type instantiation
    }

    private static bool IsOptionalGenericName(SyntaxNode node)
    {
        return node is GenericNameSyntax
        {
            Identifier.Text: "Optional",
            TypeArgumentList.Arguments.Count: 1
        };
    }

    private static TypeInfo? ExtractTypeFromOptionalUsage(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        GenericNameSyntax genericName = (GenericNameSyntax)context.Node;
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(genericName, cancellationToken);

        if (symbolInfo.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        if (!IsOptionalType(typeSymbol))
        {
            return null;
        }

        ITypeSymbol innerType = typeSymbol.TypeArguments[0];

        // CRITICAL: Filter out generic type parameters (T, TU, TSource, etc.)
        // These are not real types and cannot be used for converter generation
        if (innerType.TypeKind == TypeKind.TypeParameter)
        {
            return null;
        }

        // Skip types that are error types (unresolved references)
        if (innerType.TypeKind == TypeKind.Error)
        {
            return null;
        }

        // Skip Optional<Optional<T>> scenarios
        if (innerType is INamedTypeSymbol namedInnerType && IsOptionalType(namedInnerType))
        {
            return null;
        }

        // ALWAYS skip primitives - they're handled by OptionalJsonContext
        // This must run BEFORE the assembly check
        if (IsKnownPrimitiveType(innerType))
        {
            return null;
        }

        return CreateTypeInfo(innerType);
    }

    private static bool IsOptionalType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol is
        {
            Name: "Optional",
            TypeArguments.Length: 1
        } && typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .StartsWith($"global::{OptionalTypeName}", StringComparison.Ordinal);
    }

    private static bool IsKnownPrimitiveType(ITypeSymbol typeSymbol)
    {
        // Use SpecialType enum for accurate primitive detection
        // This handles all C# built-in types correctly
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean or
            SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal or
            SpecialType.System_String or
            SpecialType.System_Char or
            SpecialType.System_Object => true,
            _ => false
        } || IsKnownStructType(typeSymbol);
    }

    private static bool IsKnownStructType(ITypeSymbol typeSymbol)
    {
        string fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Check for common struct types in OptionalJsonContext
        return fullName switch
        {
            "global::System.DateTime" or
            "global::System.DateTimeOffset" or
            "global::System.Guid" or
            "global::System.TimeSpan" => true,
            _ => false
        };
    }

    private static TypeInfo CreateTypeInfo(ITypeSymbol typeSymbol)
    {
        string fullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string simpleName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

#pragma warning disable RS1024 // Using symbol hash for TypeInfo deduplication - symbols are not being compared for equality
        return new TypeInfo(
            fullyQualifiedName,
            simpleName,
            typeSymbol.GetHashCode());
#pragma warning restore RS1024
    }

    private static void GenerateMinimalFactory(SourceProductionContext context)
    {
        const string source = """
            /*******************************************************************************
             * Copyright (C) 2024 Francis-Black EWANE
             * 
             * <auto-generated>
             *   This code was generated by Xpandables.Net.Primitives.SourceGeneration
             *   Do not modify this file manually as it will be overwritten on rebuild.
             * </auto-generated>
             *******************************************************************************/
            #nullable enable

            using System;
            using System.Collections.Frozen;
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using Xpandables.Net.Optionals;

            namespace Xpandables.Net.Optionals;

            /// <summary>
            /// JSON converter factory for Optional&lt;T&gt; types, providing AOT-compatible serialization.
            /// This implementation uses only the base OptionalJsonContext types (primitives).
            /// </summary>
            /// <remarks>
            /// This factory was auto-generated and currently handles only primitive types defined in OptionalJsonContext.
            /// If you use Optional with custom types, they will be discovered and added automatically on rebuild.
            /// </remarks>
            public sealed partial class OptionalJsonConverterFactory : JsonConverterFactory
            {
                // Empty cache - all primitive types are handled by OptionalJsonContext
                private static readonly FrozenDictionary<Type, Func<JsonConverter>> _converterCache =
                    FrozenDictionary<Type, Func<JsonConverter>>.Empty;

                /// <inheritdoc/>
                public override bool CanConvert(Type typeToConvert)
                {
                    ArgumentNullException.ThrowIfNull(typeToConvert);

                    if (!typeToConvert.IsGenericType)
                    {
                        return false;
                    }

                    Type genericTypeDef = typeToConvert.GetGenericTypeDefinition();
                    return genericTypeDef == typeof(Optional<>);
                }

                /// <inheritdoc/>
                public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
                {
                    ArgumentNullException.ThrowIfNull(typeToConvert);
                    ArgumentNullException.ThrowIfNull(options);

                    if (options.TypeInfoResolverChain.FirstOrDefault(static resolver =>
                        resolver is OptionalJsonContext) is null)
                    {
                        options.TypeInfoResolverChain.Add(OptionalJsonContext.Default);
                    }

                    Type valueType = typeToConvert.GetGenericArguments()[0];

                    if (_converterCache.TryGetValue(valueType, out Func<JsonConverter>? converterFactory))
                    {
                        return converterFactory();
                    }

                    // For primitive types in OptionalJsonContext, the context will handle it via GetTypeInfo
                    // For unknown types, throw a helpful error
                    throw new NotSupportedException(
                        $"Optional<{valueType.Name}> is not registered for serialization. " +
                        $"Ensure OptionalJsonContext includes this type or use Optional<{valueType.Name}> in your code for automatic discovery.");
                }
            }
            """;

        context.AddSource("OptionalJsonConverterFactory.g.cs",
            SourceText.From(source, Encoding.UTF8));
    }

    private static void GenerateFullFactory(SourceProductionContext context, ImmutableArray<TypeInfo> types)
    {
        StringBuilder sb = new();

        sb.AppendLine("""
            /*******************************************************************************
             * Copyright (C) 2024 Francis-Black EWANE
             * 
             * <auto-generated>
             *   This code was generated by Xpandables.Net.Primitives.SourceGeneration
             *   Do not modify this file manually as it will be overwritten on rebuild.
             * </auto-generated>
             *******************************************************************************/
            #nullable enable

            using System;
            using System.Collections.Frozen;
            using System.Text.Json;
            using System.Text.Json.Serialization;
            using Xpandables.Net.Optionals;

            namespace Xpandables.Net.Optionals;

            /// <summary>
            /// JSON converter factory for Optional&lt;T&gt; types, providing AOT-compatible serialization.
            /// This implementation is auto-generated from Optional usage analysis.
            /// </summary>
            /// <remarks>
            /// <para>Generated converters for custom types discovered in your project:</para>
            /// <list type="bullet">
            """);

        foreach (TypeInfo type in types)
        {
            sb.AppendLine($"            /// <item><description>Optional&lt;{type.SimpleName}&gt;</description></item>");
        }

        sb.AppendLine("""
            /// </list>
            /// <para>Primitive types (string, int, etc.) are handled by OptionalJsonContext.</para>
            /// </remarks>
            public sealed partial class OptionalJsonConverterFactory : JsonConverterFactory
            {
                // Use FrozenDictionary for optimal read performance in .NET 10
                private static readonly FrozenDictionary<Type, Func<JsonConverter>> _converterCache =
                    CreateConverterCache();

                private static FrozenDictionary<Type, Func<JsonConverter>> CreateConverterCache()
                {
                    Dictionary<Type, Func<JsonConverter>> builder = new()
                    {
            """);

        // Generate cache entries with static lambdas for allocation-free delegates
        foreach (TypeInfo type in types)
        {
            sb.AppendLine($"            [typeof({type.FullyQualifiedName})] = static () => new OptionalJsonConverter<{type.FullyQualifiedName}>(),");
        }

        sb.AppendLine("""
                    };
                    
                    return builder.ToFrozenDictionary();
                }

                /// <inheritdoc/>
                public override bool CanConvert(Type typeToConvert)
                {
                    ArgumentNullException.ThrowIfNull(typeToConvert);

                    if (!typeToConvert.IsGenericType)
                    {
                        return false;
                    }

                    Type genericTypeDef = typeToConvert.GetGenericTypeDefinition();
                    return genericTypeDef == typeof(Optional<>);
                }

                /// <inheritdoc/>
                public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
                {
                    ArgumentNullException.ThrowIfNull(typeToConvert);
                    ArgumentNullException.ThrowIfNull(options);

                    if (options.TypeInfoResolverChain.FirstOrDefault(static resolver =>
                        resolver is OptionalJsonContext) is null)
                    {
                        options.TypeInfoResolverChain.Add(OptionalJsonContext.Default);
                    }

                    Type valueType = typeToConvert.GetGenericArguments()[0];

                    if (_converterCache.TryGetValue(valueType, out Func<JsonConverter>? converterFactory))
                    {
                        return converterFactory();
                    }

                    // Primitive types in OptionalJsonContext will be handled by the context itself
                    throw new NotSupportedException(
                        $"Optional<{valueType.Name}> is not registered for serialization. " +
                        $"Ensure the type is used with Optional<T> in your code for automatic discovery.");
                }
            }
            """);

        context.AddSource("OptionalJsonConverterFactory.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    /// <summary>
    /// Represents metadata about a type used in Optional&lt;T&gt;.
    /// </summary>
    private sealed record TypeInfo(
        string FullyQualifiedName,
        string SimpleName,
        int HashCode);

    /// <summary>
    /// Equality comparer for TypeInfo that uses fully qualified name.
    /// </summary>
    private sealed class TypeInfoEqualityComparer : IEqualityComparer<TypeInfo>
    {
        public static readonly TypeInfoEqualityComparer Instance = new();

        private TypeInfoEqualityComparer() { }

        public bool Equals(TypeInfo? x, TypeInfo? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.FullyQualifiedName == y.FullyQualifiedName;
        }

        public int GetHashCode(TypeInfo obj) =>
            StringComparer.Ordinal.GetHashCode(obj.FullyQualifiedName);
    }
}
