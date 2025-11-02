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
using System.Globalization;
using System.Text.Json;

namespace Xpandables.Net.Primitives.Collections;

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
        /// Deserializes the JSON string value into an anonymous object of type <typeparamref name="T"/> using the
        /// specified options.
        /// </summary>
        /// <remarks>This method requires the caller to provide a sample anonymous object to infer the
        /// target type for deserialization. The method may generate dynamic code at runtime, which can impact trimming
        /// and AOT scenarios.</remarks>
        /// <typeparam name="T">The type of the anonymous object to deserialize to. Must match the structure of the JSON string.</typeparam>
        /// <param name="_">A sample anonymous object that defines the expected structure for deserialization. The value is not used.</param>
        /// <param name="options">Options to control the behavior of the JSON deserialization. If null, default options are used.</param>
        /// <returns>An instance of type <typeparamref name="T"/> populated with data from the JSON string, or null if the JSON
        /// is empty or invalid.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to the specified type.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type <typeparamref name="T"/> is not supported for deserialization.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the string value is null.</exception>
        [RequiresUnreferencedCode("Dynamic code may be generated at runtime.")]
        [RequiresDynamicCode("Dynamic code may be generated at runtime.")]
        public T? AnonymousFromJsonString<T>(T _, JsonSerializerOptions? options = null) =>
            JsonSerializer.Deserialize<T>(value, options);

        /// <summary>
        /// Formats the current string using the specified culture and the provided arguments.
        /// </summary>
        /// <remarks>This method uses the current string instance as the format string. The number and
        /// order of format items in the string should match the number and order of objects in <paramref name="args"/>.
        /// If an argument is missing, a formatting exception may be thrown.</remarks>
        /// <param name="cultureInfo">The culture-specific formatting information to use when formatting the string. Cannot be null.</param>
        /// <param name="args">An array of objects to format. Each object will be replaced in the string according to its corresponding
        /// format item.</param>
        /// <returns>A formatted string that incorporates the specified arguments, using the provided culture for formatting.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the string value or <paramref name="cultureInfo"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when the format string is invalid or when an argument is missing.</exception>
        public string StringFormat(CultureInfo cultureInfo, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, args);
        }

        /// <summary>
        /// Formats the underlying string using the specified arguments, applying culture-invariant formatting.
        /// </summary>
        /// <remarks>This method uses <see cref="CultureInfo.InvariantCulture"/> for formatting, ensuring
        /// consistent results regardless of the current culture. If the number of format items in the string does not
        /// match the number of arguments provided, a formatting exception may occur.</remarks>
        /// <param name="args">An array of objects to format and insert into the string. Each object will replace a corresponding format
        /// item in the string.</param>
        /// <returns>A formatted string with each format item replaced by the corresponding value from <paramref name="args"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the string value is null.</exception>
        /// <exception cref="FormatException">Thrown when the format string is invalid or when an argument is missing.</exception>
        public string StringFormat(params object[] args)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.StringFormat(CultureInfo.InvariantCulture, args);
        }
    }
}
