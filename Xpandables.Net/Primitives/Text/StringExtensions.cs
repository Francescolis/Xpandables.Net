
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
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Primitives.Text;

/// <summary>
/// Provides a set of <see langword="static"/> methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
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
    public static T? DeserializeAnonymousType<T>(
        this string json,
        T _,
        JsonSerializerOptions? options = default)
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
    public static ValueTask<T?> DeserializeAnonymousTypeAsync<T>(
        this Stream stream,
        T _,
        JsonSerializerOptions? options = default,
        CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);

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
    public static string ToJsonString<T>(
        this T source,
        JsonSerializerOptions? options = default)
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

    /// <summary>
    /// Tries to convert a string to the specified value type.
    /// </summary>
    /// <typeparam name="TResult">Type source.</typeparam>
    /// <param name="value">The string value.</param>
    /// <param name="result">The string value converted to the specified value 
    /// type.</param>
    /// <param name="valueTypeException">The handled exception during conversion
    /// .</param>
    /// <returns>Returns <see langword="true"/> if conversion OK and 
    /// <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> 
    /// is null or empty.</exception>
    public static bool TryToValueType<TResult>(
        this string value,
        [MaybeNullWhen(returnValue: false)] out TResult result,
        [MaybeNullWhen(returnValue: true)] out Exception valueTypeException)
        where TResult :
        struct, IComparable, IFormattable, IConvertible,
        IComparable<TResult>, IEquatable<TResult>
    {
        try
        {
            valueTypeException = default;
            result = (TResult)Convert
                .ChangeType(value, typeof(TResult), CultureInfo.CurrentCulture);

            return true;
        }
        catch (Exception exception)
            when (exception is InvalidCastException
                            or FormatException
                            or OverflowException)
        {
            valueTypeException = exception;
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to parse the text representing a single JSON value into a 
    /// <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <param name="returnObject">The instance of the object if successful 
    /// otherwise null.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryDeserialize<T>(
        this string json,
        JsonSerializerOptions options,
        [MaybeNullWhen(returnValue: false)] out object returnObject,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
        => json
        .TryDeserialize(typeof(T), options, out returnObject, out exception);

    /// <summary>
    /// Tries to parse the text representing a single JSON value into a 
    /// <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="returnObject">The instance of the object if successful 
    /// otherwise null.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryDeserialize<T>(
        this string json,
        [MaybeNullWhen(returnValue: false)] out object returnObject,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
        => json.TryDeserialize(typeof(T), out returnObject, out exception);

    /// <summary>
    /// Tries to parse the text representing a single JSON value into a 
    /// <paramref name="returnType"/>.
    /// </summary>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and 
    /// return.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <param name="returnObject">The instance of the object if successful
    /// otherwise null.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryDeserialize(
        this string json,
        Type returnType,
        JsonSerializerOptions options,
        [MaybeNullWhen(returnValue: false)] out object returnObject,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
    {
        try
        {
            exception = default;
            returnObject = JsonSerializer.Deserialize(json, returnType, options);
            return returnObject is not null;
        }
        catch (Exception ex)
            when (ex is ArgumentNullException
                    or JsonException
                    or NotSupportedException)
        {
            exception = ex;
            returnObject = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to parse the text representing a single JSON value into a 
    /// <paramref name="returnType"/>.
    /// </summary>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and 
    /// return.</param>
    /// <param name="returnObject">The instance of the object if successful 
    /// otherwise null.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryDeserialize(
        this string json,
        Type returnType,
        [MaybeNullWhen(returnValue: false)] out object returnObject,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
    {
        try
        {
            exception = default;
            returnObject = JsonSerializer.Deserialize(json, returnType);
            return returnObject is not null;
        }
        catch (Exception ex)
            when (ex is ArgumentNullException
                    or JsonException
                    or NotSupportedException)
        {
            exception = ex;
            returnObject = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to convert the provided value into a <see cref="string"/>.
    /// </summary>
    /// <param name="value">JSON text to parse.</param>
    /// <param name="inputType">The type of the <paramref name="value"/> 
    /// to convert.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <param name="returnString">A <see cref="string"/> representation of 
    /// the value.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if serialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TrySerialize(
        this object value,
        Type inputType,
        JsonSerializerOptions options,
        [MaybeNullWhen(returnValue: false)] out string returnString,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
    {
        try
        {
            exception = default;
            returnString = JsonSerializer.Serialize(value, inputType, options);
            return returnString is not null;
        }
        catch (Exception ex)
            when (ex is ArgumentNullException
                    or ArgumentException
                    or NotSupportedException)
        {
            exception = ex;
            returnString = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to convert the provided value into a <see cref="string"/>.
    /// </summary>
    /// <param name="value">JSON text to parse.</param>
    /// <param name="inputType">The type of the <paramref name="value"/> 
    /// to convert.</param>
    /// <param name="returnString">A <see cref="string"/> representation of 
    /// the value.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if serialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TrySerialize(
        this object value,
        Type inputType,
        [MaybeNullWhen(returnValue: false)] out string returnString,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
    {
        try
        {
            exception = default;
            returnString = JsonSerializer.Serialize(value, inputType);
            return returnString is not null;
        }
        catch (Exception ex)
            when (ex is ArgumentNullException
                    or ArgumentException
                    or NotSupportedException)
        {
            exception = ex;
            returnString = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to convert the provided value into a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <param name="returnString">A <see cref="string"/> representation of 
    /// the value.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TrySerialize<T>(
        this object value,
        JsonSerializerOptions options,
        [MaybeNullWhen(returnValue: false)] out string returnString,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
        => value
        .TrySerialize(typeof(T), options, out returnString, out exception);

    /// <summary>
    /// Tries to convert the provided value into a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">JSON text to parse.</param>
    /// <param name="returnString">A <see cref="string"/> representation of 
    /// the value.</param>
    /// <param name="exception">The handler exception during deserialization
    /// .</param>
    /// <returns>Returns <see langword="true"/> if deserialization is OK and 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TrySerialize<T>(
        this object value,
        [MaybeNullWhen(returnValue: false)] out string returnString,
        [MaybeNullWhen(returnValue: true)] out Exception exception)
        => value.TrySerialize(typeof(T), out returnString, out exception);

    /// <summary>
    /// Adds a query string to the given path.
    /// </summary>
    /// <param name="path">The path to act on.</param>
    /// <param name="queryString">The collection of keys values to be added
    /// .</param>
    /// <returns>The path with the query string added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if 
    /// <paramref name="path"/> is <see langword="null"/>.</exception>
    public static string AddQueryString(
        this string path,
        IDictionary<string, string?>? queryString)
    {
        // From MS internal code
        ArgumentNullException.ThrowIfNull(path);

        if (queryString is null)
            return path;

        int anchorIndex = path.IndexOf('#', StringComparison.InvariantCulture);
        string uriToBeAppended = path;
        string anchorText = "";

        // If there is an anchor, then the query string must be inserted
        // before its first occurrence.
        if (anchorIndex != -1)
        {
            anchorText = path[anchorIndex..];
            uriToBeAppended = path[..anchorIndex];
        }

#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
        int queryIndex = uriToBeAppended
            .IndexOf('?', StringComparison.InvariantCulture);
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
        bool hasQuery = queryIndex != -1;

        StringBuilder sb = new();
        _ = sb.Append(uriToBeAppended);
        foreach (KeyValuePair<string, string?> parameter in queryString)
        {
            _ = sb.Append(hasQuery ? '&' : '?');
            _ = sb.Append(UrlEncoder.Default.Encode(parameter.Key));
            _ = sb.Append('=');
            _ = sb.Append(parameter.Value is null
                ? null
                : UrlEncoder.Default.Encode(parameter.Value));
            hasQuery = true;
        }

        _ = sb.Append(anchorText);
        return sb.ToString();
    }

    /// <summary>
    /// Converts the current document to a <see cref="JsonDocument"/> using 
    /// the specified options.
    /// </summary>
    /// <typeparam name="TDocument">The type of document.</typeparam>
    /// <param name="document">An instance of document to parse.</param>
    /// <param name="serializerOptions">Options to control the behavior during 
    /// parsing.</param>
    /// <param name="documentOptions">Options to control the reader behavior 
    /// during parsing.</param>
    /// <returns>An instance of <see cref="JsonDocument"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/>
    /// is null.</exception>   
    /// <exception cref="ArgumentException">The <paramref name="documentOptions"/> 
    /// contains unsupported options.</exception>
    /// <exception cref="InvalidOperationException">The JSON is invalid. -or-
    /// contentType is not compatible with the JSON. -or- There is remaining
    /// string in the span beyond a single JSON value.</exception>
    public static JsonDocument ToJsonDocument<TDocument>(
        this TDocument document,
        JsonSerializerOptions? serializerOptions = default,
        JsonDocumentOptions documentOptions = default)
        where TDocument : notnull
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));

        try
        {
            string documentString = JsonSerializer.Serialize(
          document,
          document.GetType(),
          serializerOptions);

            return JsonDocument.Parse(documentString, documentOptions);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.JsonDocumentFailedToParse
                .StringFormat(document.GetType().Name), exception);
        }
    }

    /// <summary>
    /// Deserializes the current JSON element to an object of specified type.
    /// </summary>
    /// <param name="element">The JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and 
    /// return.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <returns>A returnType representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="returnType"/> 
    /// is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of 
    /// this value is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent 
    /// System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is
    /// not compatible with the JSON. -or- There is remaining data in the span 
    /// beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible 
    /// System.Text.Json.Serialization.JsonConverter 
    /// for returnType or its serializable members.</exception>
    public static object? ToObject(
        this JsonElement element,
        Type returnType,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(returnType);

        ArrayBufferWriter<byte> bufferWriter = new();
        using Utf8JsonWriter writer = new(bufferWriter);
        element.WriteTo(writer);
        writer.Flush();

        return JsonSerializer
            .Deserialize(bufferWriter.WrittenSpan, returnType, options);
    }

    /// <summary>
    /// Deserializes the current JSON document to an object of specified type.
    /// </summary>
    /// <param name="document">The JSON document to parse.</param>
    /// <param name="returnType">The type of the object to convert to and 
    /// return.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <returns>A returnType representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="returnType"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> 
    /// is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of 
    /// this value 
    /// is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent 
    /// System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is 
    /// not compatible with the JSON. -or- There is remaining data in the span 
    /// beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no 
    /// compatible System.Text.Json.Serialization.JsonConverter for returnType 
    /// or its serializable members.</exception>
    public static object? ToObject(
        this JsonDocument document,
        Type returnType,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.RootElement.ToObject(returnType, options);
    }

    /// <summary>
    /// Deserializes the current JSON document to an object of specified 
    /// <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The target type of the UTF-8 encoded text
    /// .</typeparam>
    /// <param name="document">The JSON document to parse.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value
    /// .</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/>
    /// is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind 
    /// of this value is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent 
    /// System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is 
    /// not compatible with the JSON. -or- There is remaining data in the span 
    /// beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no 
    /// compatible System.Text.Json.Serialization.JsonConverter for returnType 
    /// or its serializable members.</exception>
    public static T? ToObject<T>(
        this JsonDocument document,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.RootElement.ToObject<T>(options);
    }

    /// <summary>
    /// Deserializes the current JSON element to an object of specified 
    /// <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The target type of the UTF-8 encoded text
    /// .</typeparam>
    /// <param name="element">The JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing
    /// .</param>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value
    /// .</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="element"/> 
    /// is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind 
    /// of this 
    /// value is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent 
    /// System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is 
    /// not compatible with the JSON. -or- There is remaining data in the span 
    /// beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no 
    /// compatible System.Text.Json.Serialization.JsonConverter for returnType 
    /// or its serializable members.</exception>
    public static T? ToObject<T>(
        this JsonElement element,
        JsonSerializerOptions? options = default)
        => element.ToObject(typeof(T), options) is { } result
            ? (T)result
            : default;

    /// <summary>
    /// Converts the current <see cref="JsonDocument"/> to a string.
    /// </summary>
    /// <param name="document">An instance of document to parse.</param>
    /// <returns>A string representation of the <see cref="JsonDocument"/> 
    /// instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> 
    /// is null.</exception>
    public static string ToJsonString(this JsonDocument document)
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (MemoryStream stream = new())
        using (Utf8JsonWriter writer = new(
            stream,
            new JsonWriterOptions { Indented = true }))
        {
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
#pragma warning restore IDE0063 // Use simple 'using' statement
    }
}
