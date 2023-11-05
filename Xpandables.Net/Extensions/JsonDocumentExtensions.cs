/************************************************************************************************************
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
************************************************************************************************************/
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="JsonDocument"/>.
/// </summary>
public static partial class XpandablesExtensions
{
    /// <summary>
    /// Converts the current document to a <see cref="JsonDocument"/> using the specified options.
    /// </summary>
    /// <typeparam name="TDocument">The type of document.</typeparam>
    /// <param name="document">An instance of document to parse.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <param name="documentOptions">Options to control the reader behavior during parsing.</param>
    /// <returns>An instance of <see cref="JsonDocument"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is null.</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="JsonConverter"/>
    /// for <typeparamref name="TDocument"/> 
    /// or its serializable members.</exception>
    /// <exception cref="ArgumentException">The <paramref name="documentOptions"/> 
    /// contains unsupported options.</exception>
    public static JsonDocument ToJsonDocument<TDocument>(
        this TDocument document,
        JsonSerializerOptions? serializerOptions = default,
        JsonDocumentOptions documentOptions = default)
        where TDocument : notnull
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));

        string documentString = JsonSerializer.Serialize(
            document,
            document.GetType(),
            serializerOptions);

        return JsonDocument.Parse(documentString, documentOptions);
    }

    /// <summary>
    /// Deserializes the current JSON element to an object of specified type.
    /// </summary>
    /// <param name="element">The JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and return.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>A returnType representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="returnType"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of this value 
    /// is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is not compatible with the JSON. -or- 
    /// There is remaining data in the span beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible System.Text.Json.Serialization.JsonConverter 
    /// for returnType or its serializable members.</exception>
    public static object? ToObject(
        this JsonElement element,
        Type returnType,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(returnType);

        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        element.WriteTo(writer);
        writer.Flush();

        return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, returnType, options);
    }

    /// <summary>
    /// Deserializes the current JSON document to an object of specified type.
    /// </summary>
    /// <param name="document">The JSON document to parse.</param>
    /// <param name="returnType">The type of the object to convert to and return.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>A returnType representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="returnType"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of this value 
    /// is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is not compatible with the JSON. -or- 
    /// There is remaining data in the span beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible System.Text.Json.Serialization.JsonConverter 
    /// for returnType or its serializable members.</exception>
    public static object? ToObject(
        this JsonDocument document,
        Type returnType,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.RootElement.ToObject(returnType, options);
    }

    /// <summary>
    /// Deserializes the current JSON document to an object of specified <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The target type of the UTF-8 encoded text.</typeparam>
    /// <param name="document">The JSON document to parse.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of this 
    /// value is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is not compatible with the JSON. -or- 
    /// There is remaining data in the span beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible System.Text.Json.Serialization.JsonConverter 
    /// for returnType or its serializable members.</exception>
    public static T? ToObject<T>(
        this JsonDocument document,
        JsonSerializerOptions? options = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.RootElement.ToObject<T>(options);
    }

    /// <summary>
    /// Deserializes the current JSON element to an object of specified <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The target type of the UTF-8 encoded text.</typeparam>
    /// <param name="element">The JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="element"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The JsonElement.ValueKind of this 
    /// value is System.Text.Json.JsonValueKind.Undefined.</exception>
    /// <exception cref="ObjectDisposedException">The parent System.Text.Json.JsonDocument has been disposed.</exception>
    /// <exception cref="JsonException">The JSON is invalid. -or- returnType is not compatible with the JSON. -or- 
    /// There is remaining data in the span beyond a single JSON value.</exception>
    /// <exception cref="NotSupportedException">There is no compatible System.Text.Json.Serialization.JsonConverter 
    /// for returnType or its serializable members.</exception>
    public static T? ToObject<T>(
        this JsonElement element,
        JsonSerializerOptions? options = default)
        => element.ToObject(typeof(T), options) is { } result ? (T)result : default;

    /// <summary>
    /// Converts the current <see cref="JsonDocument"/> to a string.
    /// </summary>
    /// <param name="document">An instance of document to parse.</param>
    /// <returns>A string representation of the <see cref="JsonDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is null.</exception>
    public static string ToJsonString(this JsonDocument document)
    {
        _ = document ?? throw new ArgumentNullException(nameof(document));
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (var stream = new MemoryStream())
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
#pragma warning restore IDE0063 // Use simple 'using' statement
    }

}
