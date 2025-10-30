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
using System.Text.Json;

using Xpandables.Net.ExecutionResults.Collections;

namespace Xpandables.Net.ExecutionResults.Collections;

/// <summary>
/// Provides extension methods for ElementCollection objects.
/// </summary>
/// <remarks>These extension methods automatically use the ElementCollectionContext for optimal performance
/// and AOT compatibility when working with ElementCollection serialization.</remarks>
public static class ElementCollectionExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="source">The sequence to act on.</param>
    extension(IEnumerable<ElementEntry> source)
    {
        /// <summary>
        /// Serializes the element collection to a JSON-formatted string.
        /// </summary>
        /// <returns>A string containing the JSON representation of the element collection.</returns>
        /// <exception cref="JsonException">Thrown when serialization fails.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type of the element collection is not supported for serialization.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the element collection contains invalid data that cannot be serialized.</exception>
        public string ToJsonString() => JsonSerializer.Serialize(source, ElementCollectionContext.Default.ElementCollection);

        /// <summary>
        /// Converts the current collection to a dictionary with string keys and string values, concatenating multiple
        /// values for each key into a single space-separated string.
        /// </summary>
        /// <remarks>Each entry in the returned dictionary has a key from the source and a value that is a
        /// space-separated string of all associated values. If a key has no associated values, its value will be an
        /// empty string.</remarks>
        /// <returns>An <see cref="IDictionary{TKey, TValue}"/> containing all keys from the source collection. If the source is
        /// empty, returns an empty dictionary.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source collection is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there are duplicate keys in the source collection.</exception>
        /// <exception cref="ArgumentException">Thrown when the source collection contains invalid data that cannot be converted to a dictionary.</exception>
        public IDictionary<string, string> ToDictionaryObject()
        {
            if (!source.Any())
            {
                return new Dictionary<string, string>();
            }

            return source
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Values.StringJoin(" "));
        }
    }
}