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
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Collections.Generic;

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
        /// Wraps the UTF-8 encoded text from a PipeReader into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize root-level JSON arrays in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON array with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> is <see langword="null"/>.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues: false, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a PipeReader into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize root-level JSON arrays in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON array with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues: false, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a PipeReader into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize sequences of JSON values in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="topLevelValues">Whether to deserialize from a sequence of top-level JSON values.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON sequence with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true"/>, treats the PipeReader as a sequence of
        /// whitespace separated top-level JSON values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false"/>, treats the PipeReader as a JSON array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            bool topLevelValues,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a PipeReader into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize sequences of JSON values in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="topLevelValues"><see langword="true"/> to deserialize from a sequence of top-level JSON values, or <see langword="false"/> to deserialize from a single top-level array.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON sequence with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true"/>, treats the PipeReader as a sequence of
        /// whitespace separated top-level JSON values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false"/>, treats the PipeReader as a JSON array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            bool topLevelValues,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a Stream into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize root-level JSON arrays in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON array with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> is <see langword="null"/>.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues: false, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a Stream into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize sequences of JSON values in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="topLevelValues"><see langword="true"/> to deserialize from a sequence of top-level JSON values, or <see langword="false"/> to deserialize from a single top-level array.</param>
        /// <param name="options">Options to control the behavior during reading.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON sequence with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true"/>, treats the stream as a sequence of
        /// whitespace separated top-level JSON values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false"/>, treats the stream as a JSON array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            bool topLevelValues,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = GetTypeInfo<TValue>(options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a Stream into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize root-level JSON arrays in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON array with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues: false, cancellationToken);
        }

        /// <summary>
        /// Wraps the UTF-8 encoded text from a Stream into an <see cref="IAsyncPagedEnumerable{TValue}"/>
        /// that can be used to deserialize sequences of JSON values in a streaming manner with pagination support.
        /// </summary>
        /// <typeparam name="TValue">The element type to deserialize asynchronously.</typeparam>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="jsonTypeInfo">Metadata about the element type to convert.</param>
        /// <param name="topLevelValues">Whether to deserialize from a sequence of top-level JSON values.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the read operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> representation of the provided JSON sequence with pagination metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="topLevelValues"/> is set to <see langword="true"/>, treats the stream as a sequence of
        /// whitespace separated top-level JSON values and attempts to deserialize each value into <typeparamref name="TValue"/>.
        /// When <paramref name="topLevelValues"/> is set to <see langword="false"/>, treats the stream as a JSON array and
        /// attempts to serialize each element into <typeparamref name="TValue"/>.
        /// </remarks>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            bool topLevelValues,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLevelValues, cancellationToken);
        }
    }

    /// <summary>
    /// Core implementation for deserializing from a Stream into an IAsyncPagedEnumerable.
    /// Properly extracts pagination metadata from JSON envelope structure.
    /// </summary>
    /// <remarks>
    /// This method handles JSON in two formats:
    /// 1. Envelope format: { "pagination": {...}, "items": [...] } - Pagination metadata is extracted
    /// 2. Array format: [...] - Pagination is inferred from item count
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AsyncPagedEnumerable<T?> DeserializeAsyncPagedEnumerableCore<T>(
        Stream utf8Json,
        JsonTypeInfo<T> jsonTypeInfo,
        bool topLevelValues,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);

        var state = new DeserializationState<T>();

        return new AsyncPagedEnumerable<T?>(
            DeserializeAndExtractPagination(utf8Json, jsonTypeInfo, topLevelValues, state, cancellationToken),
            async ct => await state.GetPaginationAsync(ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Core implementation for deserializing from a PipeReader into an IAsyncPagedEnumerable.
    /// Properly extracts pagination metadata from JSON envelope structure.
    /// </summary>
    /// <remarks>
    /// This method handles JSON in two formats:
    /// 1. Envelope format: { "pagination": {...}, "items": [...] } - Pagination metadata is extracted
    /// 2. Array format: [...] - Pagination is inferred from item count
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AsyncPagedEnumerable<T?> DeserializeAsyncPagedEnumerableCore<T>(
        PipeReader utf8Json,
        JsonTypeInfo<T> jsonTypeInfo,
        bool topLevelValues,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);

        var state = new DeserializationState<T>();

        return new AsyncPagedEnumerable<T?>(
            DeserializeAndExtractPagination(utf8Json, jsonTypeInfo, topLevelValues, state, cancellationToken),
            async ct => await state.GetPaginationAsync(ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Holds the state for deserialization including extracted pagination.
    /// </summary>
    private sealed class DeserializationState<T>
    {
        private readonly TaskCompletionSource<Pagination> _paginationSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<T?> _items = [];

        public List<T?> Items => _items;

        public void SetPagination(Pagination pagination) => _paginationSource.TrySetResult(pagination);

        public void SetException(Exception exception) => _paginationSource.TrySetException(exception);

        public Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                return _paginationSource.Task.WaitAsync(cancellationToken);
            }
            return _paginationSource.Task;
        }
    }

    /// <summary>
    /// Asynchronously deserializes JSON from a Stream or PipeReader and extracts pagination metadata.
    /// </summary>
    /// <remarks>
    /// This unified method handles both Stream and PipeReader sources by converting them to Stream when needed.
    /// It parses the JSON document to determine structure and extracts pagination metadata when available.
    /// </remarks>
    private static async IAsyncEnumerable<T?> DeserializeAndExtractPagination<T>(
        object utf8Json,
        JsonTypeInfo<T> jsonTypeInfo,
        bool topLevelValues,
        DeserializationState<T> state,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Convert input to Stream for unified processing
        Stream stream = utf8Json switch
        {
            Stream s => s,
            PipeReader pr => pr.AsStream(),
            _ => throw new ArgumentException($"Unsupported type: {utf8Json.GetType().Name}. Expected Stream or PipeReader.", nameof(utf8Json))
        };

        using var document = await JsonDocument
            .ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var root = document.RootElement;

        // Check if this is an envelope format with pagination metadata
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("pagination"u8, out var paginationElement) &&
            root.TryGetProperty("items"u8, out var itemsElement))
        {
            // Extract pagination metadata
            var extractedPagination = JsonSerializer.Deserialize(paginationElement, PaginationJsonContext.Default.Pagination);
            state.SetPagination(extractedPagination);

            // Deserialize items array
            if (itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in itemsElement.EnumerateArray())
                {
                    var item = JsonSerializer.Deserialize(element, jsonTypeInfo);
                    state.Items.Add(item);
                    yield return item;
                }
            }
        }
        else if (root.ValueKind == JsonValueKind.Array)
        {
            // Standard array format - infer pagination from count
            foreach (var element in root.EnumerateArray())
            {
                var item = JsonSerializer.Deserialize(element, jsonTypeInfo);
                state.Items.Add(item);
                yield return item;
            }

            state.SetPagination(CreateInferredPagination(state.Items.Count));
        }
        else if (topLevelValues)
        {
            // Single top-level value
            var item = JsonSerializer.Deserialize(root, jsonTypeInfo);
            state.Items.Add(item);
            yield return item;

            state.SetPagination(CreateInferredPagination(1));
        }
        else
        {
            // Empty or unexpected format
            state.SetPagination(Pagination.Empty);
        }
    }

    /// <summary>
    /// Creates pagination metadata inferred from the item count when no explicit pagination is available.
    /// </summary>
    private static Pagination CreateInferredPagination(int itemCount)
        => Pagination.Create(pageSize: itemCount, currentPage: itemCount > 0 ? 1 : 0, totalCount: itemCount);

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
