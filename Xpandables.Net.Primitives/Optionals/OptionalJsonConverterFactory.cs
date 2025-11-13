/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Optionals;

/// <summary>
/// JSON converter factory for Optional&lt;T&gt; types, providing AOT-compatible serialization for .NET 10.
/// </summary>
/// <remarks>
/// <para>
/// This factory handles conversion for all Optional&lt;T&gt; types.
/// It creates generic converters that delegate to the TypeInfoResolver for actual serialization.
/// </para>
/// <para>
/// Primitive types are handled via <see cref="OptionalJsonContext"/>, while custom types
/// should be registered in a source-generated JsonSerializerContext in the consuming project.
/// </para>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new OptionalJsonConverterFactory() },
///     TypeInfoResolver = JsonTypeInfoResolver.Combine(
///         OptionalJsonContext.Default,
///         MyCustomContext.Default)
/// };
/// </code>
/// </remarks>
public sealed partial class OptionalJsonConverterFactory : JsonConverterFactory
{
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
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "This is only called when types are not source-generated. Users should use source generation for AOT scenarios.")]
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
        Type converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
