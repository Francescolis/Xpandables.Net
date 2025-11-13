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
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xpandables.Net.Collections.Generic;

namespace Xpandables.Net.Collections.Extensions;

/// <summary>
/// Provides extension methods for reading HTTP content as an asynchronous paged enumerable of JSON objects.
/// </summary>
/// <remarks>These extension methods enable efficient, asynchronous processing of large JSON payloads returned
/// from HTTP responses by exposing the items as an <see cref="IAsyncPagedEnumerable{T}"/>. This is particularly useful when working
/// with paged or streaming JSON APIs, as it allows consuming items incrementally without loading the entire response
/// into memory.</remarks>
public static class HttpContentExtensions
{
    /// <summary>
    /// Reads the HTTP content as an asynchronous paged enumerable of JSON objects.
    /// </summary>
    extension(HttpContent content)
    {
        /// <summary>
        /// Deserializes the HTTP content as a paged asynchronous enumerable of JSON objects of type <typeparamref
        /// name="TValue"/>.
        /// </summary>
        /// <remarks>This method requires the HTTP content to be in a paged JSON format compatible with
        /// the deserialization process. The operation may require dynamic code generation and types that are not
        /// statically referenced, which can affect trimming and AOT scenarios.</remarks>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON content.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="IAsyncPagedEnumerable{TValue}"/> that yields deserialized objects from the JSON content, one page at a
        /// time.</returns>
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public Task<IAsyncPagedEnumerable<TValue?>> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            return ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(content, options: null, cancellationToken);
        }

        /// <summary>
        /// Deserializes the HTTP content as an asynchronous paged enumerable of JSON objects of type TValue.
        /// </summary>
        /// <remarks>This method throws an ArgumentNullException if the HTTP content is null. The returned
        /// enumerable allows for efficient, paged processing of large JSON payloads without loading all items into
        /// memory at once.</remarks>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON content.</typeparam>
        /// <param name="options">The options to use when deserializing the JSON content. If null, default serialization options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains an IAsyncPagedEnumerable of TValue
        /// objects, which can be enumerated asynchronously in pages.</returns>
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public Task<IAsyncPagedEnumerable<TValue?>> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            JsonSerializerOptions? options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            return ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(content, options, cancellationToken);
        }

        /// <summary>
        /// Reads the HTTP content as a paged asynchronous enumerable of JSON objects of the specified type.
        /// </summary>
        /// <remarks>This method uses the provided JsonTypeInfo to ensure correct serialization behavior
        /// and compatibility with trimming and ahead-of-time (AOT) scenarios. The returned enumerable allows efficient
        /// processing of large or paged JSON responses without loading all items into memory at once.</remarks>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON content.</typeparam>
        /// <param name="jsonTypeInfo">The metadata used to control JSON serialization and deserialization for the specified type.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains an asynchronous paged enumerable of
        /// deserialized objects of type TValue. The enumerable may be empty if no items are found.</returns>
        public Task<IAsyncPagedEnumerable<TValue?>> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return ReadFromJsonAsAsyncPagedEnumerableCore(content, jsonTypeInfo, cancellationToken);
        }
    }

    /// <summary>
    /// Core implementation for reading HTTP content as async paged enumerable with JsonSerializerOptions.
    /// </summary>
    [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
    [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
    private static async Task<IAsyncPagedEnumerable<TValue?>> ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(
        HttpContent content,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        PipeReader pipeReader = await GetContentStreamAsPipeReaderAsync(content, cancellationToken).ConfigureAwait(false);

        return JsonSerializer.DeserializeAsyncPagedEnumerable<TValue>(
            pipeReader,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Core implementation for reading HTTP content as async paged enumerable with JsonTypeInfo.
    /// </summary>
    private static async Task<IAsyncPagedEnumerable<TValue?>> ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(
        HttpContent content,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        PipeReader pipeReader = await GetContentStreamAsPipeReaderAsync(content, cancellationToken).ConfigureAwait(false);

        return JsonSerializer.DeserializeAsyncPagedEnumerable(
            pipeReader,
            jsonTypeInfo,
            cancellationToken);
    }

    /// <summary>
    /// Gets the content stream with proper encoding handling.
    /// </summary>
    internal static async ValueTask<PipeReader> GetContentStreamAsPipeReaderAsync(HttpContent content, CancellationToken cancellationToken)
    {
        Task<Stream> task = ReadHttpContentStreamAsync(content, cancellationToken);

        var stream = GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? await GetTranscodingStreamAsync(task, sourceEncoding).ConfigureAwait(false)
            : await task.ConfigureAwait(false);

        return PipeReader.Create(stream);
    }

    private static Task<Stream> ReadHttpContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        return content.ReadAsStreamAsync(cancellationToken);
    }

    private static async ValueTask<Stream> GetTranscodingStreamAsync(Task<Stream> task, Encoding sourceEncoding)
    {
        Stream contentStream = await task.ConfigureAwait(false);
        return Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Encoding parsing should not fail the operation")]
    private static Encoding? GetEncoding(HttpContent content)
    {
        string? charset = content.Headers.ContentType?.CharSet;
        if (string.IsNullOrEmpty(charset))
        {
            return null;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch
        {
            return null;
        }
    }
}