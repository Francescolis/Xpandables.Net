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

namespace Xpandables.Net.Collections;

/// <summary>
/// Represents an entry in a collection with a key and associated values.
/// </summary>
public readonly record struct ElementEntry
{
    /// <summary>
    /// Gets the key of the entry.
    /// </summary>
    public required string Key { get; init; }
    /// <summary>
    /// Gets the values associated with the key.
    /// </summary>
    public required IReadOnlyCollection<string> Values { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementEntry"/> struct.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="values">The values associated with the key.</param>
    /// <exception cref="ArgumentException">Thrown when values are empty.</exception>
    [SetsRequiredMembers]
    public ElementEntry(string key, params string[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "Values cannot be empty.",
                nameof(values));
        }

        Key = key;
        Values = values;
    }
}
