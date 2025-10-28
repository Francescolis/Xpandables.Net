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

namespace Xpandables.Net.Tasks.Collections;

/// <summary>
/// Provides extension methods for working with string values.
/// </summary>
/// <remarks>This class contains methods that extend the functionality of the string type, enabling additional
/// operations such as deserialization or conversion. All methods are static and are intended to be used as extension
/// methods on string instances.</remarks>
public static class StringExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="value">The string value.</param>
    extension(string value)
    {
        /// <summary>
        /// Deserializes the current JSON string value to an instance of the ElementCollection class.
        /// </summary>
        /// <returns>An ElementCollection object representing the deserialized JSON value, or null if the value cannot be
        /// deserialized.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to an ElementCollection.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for deserialization.</exception>
        public ElementCollection ToElementCollection() =>
            JsonSerializer.Deserialize(value, ElementCollectionContext.Default.ElementCollection);
    }
}
