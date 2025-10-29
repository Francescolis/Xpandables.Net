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
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.AsyncPaged.Extensions;

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
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonSerializerOptions? options = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);

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
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            bool topLevelValues,
            JsonSerializerOptions? options = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);

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
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonSerializerOptions? options = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);

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
        [RequiresDynamicCode("The type to deserialize may require dynamic code generation.")]
        [RequiresUnreferencedCode("The type to deserialize may require types that are not statically referenced.")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            bool topLevelValues,
            JsonSerializerOptions? options = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);

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
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AsyncPagedEnumerable<T?> DeserializeAsyncPagedEnumerableCore<T>(
        Stream utf8Json,
        JsonTypeInfo<T> jsonTypeInfo,
        bool topLevelValues,
        CancellationToken cancellationToken)
    {
        // Wrap the Stream-based IAsyncEnumerable in an AsyncPagedEnumerable
        IAsyncEnumerable<T?> source = JsonSerializer.DeserializeAsyncEnumerable(
            utf8Json,
            jsonTypeInfo,
            topLevelValues,
            cancellationToken);

        return new AsyncPagedEnumerable<T?>(
            source,
            async ct =>
            {
                // Compute pagination metadata by counting items
                // Note: This requires enumerating the entire source
                long count = 0;
                await foreach (var _ in source.WithCancellation(ct))
                {
                    count++;
                }

                return Pagination.Create(
                    pageSize: 0,
                    currentPage: 0,
                    continuationToken: null,
                    totalCount: count > int.MaxValue ? int.MaxValue : (int)count);
            });
    }

    /// <summary>
    /// Core implementation for deserializing from a PipeReader into an IAsyncPagedEnumerable.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AsyncPagedEnumerable<T?> DeserializeAsyncPagedEnumerableCore<T>(
        PipeReader utf8Json,
        JsonTypeInfo<T> jsonTypeInfo,
        bool topLevelValues,
        CancellationToken cancellationToken)
    {
        // Wrap the PipeReader-based IAsyncEnumerable in an AsyncPagedEnumerable
        IAsyncEnumerable<T?> source = JsonSerializer.DeserializeAsyncEnumerable(
            utf8Json,
            jsonTypeInfo,
            topLevelValues,
            cancellationToken);

        return new AsyncPagedEnumerable<T?>(
            source,
            async ct =>
                {
                    // Compute pagination metadata by counting items
                    // Note: This requires enumerating the entire source
                    long count = 0;
                    await foreach (var _ in source.WithCancellation(ct))
                    {
                        count++;
                    }

                    return Pagination.Create(
                        pageSize: 0,
                        currentPage: 0,
                        continuationToken: null,
                        totalCount: count > int.MaxValue ? int.MaxValue : (int)count);
                });
    }

    /// <summary>
    /// Gets JSON type information for the specified type from the provided options.
    /// </summary>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTypeInfo<T> GetTypeInfo<T>(JsonSerializerOptions? options)
    {
        options ??= JsonSerializerOptions.Default;

        JsonTypeInfo? typeInfo = options.TypeInfoResolver?.GetTypeInfo(typeof(T), options);

        if (typeInfo is JsonTypeInfo<T> genericTypeInfo)
        {
            return genericTypeInfo;
        }

        // Fallback: create type info dynamically (not AOT-safe, but required for non-source-generated scenarios)
        return JsonTypeInfo.CreateJsonTypeInfo<T>(options);
    }
}
