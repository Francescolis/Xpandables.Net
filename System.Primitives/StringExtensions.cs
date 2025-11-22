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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace System;

/// <summary>
/// Provides extension methods for working with string values.
/// </summary>
/// <remarks>This class contains methods that extend the functionality of the string type, enabling additional
/// operations such as deserialization or conversion. All methods are static and are intended to be used as extension
/// methods on string instances.</remarks>
public static partial class StringExtensions
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
        public T? DeserializeAnonymousType<T>(T _, JsonSerializerOptions? options = null) =>
            JsonSerializer.Deserialize<T>(value, options);

        /// <summary>
        /// Deserializes the current JSON string value to an instance of the ElementCollection class.
        /// </summary>
        /// <returns>An ElementCollection object representing the deserialized JSON value, or null if the value cannot be
        /// deserialized.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to an ElementCollection.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for deserialization.</exception>
        public ElementCollection ToElementCollection() =>
            JsonSerializer.Deserialize(value, ElementCollectionContext.Default.ElementCollection);

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

        /// <summary>
        /// Splits the type name stored in the current instance into its constituent parts based on common naming
        /// conventions, such as transitions between lowercase and uppercase letters, acronyms, and digits.
        /// </summary>
        /// <remarks>This method is useful for formatting type names for display or analysis, especially
        /// when working with names that use PascalCase, camelCase, or include acronyms and digits.</remarks>
        /// <returns>A string containing the separated parts of the type name, joined by spaces.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the regular expression used to split the type name fails to produce matches.</exception>
        public string SplitTypeName()
        {
            ArgumentNullException.ThrowIfNull(value);

            // Regex to split on transitions: lowercase→uppercase, acronym→normal, letter→digit, digit→letter
            var parts = TypeNameFormaterRegex().Matches(value)
                             ?? throw new InvalidOperationException("Regex failed");

            return string.Join(" ", parts);
        }
    }

    extension<TSource>(IEnumerable<TSource> collection)
    {
        /// <summary>
        /// Concatenates the elements of the collection into a single string, using the specified character as the
        /// separator.
        /// </summary>
        /// <param name="separator">The character to use as a separator between elements in the resulting string.</param>
        /// <returns>A string consisting of the collection's elements separated by the specified character. Returns an empty
        /// string if the collection contains no elements.</returns>
        public string StringJoin(char separator) =>
            string.Join(separator.ToString(CultureInfo.InvariantCulture), collection);

        /// <summary>
        /// Concatenates the elements of the collection into a single string, using the specified separator between each
        /// element.
        /// </summary>
        /// <param name="separator">The string to use as a separator between elements. If null, an empty string is used as the separator.</param>
        /// <returns>A string that consists of the elements of the collection delimited by the specified separator. Returns an
        /// empty string if the collection contains no elements.</returns>
        public string StringJoin(string separator) => string.Join(separator, collection);
    }

    [GeneratedRegex(@"([A-Z]+(?=$|[A-Z][a-z0-9])|[A-Z]?[a-z0-9]+)")]
    private static partial Regex TypeNameFormaterRegex();

}
