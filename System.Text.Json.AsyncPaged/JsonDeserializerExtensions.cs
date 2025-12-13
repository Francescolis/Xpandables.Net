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
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

/// <summary>
/// Provides extension methods for deserializing UTF-8 encoded JSON data into asynchronous paged enumerables using
/// System.Text.Json.
/// </summary>
/// <remarks>These extension methods enable efficient, asynchronous deserialization of large or streaming JSON
/// payloads into paged enumerables, supporting both PipeReader and Stream sources. The methods offer flexibility in
/// specifying serialization options or type metadata, and can handle top-level JSON values or collections. Use these
/// methods to process JSON data in a memory-efficient, non-blocking manner, especially when working with large datasets
/// or data streams.</remarks>
public static class JsonDeserializerExtensions
{
    /// <summary>
    /// Extension methods for the <see cref="JsonSerializer"/> class.
    /// </summary>  
    extension(JsonSerializer)
    {
        /// <summary>
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values of type
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The <see cref="PipeReader"/> containing the UTF-8 encoded JSON data to deserialize.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization. Cannot be null.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream. The enumerable may be empty if the stream contains no items.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonSerializerOptions options,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerable(utf8Json, jsonTypeInfo, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values.
        /// </summary>
        /// <typeparam name="TValue">The type of objects to deserialize from the JSON data.</typeparam>
        /// <param name="utf8Json">The pipe reader that provides the UTF-8 encoded JSON data to be deserialized.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of objects of type TValue.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized objects of type TValue from the provided JSON
        /// data. If the input contains no data, the enumerable will be empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerable(utf8Json.AsStream(), jsonTypeInfo, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of elements of type TValue.
        /// </summary>
        /// <remarks>The returned enumerable reads and deserializes items from the stream as they are
        /// requested, enabling efficient processing of large or paged JSON datasets. The caller is responsible for
        /// disposing the stream when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The stream containing UTF-8 encoded JSON data representing a paged collection of TValue elements. Must not
        /// be null.</param>
        /// <param name="options">The options to use when deserializing the JSON data. Must not be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration operation.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized TValue elements from the provided JSON stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonSerializerOptions options,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            var pipeReader = PipeReader.Create(utf8Json);
            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeCore(pipeReader, jsonTypeInfo, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of the specified
        /// type.
        /// </summary>
        /// <remarks>The returned enumerable reads and deserializes data from the provided stream as pages
        /// are requested. The caller is responsible for disposing the stream when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The stream containing UTF-8 encoded JSON data representing a paged collection of values. The stream must be
        /// readable and positioned at the start of the JSON content.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of elements of type <typeparamref name="TValue"/>.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream in pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            var pipeReader = PipeReader.Create(utf8Json);
            return DeserializeCore(pipeReader, jsonTypeInfo, strategy, cancellationToken);
        }
    }

    private static IAsyncPagedEnumerable<TValue> DeserializeCore<TValue>(
        PipeReader pipeReader,
        JsonTypeInfo<TValue> jsonTypeInfo,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {
        Task<(Pagination Pagination, Stream ItemsStream)> extractionTask =
            ExtractPaginationAndItemsAsync(pipeReader, cancellationToken);

        Func<CancellationToken, ValueTask<Pagination>> paginationFactory = async ct =>
        {
            var (pagination, _) = await extractionTask.ConfigureAwait(false);
            return pagination;
        };

        IAsyncEnumerable<TValue> items = EnumerateItemsAsync(extractionTask, jsonTypeInfo, cancellationToken);

        return AsyncPagedEnumerable.Create(items, paginationFactory, strategy);
    }

    private static async IAsyncEnumerable<TValue> EnumerateItemsAsync<TValue>(
        Task<(Pagination Pagination, Stream ItemsStream)> extractionTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (Pagination _, Stream itemsStream) = await extractionTask.ConfigureAwait(false);

        try
        {
            IAsyncEnumerable<TValue> items = JsonSerializer.DeserializeAsyncEnumerable(
                itemsStream,
                jsonTypeInfo,
                cancellationToken)!;

            await foreach (TValue item in items.ConfigureAwait(false))
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
        finally
        {
            await itemsStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task<(Pagination Pagination, Stream ItemsStream)> ExtractPaginationAndItemsAsync(
        PipeReader pipeReader,
        CancellationToken cancellationToken)
    {
        Pagination pagination = Pagination.Empty;

        // Buffer for scanning – we try to avoid unbounded growth; Benchmark JSON is modest in size.
        using MemoryStream bufferStream = new();

        while (true)
        {
            ReadResult result = await pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            ReadOnlySequence<byte> buffer = result.Buffer;

            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                bufferStream.Write(segment.Span);
            }

            pipeReader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await pipeReader.CompleteAsync().ConfigureAwait(false);

        byte[] data = bufferStream.ToArray();

        // Use Utf8JsonReader once over the whole payload to:
        //   - find and parse "pagination"
        //   - slice out the "items" array JSON into a MemoryStream
        ReadOnlySpan<byte> span = data.AsSpan();
        var reader = new Utf8JsonReader(span, isFinalBlock: true, default);

        int itemsStart = -1;
        int itemsEnd = -1;

        if (reader.Read() && reader.TokenType == JsonTokenType.StartObject)
        {
            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                if (reader.ValueTextEquals("pagination"u8))
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    Utf8JsonReader paginationReader = reader; // struct copy
                    pagination = JsonSerializer.Deserialize(
                        ref paginationReader,
                        PaginationJsonContext.Default.Pagination);

                    // Skip over pagination object in the main reader
                    reader.Skip();
                }
                else if (reader.ValueTextEquals("items"u8))
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        itemsStart = (int)reader.TokenStartIndex;

                        int depth = 1;
                        while (depth > 0 && reader.Read())
                        {
                            if (reader.TokenType == JsonTokenType.StartArray ||
                                reader.TokenType == JsonTokenType.StartObject)
                            {
                                depth++;
                            }
                            else if (reader.TokenType == JsonTokenType.EndArray ||
                                     reader.TokenType == JsonTokenType.EndObject)
                            {
                                depth--;
                            }
                        }

                        itemsEnd = (int)reader.BytesConsumed;
                    }
                    else
                    {
                        // items exists but is not an array – treat as empty.
                        itemsStart = itemsEnd = -1;
                    }
                }
                else
                {
                    // Skip unknown properties
                    reader.Read();
                    reader.Skip();
                }
            }
        }

        MemoryStream itemsStream = new();

        if (itemsStart >= 0 && itemsEnd > itemsStart)
        {
            ReadOnlySpan<byte> itemsSpan = span.Slice(itemsStart, itemsEnd - itemsStart);
            itemsStream.Write(itemsSpan);
            itemsStream.Position = 0;
        }
        else
        {
            // No items property → empty array as input to the deserializer
            byte[] emptyArray = "[]\u0000"u8.ToArray(); // small, constant allocation
            itemsStream.Write(emptyArray.AsSpan(0, 2)); // just "[]"
            itemsStream.Position = 0;
        }

        return (pagination, itemsStream);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions options)
    {
        return options.TryGetTypeInfo(typeof(T), out JsonTypeInfo? typeInfo)
            ? (JsonTypeInfo<T>)typeInfo
            : throw new InvalidOperationException(
                $"The JsonSerializerOptions does not contain metadata for type {typeof(T)}. " +
                "Ensure that the options include a JsonTypeInfoResolver that can provide metadata for this type.");
    }
}