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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Primtives;

/// <summary>
/// Provides extension methods for common abstraction, conversion, formatting, and serialization operations, including
/// type-safe casting, conditional execution, exception handling, and JSON serialization/deserialization.
/// </summary>
/// <remarks>This static class contains a variety of utility extension methods designed to simplify common
/// programming patterns, such as safely casting objects, formatting strings, handling exceptions in functional and
/// asynchronous code, and working with JSON serialization and deserialization. The methods are intended to improve code
/// readability and reduce boilerplate in application and library development. All methods are implemented as extension
/// methods for ease of use with existing .NET types.</remarks>
public static class ThrowExceptionExtensions
{
    /// <summary>
    /// Throws an InvalidOperationException if the provided function throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the function 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static T ThrowInvalidOperationException<T>(this Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return func();
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an InvalidOperationException if the provided Task throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the Task.</typeparam>
    /// <param name="func">The Task to execute.</param>
    /// <returns>The original Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Task 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static Task<T> ThrowInvalidOperationException<T>(this Task<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return func;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Throws an InvalidOperationException if the provided Task throws an 
    /// exception that is not an InvalidOperationException.
    /// </summary>
    /// <param name="func">The Task to execute.</param>
    /// <returns>The original Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Task 
    /// throws an exception that is not an InvalidOperationException.</exception>
    public static Task ThrowInvalidOperationException(this Task func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return func;
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>
    /// Retrieves the full message from an exception, including all inner 
    /// exceptions.
    /// </summary>
    /// <param name="exception">The exception to get the full message from.</param>
    /// <returns>A string containing the full message of the exception and i
    /// ts inner exceptions.</returns>
    public static string GetCompleteExceptionMessage(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        StringBuilder message = new();
        _ = message.AppendLine(exception.Message);

        while (exception.InnerException is not null)
        {
            exception = exception.InnerException;
            _ = message.AppendLine(exception.Message);
        }

        return message.ToString();
    }

    /// <summary>
    /// Changes the type of the given value to the specified conversion type, 
    /// handling nullable types.
    /// </summary>
    /// <param name="value">The value to change the type of.</param>
    /// <param name="conversionType">The type to convert the value to.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>The value converted to the specified type, or null if the 
    /// value is null.</returns>
    public static object? ChangeTypeNullable(
        this object? value,
        Type conversionType,
        IFormatProvider? formatProvider = null)
    {
        if (value is null)
        {
            return null;
        }

        Type targetType = conversionType
            ?? throw new ArgumentNullException(nameof(conversionType));

        if (!conversionType.IsGenericType
            || conversionType.GetGenericTypeDefinition() != typeof(Nullable<>))
        {
            return Convert.ChangeType(value, targetType, formatProvider);
        }

        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType is null)
        {
            return null;
        }

        targetType = underlyingType;

        return Convert.ChangeType(value, targetType, formatProvider);
    }

    /// <summary>
    /// Changes the type of the given value to the specified conversion type, 
    /// handling nullable types.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="value">The value to change the type of.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>The value converted to the specified type, or null if the 
    /// value is null.</returns>
    public static T? ChangeTypeNullable<T>(
        this object? value,
        IFormatProvider? formatProvider = null)
    {
        if (value is null)
        {
            return default;
        }

        return value.ChangeTypeNullable(typeof(T), formatProvider) is T instance
            ? instance : default;
    }

    /// <summary>
    /// Executes the provided function if the condition is true, otherwise 
    /// returns the original object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to potentially modify.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="func">The function to execute if the condition is true.</param>
    /// <returns>The modified object if the condition is true, otherwise the 
    /// original object.</returns>
    public static T When<T>(this T obj, bool condition, Func<T, T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(obj);

        return condition ? func(obj) : obj;
    }

    /// <summary>
    /// Executes the provided action if the condition is true, otherwise 
    /// returns the original object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to potentially modify.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="action">The action to execute if the condition is true.</param>
    /// <returns>The original object.</returns>
    public static T When<T>(this T obj, bool condition, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(obj);

        if (condition)
        {
            action(obj);
        }

        return obj;
    }

    /// <summary>
    /// Attempts to cast the object to the specified type, returning null if
    /// not possible.
    /// </summary>
    /// <typeparam name="T">The type to cast the object to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The object cast to the specified type, or null if not 
    /// possible.</returns>
    public static T? As<T>(this object? obj)
        where T : class => obj is T t ? t : null;

    /// <summary>
    /// Attempts to cast the object to the specified type, returning null if
    /// not possible.
    /// </summary>
    /// <typeparam name="T">The type to cast the object to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <param name="_">The type to cast the object to.</param>
    /// <returns>The object cast to the specified type, or null if
    /// not possible.</returns>
    public static T? As<T>(this object? obj, T _)
        where T : class => obj is T t ? t : null;

    /// <summary>
    /// Attempts to cast the object to the specified type, throwing an
    /// exception if not possible.
    /// </summary>
    /// <typeparam name="T">The type to cast the object to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The object cast to the specified type.</returns>
    /// <exception cref="InvalidCastException">Thrown when the object cannot
    /// cast to the specified type.</exception>
    public static T AsRequired<T>(this object obj)
        where T : class => (T)obj;

    /// <summary>
    /// Replaces the argument object into the current text equivalent
    /// using the default <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    /// <param name="value">The format string.</param>
    /// <param name="args">The object to be formatted.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null
    /// or <paramref name="args"/> is null.</exception>
    /// <exception cref="FormatException">The format is invalid.</exception>
    /// <returns>value <see cref="string"/> filled with 
    /// <paramref name="args"/>.</returns>
    public static string StringFormat(this string value, params object[] args)
        => value.StringFormat(CultureInfo.InvariantCulture, args);

    /// <summary>
    /// Replaces the argument object into the current text equivalent using the 
    /// specified culture.
    /// </summary>
    /// <param name="value">The format string.</param>
    /// <param name="cultureInfo">CultureInfo to be used.</param>
    /// <param name="args">The object to be formatted.</param>
    /// <returns>value <see cref="string"/> filled with 
    /// <paramref name="args"/></returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> 
    /// or <paramref name="cultureInfo"/> or <paramref name="args"/> is null
    /// .</exception>    
    /// <exception cref="FormatException">The format is invalid.</exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string StringFormat(
        this string value,
        CultureInfo cultureInfo,
        params object[] args)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        return string.Format(cultureInfo, value, args);
    }

    /// <summary>
    /// Parses the text representing a single JSON value into an instance of the 
    /// anonymous type specified by a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The anonymous type.</typeparam>
    /// <param name="json">The JSON data to parse.</param>
    /// <param name="_">The anonymous instance.</param>
    /// <param name="options">Options to control the behavior during 
    /// parsing.</param>
    /// <returns> A T representation of the JSON value.</returns>
    /// <exception cref="JsonException">The JSON is invalid or T is not 
    /// compatible with the JSON or There is remaining data in the string 
    /// beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible 
    /// <see cref="JsonConverter"/> for T or its serializable 
    /// members.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="json"/> 
    /// is null.</exception>
    [RequiresUnreferencedCode("This method may not work correctly if the type T is not fully referenced.")]
    [RequiresDynamicCode("This method may not work correctly if the type T is not fully referenced.")]
    public static T? DeserializeAnonymousType<T>(
        this string json,
        T _,
        JsonSerializerOptions? options = null)
         => JsonSerializer.Deserialize<T>(json, options);

    /// <summary>
    /// Asynchronously reads the UTF-8 encoded text representing a single JSON 
    /// value into an instance of an anonymous type specified by a generic type 
    /// parameter. The stream will be read to completion.
    /// </summary>
    /// <typeparam name="T">The anonymous type.</typeparam>
    /// <param name="stream">The JSON data to parse.</param>
    /// <param name="_">The anonymous instance.</param>
    /// <param name="options">Options to control the behavior during 
    /// parsing.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <returns> A T representation of the JSON value.</returns>
    /// <exception cref="JsonException">The JSON is invalid or T is not 
    /// compatible with the JSON or There is remaining data in the 
    /// stream.</exception>
    /// <exception cref="NotSupportedException">There is no compatible 
    /// <see cref="JsonConverter"/> for T or its serializable 
    /// members.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> 
    /// is null.</exception>
    [RequiresUnreferencedCode("This method may not work correctly if the type T is not fully referenced.")]
    [RequiresDynamicCode("This method may not work correctly if the type T is not fully referenced.")]
    public static Task<T?> DeserializeAnonymousTypeAsync<T>(
        this Stream stream,
        T _,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
        => JsonSerializer
            .DeserializeAsync<T>(stream, options, cancellationToken)
            .AsTask();

    /// <summary>
    /// Deserializes a stream into an asynchronous enumerable collection of a specified type.
    /// </summary>
    /// <typeparam name="T">Specifies the type of objects contained in the asynchronous enumerable collection.</typeparam>
    /// <param name="stream">Represents the input stream from which data will be deserialized.</param>
    /// <param name="_">An instance of the specified type used to guide the deserialization process.</param>
    /// <param name="options">Provides options to customize the deserialization behavior.</param>
    /// <param name="cancellationToken">Allows for the operation to be canceled if needed.</param>
    /// <returns>An asynchronous enumerable collection of the deserialized objects.</returns>
    [RequiresUnreferencedCode("This method may not work correctly if the type T is not fully referenced.")]
    [RequiresDynamicCode("This method may not work correctly if the type T is not fully referenced.")]
    public static IAsyncEnumerable<T?> DeserializeAsyncEnumerableAsync<T>(
        this Stream stream,
        T _,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options, cancellationToken);

    /// <summary>
    /// Serializes the current instance to JSON string using 
    /// <see cref="System.Text.Json"/>.
    /// </summary>
    /// <param name="source">The object to act on.</param>
    /// <param name="options">The serializer options to be applied.</param>
    /// <returns>A JSOn string representation of the object.</returns>
    /// <exception cref="NotSupportedException">There is no compatible 
    /// System.Text.Json.Serialization.JsonConverter for TValue or its 
    /// serializable members.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentException">inputType is not compatible with 
    /// value.</exception>
    /// <exception cref="NotSupportedException"> There is no compatible 
    /// System.Text.Json.Serialization.JsonConverter for inputType or its 
    /// serializable members.</exception>
    [RequiresUnreferencedCode("This method may not work correctly if the type T is not fully referenced.")]
    [RequiresDynamicCode("This method may not work correctly if the type T is not fully referenced.")]
    public static string ToJsonString<T>(
        this T source,
        JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        return JsonSerializer.Serialize(source, source.GetType(), options);
    }

    /// <summary>
    /// Concatenates all the elements of an <see cref="IEnumerable{T}"/>,
    /// using the specified string separator between each element.
    /// </summary>
    /// <typeparam name="TSource">The generic type parameter.</typeparam>
    /// <param name="collection">The collection to act on.</param>
    /// <param name="separator">The string to use as a separator.
    /// Separator is included in the returned string only if value has more 
    /// than one element.</param>
    /// <returns>A string that consists of the elements in value delimited by 
    /// the separator string. If value is an empty array, the method returns 
    /// String.Empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="collection"/> 
    /// is null.</exception>
    /// <exception cref="OutOfMemoryException">The length of the resulting 
    /// string overflows the maximum allowed length (<see cref="int.MaxValue"/>)
    /// .</exception>
    public static string StringJoin<TSource>(
        this IEnumerable<TSource> collection,
        string separator)
        => string.Join(separator, collection);

    /// <summary>
    /// Concatenates all the elements of an <see cref="IEnumerable{T}"/>,
    /// using the specified char separator between each element.
    /// </summary>
    /// <typeparam name="TSource">The generic type parameter.</typeparam>
    /// <param name="collection">The collection to act on.</param>
    /// <param name="separator">The string to use as a separator.
    /// Separator is included in the returned string only if value has more 
    /// than one element.</param>
    /// <returns>A string that consists of the elements in value delimited by 
    /// the separator string. If value is an empty array, the method returns 
    /// String.Empty.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="collection"/> 
    /// is null.</exception>
    /// <exception cref="OutOfMemoryException">The length of the resulting 
    /// string overflows the maximum allowed length (<see cref="int.MaxValue"/>)
    /// .</exception>
    public static string StringJoin<TSource>(
        this IEnumerable<TSource> collection,
        char separator)
        => string.Join(
            separator.ToString(CultureInfo.InvariantCulture),
            collection);
}
