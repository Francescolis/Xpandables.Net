
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Represents an entry for <see cref="ElementCollection"/>.
/// </summary>
public readonly record struct ElementEntry
{
    /// <summary>
    /// Gets the key associated with the element.
    /// </summary>
    [Required, DataType(DataType.Text)]
    public required string Key { get; init; }

    /// <summary>
    /// Gets the collection of strings that represents the values.
    /// </summary>
    [Required]
    public required IReadOnlyCollection<string> Values { get; init; }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="ElementEntry"/> with the specified key and the values.
    /// </summary>
    /// <param name="key">The key associated with the key.</param>
    /// <param name="values">The string that represents the values.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="values"/> is empty.</exception>
    [SetsRequiredMembers]
    public ElementEntry(string key, params string[] values)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(values),
                $"{nameof(values)} can not be empty.");
        }

        Key = key;
        Values = [.. values];
    }
}
