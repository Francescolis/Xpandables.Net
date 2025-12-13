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

            return DeserializeCore(utf8Json, jsonTypeInfo, strategy, cancellationToken);
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
        // Extract pagination and items stream
        var extractionTask = ExtractPaginationAndItemsStreamAsync(pipeReader, cancellationToken);

        // Pagination factory
        Func<CancellationToken, ValueTask<Pagination>> paginationFactory = async ct =>
        {
            var (pagination, _) = await extractionTask.ConfigureAwait(false);
            return pagination;
        };

        // Items enumerable using framework deserializer
        var items = EnumerateItemsAsync(extractionTask, jsonTypeInfo, pipeReader, cancellationToken);

        // Wrap in AsyncPagedEnumerable - THIS WAS YOUR KEY INSIGHT!
        return AsyncPagedEnumerable.Create(items, paginationFactory, strategy);
    }

    private static async IAsyncEnumerable<TValue> EnumerateItemsAsync<TValue>(
        Task<(Pagination, Stream)> extractionTask,
        JsonTypeInfo<TValue> jsonTypeInfo,
        PipeReader pipeReader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Stream? itemsStream = null;
        try
        {
            var (_, stream) = await extractionTask.ConfigureAwait(false);
            itemsStream = stream;

            // Use framework's JsonSerializer.DeserializeAsyncEnumerable - MUCH SIMPLER!
            var items = JsonSerializer.DeserializeAsyncEnumerable<TValue>(
                itemsStream,
                jsonTypeInfo,
                cancellationToken);

            await foreach (var item in items.ConfigureAwait(false))
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
        finally
        {
            if (itemsStream is not null)
            {
                await itemsStream.DisposeAsync().ConfigureAwait(false);
            }
            await pipeReader.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static async Task<(Pagination, Stream)> ExtractPaginationAndItemsStreamAsync(
         PipeReader pipeReader,
         CancellationToken cancellationToken)
    {
        Pagination pagination = Pagination.Empty;
        var itemsStream = new MemoryStream();
        bool foundPagination = false;
        bool foundItems = false;
        int depth = 0;
        bool inItemsArray = false;

        try
        {
            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = result.Buffer;

                if (buffer.IsEmpty && result.IsCompleted)
                {
                    break;
                }

                var (consumed, pag, itemsData) = ProcessBuffer(
                    buffer,
                    ref foundPagination,
                    ref foundItems,
                    ref inItemsArray,
                    ref depth);

                if (pag is not null)
                {
                    pagination = pag.Value;
                }

                if (itemsData.Length > 0)
                {
                    foreach (var segment in itemsData)
                    {
                        itemsStream.Write(segment.Span);
                    }
                }

                pipeReader.AdvanceTo(buffer.GetPosition(consumed));

                if (foundItems && depth == 0)
                {
                    break;
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }

            itemsStream.Position = 0;
            return (pagination, itemsStream);
        }
        catch
        {
            await itemsStream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static (long Consumed, Pagination? Pagination, ReadOnlySequence<byte> ItemsData) ProcessBuffer(
         ReadOnlySequence<byte> buffer,
         ref bool foundPagination,
         ref bool foundItems,
         ref bool inItemsArray,
         ref int depth)
    {
        var reader = new Utf8JsonReader(buffer, isFinalBlock: false, default);
        Pagination? pagination = null;
        long startPosition = 0;
        long endPosition = 0;

        try
        {
            while (reader.Read())
            {
                if (!foundPagination && reader.TokenType == JsonTokenType.PropertyName &&
                    reader.ValueTextEquals("pagination"u8))
                {
                    if (reader.Read())
                    {
                        pagination = JsonSerializer.Deserialize(ref reader, PaginationJsonContext.Default.Pagination);
                        foundPagination = true;
                    }
                }
                else if (!foundItems && reader.TokenType == JsonTokenType.PropertyName &&
                         reader.ValueTextEquals("items"u8))
                {
                    if (reader.Read() && reader.TokenType == JsonTokenType.StartArray)
                    {
                        foundItems = true;
                        inItemsArray = true;
                        depth = 1;
                        startPosition = reader.TokenStartIndex;
                    }
                }
                else if (reader.TokenType == JsonTokenType.PropertyName && !foundPagination && !foundItems)
                {
                    reader.Read();
                    reader.Skip();
                }
                else if (inItemsArray)
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
                        if (depth == 0)
                        {
                            endPosition = reader.BytesConsumed;
                            break;
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Incomplete JSON
        }

        var consumed = reader.BytesConsumed;

        if (foundItems && startPosition >= 0)
        {
            var length = endPosition > 0 ? endPosition - startPosition : consumed - startPosition;
            var itemsSlice = buffer.Slice(startPosition, length);
            return (consumed, pagination, itemsSlice);
        }

        return (consumed, pagination, ReadOnlySequence<byte>.Empty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions options)
    {
        return options.TryGetTypeInfo(typeof(T), out var typeInfo)
            ? (JsonTypeInfo<T>)typeInfo
            : throw new InvalidOperationException(
                $"The JsonSerializerOptions does not contain metadata for type {typeof(T)}. " +
                "Ensure that the options include a JsonTypeInfoResolver that can provide metadata for this type.");
    }
}