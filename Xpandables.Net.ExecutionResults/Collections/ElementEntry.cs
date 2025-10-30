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
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.ExecutionResults.Collections;

namespace Xpandables.Net.ExecutionResults.Collections;

/// <summary>
/// Represents an entry consisting of a key and associated values.
/// </summary>
/// <remarks>The <see cref="ElementEntry"/> struct is immutable and is used to store a key-value pair where the
/// key is a string and the values are a collection of strings. It is designed to be used in scenarios where a
/// collection of values needs to be associated with a single key.
/// <para>For serialization and deserialization of <see cref="ElementEntry"/> instances, it is recommended to use the
/// <see cref="ElementEntryContext"/> source generation context. This approach provides better performance</para>
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[JsonConverter(typeof(ElementEntryJsonConverterFactory))]
public readonly record struct ElementEntry
{
    /// <summary>
    /// Gets the key of the entry.
    /// </summary>
    public readonly required string Key { get; init; }
    /// <summary>
    /// Gets the values associated with the key.
    /// </summary>
    public readonly required StringValues Values { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementEntry"/> struct.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="values">The values associated with the key.</param>
    /// <exception cref="ArgumentException">Thrown when values are empty or null.</exception>
    [SetsRequiredMembers]
    public ElementEntry(string key, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (values is null || values.Length == 0)
        {
            throw new ArgumentException("Values cannot be empty.", nameof(values));
        }

        Key = key;
        Values = values;
    }

    /// <summary>
    /// Initializes a new instance of the ElementEntry class with the specified key and associated values.
    /// </summary>
    /// <param name="key">The key that identifies the element. Cannot be null.</param>
    /// <param name="values">The collection of values associated with the key. Must contain at least one value.</param>
    [SetsRequiredMembers]
    public ElementEntry(string key, StringValues values)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentOutOfRangeException.ThrowIfZero(values.Count);

        Key = key;
        Values = values;
    }

    /// <summary>
    /// Returns a string representation of the <see cref="ElementEntry"/> instance.
    /// </summary>
    /// <returns> A string that represents the current <see cref="ElementEntry"/>.</returns>
    public readonly override string ToString() => $"{Key}: {Values.StringJoin(",")}";
}

/// <summary>
/// Provides a source generation context for serializing and deserializing ElementEntry objects using System.Text.Json.
/// <code>
/// // Registration in Console/General Application
///     JsonSerializerOptions.Default.TypeInfoResolver = JsonTypeInfoResolver.Combine(
///     ElementEntryContext.Default,
///     new DefaultJsonTypeInfoResolver());
///     
/// // AspNet Core Registration
///     builder.Services.ConfigureHttpJsonOptions(options =>
///     {
///         options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
///             ElementEntryContext.Default,
///             new DefaultJsonTypeInfoResolver());
///         options.SerializerOptions.Converters.Add(new ElementEntryJsonConverterFactory());
///     });
///     
/// // Or in Minimal API
///     builder.Services.Configure&lt;JsonOptions&gt;(options=>
///     {
///         options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
///             ElementEntryContext.Default,
///             new DefaultJsonTypeInfoResolver());
///         options.SerializerOptions.Converters.Add(new ElementEntryJsonConverterFactory());
///     });
/// </code>
/// </summary>
/// <remarks>This context enables efficient, compile-time generation of serialization logic for the ElementEntry
/// type and its associated StringValues. Use this context with System.Text.Json serialization APIs to improve 
/// performance and reduce runtime reflection when working with ElementEntry instances. This approach is preferred
/// over custom JsonConverter implementations for better AOT compatibility and performance in .NET 10.</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ElementEntry))]
[JsonSerializable(typeof(ElementEntry[]))]
[JsonSerializable(typeof(List<ElementEntry>))]
[JsonSerializable(typeof(IEnumerable<ElementEntry>))]
[JsonSerializable(typeof(ElementEntryJsonConverterFactory))]
[JsonSerializable(typeof(StringValues))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(string))]
public partial class ElementEntryContext : JsonSerializerContext { }