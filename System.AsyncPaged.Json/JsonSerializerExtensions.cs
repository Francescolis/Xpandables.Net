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
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

/// <summary>
/// Provides extension methods for the JsonSerializer class to simplify JSON <see cref="IAsyncPagedEnumerable{T}"/> serialization tasks.
/// </summary>
public static class JsonSerializerExtensions
{
	/// <summary>
	/// Extension methods for the <see cref="JsonSerializer"/> class.
	/// </summary>  
	extension(JsonSerializer)
	{
		/// <summary>
		/// Asynchronously serializes the elements of a paged asynchronous enumerable to JSON and writes the result to
		/// the specified PipeWriter.
		/// </summary>
		/// <typeparam name="TValue">The type of the elements in the paged enumerable to serialize.</typeparam>
		/// <param name="utf8Json">The PipeWriter to which the UTF-8 encoded JSON output will be written. Must not be null.</param>
		/// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements will be serialized to JSON. Must not be null.</param>
		/// <param name="jsonTypeInfo">Metadata used to control the JSON serialization of elements of type TValue. Must not be null.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
		/// <returns>A task that represents the asynchronous serialization operation.</returns>
		public static Task SerializeAsyncPaged<TValue>(
			PipeWriter utf8Json,
			IAsyncPagedEnumerable<TValue> pagedEnumerable,
			JsonTypeInfo<TValue> jsonTypeInfo,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(utf8Json);
			ArgumentNullException.ThrowIfNull(pagedEnumerable);
			ArgumentNullException.ThrowIfNull(jsonTypeInfo);

			return SerializeAsyncPagedCoreGenericAsync(
				utf8Json,
				pagedEnumerable,
				jsonTypeInfo,
				cancellationToken);
		}


		/// <summary>
		/// Asynchronously serializes the elements of a paged asynchronous enumerable to the specified stream in UTF-8
		/// encoded JSON format.
		/// </summary>
		/// <typeparam name="TValue">The type of the elements in the paged enumerable to serialize.</typeparam>
		/// <param name="utf8Json">The stream to which the JSON data will be written. The stream must be writable.</param>
		/// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements will be serialized to JSON.</param>
		/// <param name="jsonTypeInfo">Metadata used to control the JSON serialization of elements of type TValue.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
		/// <returns>A task that represents the asynchronous serialization operation.</returns>
		public static Task SerializeAsyncPaged<TValue>(
			Stream utf8Json,
			IAsyncPagedEnumerable<TValue> pagedEnumerable,
			JsonTypeInfo<TValue> jsonTypeInfo,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(utf8Json);
			ArgumentNullException.ThrowIfNull(pagedEnumerable);
			ArgumentNullException.ThrowIfNull(jsonTypeInfo);

			return SerializeAsyncPagedCoreGenericAsync(
				utf8Json,
				pagedEnumerable,
				jsonTypeInfo,
				cancellationToken);
		}

		/// <summary>
		/// Asynchronously serializes a paged asynchronous enumerable to UTF-8 JSON using source-generated metadata from the provided context.
		/// </summary>
		/// <typeparam name="TValue">The type of the elements in the paged enumerable.</typeparam>
		/// <param name="utf8Json">The writer that receives UTF-8 JSON output.</param>
		/// <param name="pagedEnumerable">The asynchronous paged sequence to serialize.</param>
		/// <param name="context">The source-generated serialization context containing metadata for <typeparamref name="TValue"/>.</param>
		/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
		/// <returns>A task representing the asynchronous serialization operation.</returns>
		public static Task SerializeAsyncPaged<TValue>(
			PipeWriter utf8Json,
			IAsyncPagedEnumerable<TValue> pagedEnumerable,
			JsonSerializerContext context,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(utf8Json);
			ArgumentNullException.ThrowIfNull(pagedEnumerable);
			ArgumentNullException.ThrowIfNull(context);

			JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)(context.GetTypeInfo(typeof(TValue))
				?? throw new InvalidOperationException($"The type '{typeof(TValue)}' is not registered in the provided JsonSerializerContext."));

			return SerializeAsyncPagedCoreGenericAsync(
				utf8Json,
				pagedEnumerable,
				jsonTypeInfo,
				cancellationToken);
		}

		/// <summary>
		/// Asynchronously serializes a paged asynchronous enumerable to a stream using source-generated metadata from the provided context.
		/// </summary>
		/// <typeparam name="TValue">The type of the elements in the paged enumerable.</typeparam>
		/// <param name="utf8Json">The stream that receives UTF-8 JSON output.</param>
		/// <param name="pagedEnumerable">The asynchronous paged sequence to serialize.</param>
		/// <param name="context">The source-generated serialization context containing metadata for <typeparamref name="TValue"/>.</param>
		/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
		/// <returns>A task representing the asynchronous serialization operation.</returns>
		public static Task SerializeAsyncPaged<TValue>(
			Stream utf8Json,
			IAsyncPagedEnumerable<TValue> pagedEnumerable,
			JsonSerializerContext context,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(utf8Json);
			ArgumentNullException.ThrowIfNull(pagedEnumerable);
			ArgumentNullException.ThrowIfNull(context);

			JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)(context.GetTypeInfo(typeof(TValue))
				?? throw new InvalidOperationException($"The type '{typeof(TValue)}' is not registered in the provided JsonSerializerContext."));

			return SerializeAsyncPagedCoreGenericAsync(
				utf8Json,
				pagedEnumerable,
				jsonTypeInfo,
				cancellationToken);
		}
	}

	private static async Task SerializeAsyncPagedCoreGenericAsync<T>(
		object output,
		IAsyncPagedEnumerable<T> paged,
		JsonTypeInfo<T> jsonTypeInfo,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		JsonSerializerOptions serializerOptions = jsonTypeInfo.Options;
		JsonWriterOptions writerOptions = new()
		{
			Indented = serializerOptions.WriteIndented,
			Encoder = serializerOptions.Encoder,
			SkipValidation = !serializerOptions.WriteIndented
		};

		PipeWriter pipeWriter;
		bool ownsPipeWriter = false;

		switch (output)
		{
			case PipeWriter providedPipe:
				pipeWriter = providedPipe;
				break;
			case Stream stream:
				pipeWriter = PipeWriter.Create(
					stream,
					new StreamPipeWriterOptions(leaveOpen: true));
				ownsPipeWriter = true;
				break;
			default:
				throw new ArgumentException("Output must be PipeWriter or Stream", nameof(output));
		}

		using Utf8JsonWriter writer = new(pipeWriter, writerOptions);

		Pagination pagination = await paged
			.GetPaginationAsync(cancellationToken)
			.ConfigureAwait(false);

		writer.WriteStartObject();
		writer.WritePropertyName("pagination"u8);
		JsonSerializer.Serialize(writer, pagination, PaginationJsonContext.Default.Pagination);

		writer.WritePropertyName("items"u8);
		writer.WriteStartArray();

		var flushStrategy = FlushStrategy.Create(pagination.TotalCount);
		int itemCount = 0;

		await foreach (T item in paged.WithCancellation(cancellationToken).ConfigureAwait(false))
		{
			cancellationToken.ThrowIfCancellationRequested();

			JsonSerializer.Serialize(writer, item, jsonTypeInfo);
			itemCount++;

			if (flushStrategy.ShouldFlush(itemCount, writer.BytesPending))
			{
				await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		writer.WriteEndArray();
		writer.WriteEndObject();
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

		if (ownsPipeWriter)
		{
			await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
			await pipeWriter.CompleteAsync().ConfigureAwait(false);
		}
	}





	/// <summary>
	/// Provides adaptive flushing strategy based on dataset size and memory pressure.
	/// </summary>
	private readonly struct FlushStrategy
	{
		private const int DefaultBatchSize = 100;
		private const int SmallDatasetBatchSize = 200;
		private const int MediumDatasetBatchSize = 100;
		private const int LargeDatasetBatchSize = 50;
		private const int VeryLargeDatasetBatchSize = 25;
		private const int BytesPendingThreshold = 32_768; // 32KB

		private readonly long _batchSize;

		private FlushStrategy(long batchSize)
		{
			_batchSize = batchSize;
		}

		public static FlushStrategy Create(long? totalCount)
		{
			long batchSize = totalCount switch
			{
				null => DefaultBatchSize,
				< 1_000 => SmallDatasetBatchSize,
				< 10_000 => MediumDatasetBatchSize,
				< 100_000 => LargeDatasetBatchSize,
				_ => VeryLargeDatasetBatchSize
			};

			return new FlushStrategy(batchSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool ShouldFlush(long itemCount, long bytesPending)
		{
			// Flush based on item count OR bytes pending (memory-aware)
			return itemCount % _batchSize == 0 || bytesPending > BytesPendingThreshold;
		}
	}
}
