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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Net.Http.Json;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for <see cref="HttpContent"/> that deserialize JSON payloads
/// into <see cref="IAsyncPagedEnumerable{T}"/> allowing incremental, paged consumption of
/// large responses without buffering the entire content in memory.
/// </summary>
public static class HttpContentExtensions
{
    /// <summary>
    /// Extension methods for <see cref="HttpContent"/> that deserialize JSON payloads
    /// into <see cref="IAsyncPagedEnumerable{T}"/>.
    /// </summary>
    /// <param name="content">The HTTP content to extend.</param>
    extension(HttpContent content)
    {
        /// <summary>
        /// Reads the HTTP content as an asynchronous paged enumerable of JSON values of the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type of the elements to deserialize from the JSON content.</typeparam>
        /// <param name="strategy">The pagination strategy to use when reading the content. Specifies how the content should be split into
        /// pages. The default is PaginationStrategy.None.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized values of type TValue from the JSON content. The
        /// enumerable may be empty if the content contains no items.</returns>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            return content.ReadFromJsonAsAsyncPagedEnumerable<TValue>(options: null, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes the HTTP JSON content into an <see cref="IAsyncPagedEnumerable{T}"/> using
        /// the supplied <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="options">The options to use when deserializing the JSON content.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the paged data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            JsonSerializerOptions? options,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);

            return ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(content, options, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes the HTTP JSON content into an <see cref="IAsyncPagedEnumerable{T}"/> using
        /// the provided <see cref="JsonTypeInfo{T}"/> metadata.
        /// </summary>
        /// <param name="jsonTypeInfo">The type metadata to use when deserializing the JSON content.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the paged data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return ReadFromJsonAsAsyncPagedEnumerableCore(content, jsonTypeInfo, strategy, cancellationToken);
        }
    }

    [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
    private static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(
        HttpContent content,
        JsonSerializerOptions? options,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {
        var jsonTypeInfo = (JsonTypeInfo<TValue>)GetJsonTypeInfo(typeof(TValue), options);
        return ReadFromJsonAsAsyncPagedEnumerableCore(content, jsonTypeInfo, strategy, cancellationToken);
    }

    private static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerableCore<TValue>(
        HttpContent content,
        JsonTypeInfo<TValue> jsonTypeInfo,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {
        return AsyncPagedEnumerable.Create(async ct =>
        {
            Stream contentStream = await GetContentStreamAsync(content, ct)
                .ConfigureAwait(false);

            return JsonSerializer.DeserializeAsyncPagedEnumerable(
                contentStream, jsonTypeInfo, strategy, ct);
        });
    }

    private static ValueTask<Stream> GetContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        Task<Stream> task = ReadHttpContentStreamAsync(content, cancellationToken);

        return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? GetTranscodingStreamAsync(task, sourceEncoding)
            : new(task);
    }

    private static Stream GetContentStream(HttpContent content)
    {
        Stream stream = ReadHttpContentStream(content);

        return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? GetTranscodingStream(stream, sourceEncoding)
            : stream;
    }

    private static async ValueTask<Stream> GetTranscodingStreamAsync(Task<Stream> task, Encoding sourceEncoding)
    {
        Stream contentStream = await task.ConfigureAwait(false);

        // Wrap content stream into a transcoding stream that buffers the data transcoded from the sourceEncoding to utf-8.
        return GetTranscodingStream(contentStream, sourceEncoding);
    }

    private static Stream ReadHttpContentStream(HttpContent content) => content.ReadAsStream();

    private static Task<Stream> ReadHttpContentStreamAsync(HttpContent content, CancellationToken cancellationToken) =>
        content.ReadAsStreamAsync(cancellationToken);

    private static Stream GetTranscodingStream(Stream contentStream, Encoding sourceEncoding) =>
        Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializerOptions.Web")]
    internal static JsonTypeInfo GetJsonTypeInfo(Type type, JsonSerializerOptions? options)
    {
        Debug.Assert(type is not null);

        // Resolves JsonTypeInfo metadata using the appropriate JsonSerializerOptions configuration,
        // following the semantics of the JsonSerializer reflection methods.
        options ??= JsonSerializerOptions.Web;
        options.MakeReadOnly(populateMissingResolver: true);
        return options.GetTypeInfo(type);
    }

    internal const string DefaultMediaType = "application/json; charset=utf-8";

    internal static Encoding? GetEncoding(HttpContent content)
    {
        Encoding? encoding = null;

        if (content.Headers.ContentType?.CharSet is string charset)
        {
            try
            {
                // Remove at most a single set of quotes.
                if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
                {
                    encoding = Encoding.GetEncoding(charset[1..^1]);
                }
                else
                {
                    encoding = Encoding.GetEncoding(charset);
                }
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException("Charset is invalid", e);
            }

            Debug.Assert(encoding != null);
        }

        return encoding;
    }
}