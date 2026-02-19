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
        /// <remarks>If an argument is missing, a formatting exception may be thrown.</remarks>
        /// <param name="cultureInfo">The culture-specific formatting information to use when formatting the string. Cannot be null.</param>
        /// <param name="arg0">The object to format and insert into the string. This object will replace the first format item in the string.</param>
        /// <returns>A formatted string that incorporates the specified arguments, using the provided culture for formatting.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the string value or <paramref name="cultureInfo"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when the format string is invalid or when an argument is missing.</exception>
        public string StringFormat(CultureInfo cultureInfo, object? arg0)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0);
        }

        /// <summary>
        /// Formats the current string using the specified culture-specific formatting information and two arguments.
        /// </summary>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first object to format and insert into the string.</param>
        /// <param name="arg1">The second object to format and insert into the string.</param>
        /// <returns>A copy of the current string in which format items have been replaced by the string representations of the
        /// corresponding arguments, formatted using the specified culture.</returns>
        public string StringFormat(CultureInfo cultureInfo, object? arg0, object? arg1)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0, arg1);
        }

        /// <summary>
        /// Formats the composite format string using the specified culture-specific formatting information and three
        /// arguments.
        /// </summary>
        /// <remarks>The format string is provided by the instance's value. If any argument is null, it is
        /// replaced by an empty string in the result.</remarks>
        /// <param name="cultureInfo">The culture-specific formatting information to use when formatting the string. Cannot be null.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>A string in which the format items have been replaced by the string representations of the corresponding
        /// arguments, formatted using the specified culture.</returns>
        public string StringFormat(CultureInfo cultureInfo, object? arg0, object? arg1, object? arg2)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0, arg1, arg2);
        }

        /// <summary>
        /// Formats the composite format string using the specified culture-specific formatting information and up to
        /// four arguments.
        /// </summary>
        /// <remarks>This method uses the composite formatting feature, where placeholders in the format
        /// string are replaced by the string representations of the provided arguments. If an argument is null, its
        /// corresponding format item is replaced with an empty string.</remarks>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information for the result string. Cannot be null.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <param name="arg3">The fourth object to format.</param>
        /// <returns>A copy of the format string in which format items are replaced by the string representations of the
        /// corresponding arguments, formatted using the specified culture.</returns>
        public string StringFormat(CultureInfo cultureInfo, object? arg0, object? arg1, object? arg2, object? arg3)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// Formats the composite format string using the specified culture-specific formatting information and five
        /// format arguments.
        /// </summary>
        /// <remarks>The composite format string is provided by the instance and may contain indexed
        /// placeholders such as {0} through {4}, which are replaced by the corresponding argument values. If an
        /// argument is null, its corresponding format item is replaced by an empty string.</remarks>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information for the result string. Cannot be null.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <param name="arg3">The fourth object to format.</param>
        /// <param name="arg4">The fifth object to format.</param>
        /// <returns>A string in which the format items have been replaced by the string representations of the corresponding
        /// arguments, formatted using the specified culture.</returns>
        public string StringFormat(CultureInfo cultureInfo, object? arg0, object? arg1, object? arg2, object? arg3, object? arg4)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Formats the current string using the specified culture-specific formatting information and up to six format
        /// arguments.
        /// </summary>
        /// <remarks>Format items in the string should be in the form {index}, where index corresponds to
        /// the position of the argument. If an argument is null, its format item is replaced with an empty
        /// string.</remarks>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information for the operation. Cannot be null.</param>
        /// <param name="arg0">The first object to format. Can be null.</param>
        /// <param name="arg1">The second object to format. Can be null.</param>
        /// <param name="arg2">The third object to format. Can be null.</param>
        /// <param name="arg3">The fourth object to format. Can be null.</param>
        /// <param name="arg4">The fifth object to format. Can be null.</param>
        /// <param name="arg5">The sixth object to format. Can be null.</param>
        /// <returns>A copy of the current string in which format items are replaced by the string representations of the
        /// corresponding arguments, formatted using the specified culture.</returns>
        public string StringFormat(CultureInfo cultureInfo, object? arg0, object? arg1, object? arg2, object? arg3, object? arg4, object? arg5)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(cultureInfo);

            return string.Format(cultureInfo, value, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Formats the current string instance using the specified argument and the invariant culture.
        /// </summary>
        /// <remarks>This method uses <see cref="CultureInfo.InvariantCulture"/> to format the argument.
        /// Use this method when a culture-invariant result is required, such as for logging or serialization.</remarks>
        /// <param name="arg0">The object to format and insert into the format string.</param>
        /// <returns>A copy of the current string instance in which the format item has been replaced by the string
        /// representation of <paramref name="arg0"/>.</returns>
        public string StringFormat(object? arg0)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0);
        }

        /// <summary>
        /// Formats a composite format string using the specified arguments and returns the result as a string.
        /// </summary>
        /// <remarks>This method uses the invariant culture to format the composite string, ensuring
        /// consistent formatting regardless of the current culture settings.</remarks>
        /// <param name="arg0">The first object to format and insert into the format string.</param>
        /// <param name="arg1">The second object to format and insert into the format string.</param>
        /// <returns>A string in which the format items have been replaced by the string representations of the corresponding
        /// arguments, formatted using the invariant culture.</returns>
        public string StringFormat(object? arg0, object? arg1)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0, arg1);
        }

        /// <summary>
        /// Formats a composite format string using the specified arguments and the invariant culture.
        /// </summary>
        /// <remarks>This method uses the invariant culture to format the string, ensuring consistent
        /// formatting regardless of the current culture settings. The composite format string is provided by the
        /// instance and must not be null.</remarks>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>A string in which the format items have been replaced by the string representations of the corresponding
        /// arguments, formatted using the invariant culture.</returns>
        public string StringFormat(object? arg0, object? arg1, object? arg2)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0, arg1, arg2);
        }

        /// <summary>
        /// Formats the current string by replacing format items with the string representations of four specified
        /// objects, using invariant culture formatting.
        /// </summary>
        /// <remarks>Format items in the string should be in the form {0}, {1}, {2}, and {3},
        /// corresponding to the provided arguments. Formatting uses the invariant culture, which is culture-insensitive
        /// and consistent across different locales.</remarks>
        /// <param name="arg0">The first object to format and insert into the string.</param>
        /// <param name="arg1">The second object to format and insert into the string.</param>
        /// <param name="arg2">The third object to format and insert into the string.</param>
        /// <param name="arg3">The fourth object to format and insert into the string.</param>
        /// <returns>A copy of the current string in which format items are replaced by the string representations of the
        /// corresponding arguments.</returns>
        public string StringFormat(object? arg0, object? arg1, object? arg2, object? arg3)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// Formats a composite format string by using the specified arguments and returns the formatted string.
        /// </summary>
        /// <remarks>The formatting uses the invariant culture. If any argument is null, it is replaced
        /// with an empty string in the result. The composite format string and the number of arguments must match;
        /// otherwise, a FormatException is thrown.</remarks>
        /// <param name="arg0">The first object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg1">The second object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg2">The third object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg3">The fourth object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg4">The fifth object to format. Can be null, in which case an empty string is substituted.</param>
        /// <returns>A string in which the format items in the composite format string are replaced by the string representations
        /// of the corresponding arguments.</returns>
        public string StringFormat(object? arg0, object? arg1, object? arg2, object? arg3, object? arg4)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Formats a composite format string by replacing each format item with the string representation of the
        /// corresponding argument using invariant culture.
        /// </summary>
        /// <remarks>This method uses the invariant culture to format the composite string, ensuring
        /// consistent formatting regardless of the current culture settings. If a format item in the string does not
        /// have a corresponding argument, a FormatException is thrown.</remarks>
        /// <param name="arg0">The first object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg1">The second object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg2">The third object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg3">The fourth object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg4">The fifth object to format. Can be null, in which case an empty string is substituted.</param>
        /// <param name="arg5">The sixth object to format. Can be null, in which case an empty string is substituted.</param>
        /// <returns>A copy of the format string in which format items are replaced by the string representation of the
        /// corresponding arguments, formatted using invariant culture.</returns>
        public string StringFormat(object? arg0, object? arg1, object? arg2, object? arg3, object? arg4, object? arg5)
        {
            ArgumentNullException.ThrowIfNull(value);

            return string.Format(CultureInfo.InvariantCulture, value, arg0, arg1, arg2, arg3, arg4, arg5);
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
			MatchCollection parts = TypeNameFormaterRegex().Matches(value)
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
