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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.Collections;

/// <summary>
/// Represents an entry consisting of a key and associated values.
/// </summary>
/// <remarks>The <see cref="ElementEntry"/> struct is immutable and is used to store a key-value pair where the
/// key is a string and the values are a collection of strings. It is designed to be used in scenarios where a
/// collection of values needs to be associated with a single key.</remarks>
[StructLayout(LayoutKind.Auto)]
[JsonConverter(typeof(ElementEntryJsonConverter))]
public readonly record struct ElementEntry
{
    /// <summary>
    /// Gets the key of the entry.
    /// </summary>
    public required string Key { get; init; }
    /// <summary>
    /// Gets the values associated with the key.
    /// </summary>
    public required StringValues Values { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementEntry"/> struct.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="values">The values associated with the key.</param>
    /// <exception cref="ArgumentException">Thrown when values are empty or null.</exception>
    [SetsRequiredMembers]
    public ElementEntry(string key, params string[] values)
    {
        if (values is null || values.Length == 0)
        {
            throw new ArgumentException("Values cannot be empty.", nameof(values));
        }

        Key = key;
        Values = values;
    }

    /// <summary>
    /// Returns a string representation of the <see cref="ElementEntry"/> instance.
    /// </summary>
    /// <returns> A string that represents the current <see cref="ElementEntry"/>.</returns>
    public override string ToString() => $"{Key}: {Values.StringJoin(",")}";
}
