/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
	/// Deserializes the HTTP JSON content into an <see cref="IAsyncPagedEnumerable{T}"/> using
	/// the provided <see cref="JsonTypeInfo{T}"/> metadata.
	/// </summary>
	/// <param name="content">The HTTP content to read and deserialize.</param>
	/// <param name="jsonTypeInfo">The type metadata to use when deserializing the JSON content.</param>
	/// <param name="strategy">The pagination strategy to use when deserializing the paged data.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	public static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(this HttpContent content,
		JsonTypeInfo<TValue> jsonTypeInfo,
		PaginationStrategy strategy = PaginationStrategy.Manual,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(content);
		ArgumentNullException.ThrowIfNull(jsonTypeInfo);

		return ReadFromJsonAsAsyncPagedEnumerableCore(content, jsonTypeInfo, strategy, cancellationToken);
	}

	/// <summary>
	/// Deserializes the HTTP JSON content into an <see cref="IAsyncPagedEnumerable{T}"/> using
	/// source-generated metadata from the provided <see cref="JsonSerializerContext"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of elements to deserialize.</typeparam>
	/// <param name="content">The HTTP content to read and deserialize.</param>
	/// <param name="context">The source-generated JSON serialization context that contains metadata for <typeparamref name="TValue"/>.</param>
	/// <param name="strategy">The pagination strategy to use when deserializing the paged data.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	public static IAsyncPagedEnumerable<TValue?> ReadFromJsonAsAsyncPagedEnumerable<TValue>(this HttpContent content,
		JsonSerializerContext context,
		PaginationStrategy strategy = PaginationStrategy.Manual,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(content);
		ArgumentNullException.ThrowIfNull(context);

		var jsonTypeInfo = (JsonTypeInfo<TValue>)(context.GetTypeInfo(typeof(TValue))
			?? throw new InvalidOperationException($"The type '{typeof(TValue)}' is not registered in the provided JsonSerializerContext."));

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
			Stream contentStream = await GetContentStreamAsync(content, ct).ConfigureAwait(false);
			var reader = PipeReader.Create(
				contentStream,
				new StreamPipeReaderOptions(leaveOpen: false));

			return JsonSerializer.DeserializeAsyncPagedEnumerable(
				reader, jsonTypeInfo, strategy, ct);
		});
	}

	private static ValueTask<Stream> GetContentStreamAsync(HttpContent content, CancellationToken cancellationToken)
	{
		Task<Stream> task = content.ReadAsStreamAsync(cancellationToken);

		return GetEncoding(content) is Encoding sourceEncoding && sourceEncoding != Encoding.UTF8
			? GetTranscodingStreamAsync(task, sourceEncoding)
			: new(task);
	}

	private static async ValueTask<Stream> GetTranscodingStreamAsync(Task<Stream> task, Encoding sourceEncoding)
	{
		Stream contentStream = await task.ConfigureAwait(false);

		// Wrap content stream into a transcoding stream that buffers the data transcoded from the sourceEncoding to utf-8.
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
				throw new InvalidOperationException($"The character set '{charset}' is not supported.", e);
			}
		}

		return encoding;
	}
}
