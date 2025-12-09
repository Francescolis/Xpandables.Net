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
using System.IO.Pipelines;
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
        /// Deserializes the HTTP JSON content into an <see cref="IAsyncPagedEnumerable{T}"/> using
        /// the supplied <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="options">The options to use when deserializing the JSON content.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the paged data.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(
            JsonSerializerOptions options,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(options);

            PipeReader reader = PipeReader.Create(GetContentStream(content));
            return JsonSerializer.DeserializeAsyncPagedEnumerable<TValue>(reader, options, strategy, cancellationToken);
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

            PipeReader reader = PipeReader.Create(GetContentStream(content));
            return JsonSerializer.DeserializeAsyncPagedEnumerable(reader, jsonTypeInfo, strategy, cancellationToken);
        }
    }

    internal static ValueTask<Stream> GetContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        Task<Stream> task = ReadHttpContentStreamAsync(content, cancellationToken);

        return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
            ? GetTranscodingStreamAsync(task, sourceEncoding)
            : new(task);
    }

    internal static Stream GetContentStream(HttpContent content)
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

    private static Stream ReadHttpContentStream(HttpContent content)
    {
        return content.ReadAsStream();
    }

    private static Task<Stream> ReadHttpContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        return content.ReadAsStreamAsync(cancellationToken);
    }

    private static Stream GetTranscodingStream(Stream contentStream, Encoding sourceEncoding)
    {
        return Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);
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