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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.AsyncPaged.Extensions;

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
        #region PipeWriter - Generic with JsonTypeInfo

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

            // Note: MakeReadOnly on JsonTypeInfo.Options is safe because JsonTypeInfo is already immutable
            return CoreSerializeAsyncPagedAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new TypeInfoSerializer<TValue>(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region PipeWriter - Generic with JsonSerializerOptions

        /// <summary>
        /// Asynchronously serializes the elements of a paged asynchronous enumerable to UTF-8 encoded JSON using the
        /// provided writer.
        /// </summary>
        /// <typeparam name="TValue">The type of the elements in the paged enumerable to serialize.</typeparam>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized.</param>
        /// <param name="options">Options to control the behavior of the JSON serialization. If null, default options are used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresDynamicCode("Dynamic serialization is required for this operation.")]
        [RequiresUnreferencedCode("Dynamic serialization is required for this operation.")]
        public static Task SerializeAsyncPaged<TValue>(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            options.MakeReadOnly(true);

            return CoreSerializeAsyncPagedAsync(
                utf8Json,
                pagedEnumerable,
                options,
                new OptionsSerializer<TValue>(options),
                cancellationToken);
        }

        #endregion

        #region PipeWriter - Non-generic with JsonTypeInfo

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to UTF-8 encoded JSON using the
        /// specified type metadata.
        /// </summary>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON output will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized.</param>
        /// <param name="jsonTypeInfo">The metadata that defines how to serialize the elements of the enumerable.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        public static Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonTypeInfo jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new NonGenericTypeInfoSerializer(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region PipeWriter - Non-generic with JsonSerializerContext

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to UTF-8 encoded JSON using the
        /// specified serialization context.
        /// </summary>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON output will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized.</param>
        /// <param name="context">The source-generated JSON serialization context that provides metadata for serializing the elements.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        public static Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(context);

            JsonTypeInfo? jsonTypeInfo = context.GetTypeInfo(pagedEnumerable.Type)
                ?? throw new InvalidOperationException(
                    $"The JsonSerializerContext does not contain metadata for type '{pagedEnumerable.Type.FullName}'.");

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new NonGenericTypeInfoSerializer(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region PipeWriter - Non-generic with JsonSerializerOptions

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to UTF-8 encoded JSON using the
        /// specified writer.
        /// </summary>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON output will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON.</param>
        /// <param name="options">Options to control the behavior of the JSON serialization. If null, default serialization options are used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresDynamicCode("Dynamic serialization is required for this operation.")]
        [RequiresUnreferencedCode("Dynamic serialization is required for this operation.")]
        public static Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            options.MakeReadOnly(true);

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
                new NonGenericOptionsSerializer(options),
                cancellationToken);
        }

        #endregion

        #region Stream - Generic with JsonTypeInfo

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

            return CoreSerializeAsyncPagedAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new TypeInfoSerializer<TValue>(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region Stream - Generic with JsonSerializerOptions

        /// <summary>
        /// Asynchronously serializes the elements of a paged asynchronous enumerable to the specified stream in UTF-8
        /// encoded JSON format.
        /// </summary>
        /// <remarks>This method requires dynamic code generation and may not be compatible with all
        /// trimming or AOT scenarios. The caller is responsible for managing the lifetime of the provided
        /// stream.</remarks>
        /// <typeparam name="TValue">The type of the elements in the paged enumerable to serialize.</typeparam>
        /// <param name="utf8Json">The stream to which the UTF-8 encoded JSON will be written. The stream must be writable.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized.</param>
        /// <param name="options">Options to control the behavior of the JSON serialization. If null, default options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresDynamicCode("Dynamic serialization is required for this operation.")]
        [RequiresUnreferencedCode("Dynamic serialization is required for this operation.")]
        public static Task SerializeAsyncPaged<TValue>(
            Stream utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            options.MakeReadOnly(true);

            return CoreSerializeAsyncPagedAsync(
                utf8Json,
                pagedEnumerable,
                options,
                new OptionsSerializer<TValue>(options),
                cancellationToken);
        }

        #endregion

        #region Stream - Non-generic with JsonTypeInfo

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to the specified stream in UTF-8
        /// encoded JSON format.
        /// </summary>
        /// <param name="utf8Json">The stream to which the JSON data will be written. The stream must be writable and will not be closed by
        /// this method.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON. Cannot be null.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the JSON serialization of the elements. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        public static Task SerializeAsyncPaged(
            Stream utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonTypeInfo jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new NonGenericTypeInfoSerializer(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region Stream - Non-generic with JsonSerializerContext

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to the specified stream in UTF-8
        /// encoded JSON format.
        /// </summary>
        /// <param name="utf8Json">The stream to which the JSON data will be written. The stream must be writable and will not be closed by
        /// this method.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON. Cannot be null.</param>
        /// <param name="context">The source-generated JSON serialization context that provides metadata required for serialization. Cannot be
        /// null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        public static Task SerializeAsyncPaged(
            Stream utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(context);

            JsonTypeInfo? jsonTypeInfo = context.GetTypeInfo(pagedEnumerable.Type)
              ?? throw new InvalidOperationException(
                    $"The JsonSerializerContext does not contain metadata for type '{pagedEnumerable.Type.FullName}'.");

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo.Options,
                new NonGenericTypeInfoSerializer(jsonTypeInfo),
                cancellationToken);
        }

        #endregion

        #region Stream - Non-generic with JsonSerializerOptions

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to a UTF-8 encoded JSON stream.
        /// </summary>
        /// <param name="utf8Json">The stream to which the UTF-8 encoded JSON data will be written. The stream must be writable.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON. Cannot be null.</param>
        /// <param name="options">Optional serialization options to control JSON formatting and behavior. If null, default options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresDynamicCode("Dynamic serialization is required for this operation.")]
        [RequiresUnreferencedCode("Dynamic serialization is required for this operation.")]
        public static Task SerializeAsyncPaged(
            Stream utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            options.MakeReadOnly(true);

            return CoreSerializeAsyncPagedNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
                new NonGenericOptionsSerializer(options),
                cancellationToken);
        }

        #endregion
    }

    #region Private - Serializer Structs (Avoid Lambda Allocations)

    private readonly struct TypeInfoSerializer<T>(JsonTypeInfo<T> typeInfo) : IItemSerializer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Utf8JsonWriter writer, T item)
        {
            JsonSerializer.Serialize(writer, item, typeInfo);
        }
    }

    private readonly struct OptionsSerializer<T>(JsonSerializerOptions options) : IItemSerializer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Caller is already annotated with RequiresUnreferencedCode")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Caller is already annotated with RequiresDynamicCode")]
        public void Serialize(Utf8JsonWriter writer, T item)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
    }

    private readonly struct NonGenericTypeInfoSerializer(JsonTypeInfo typeInfo) : IItemSerializer<object?>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Utf8JsonWriter writer, object? item)
        {
            JsonSerializer.Serialize(writer, item, typeInfo);
        }
    }

    private readonly struct NonGenericOptionsSerializer(JsonSerializerOptions options) : IItemSerializer<object?>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Caller is already annotated with RequiresUnreferencedCode")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Caller is already annotated with RequiresDynamicCode")]
        public void Serialize(Utf8JsonWriter writer, object? item)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
    }

    private interface IItemSerializer<in T>
    {
        void Serialize(Utf8JsonWriter writer, T item);
    }

    #endregion

    #region Private - Core Implementation for Generic

    private static async Task CoreSerializeAsyncPagedAsync<T, TSerializer>(
        object output,
        IAsyncPagedEnumerable<T> pagedEnumerable,
        JsonSerializerOptions options,
        TSerializer serializer,
        CancellationToken cancellationToken)
        where TSerializer : struct, IItemSerializer<T>
    {
        cancellationToken.ThrowIfCancellationRequested();

        JsonWriterOptions writerOptions = new()
        {
            Indented = options.WriteIndented,
            Encoder = options.Encoder,
            SkipValidation = !options.WriteIndented
        };

        Utf8JsonWriter writer = output switch
        {
            PipeWriter pipe => new Utf8JsonWriter(pipe, writerOptions),
            Stream stream => new Utf8JsonWriter(stream, writerOptions),
            _ => throw new ArgumentException("Output must be PipeWriter or Stream", nameof(output))
        };

        try
        {
            Pagination pagination = await pagedEnumerable
                .GetPaginationAsync(cancellationToken)
                .ConfigureAwait(false);

            writer.WriteStartObject();
            writer.WritePropertyName("pagination"u8);
            JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);

            writer.WritePropertyName("items"u8);
            writer.WriteStartArray();

            // PERFORMANCE: Adaptive flushing based on dataset size and memory pressure
            FlushStrategy flushStrategy = FlushStrategy.Create(pagination.TotalCount);
            int itemCount = 0;

            await foreach (T item in pagedEnumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                serializer.Serialize(writer, item);
                itemCount++;

                // PERFORMANCE: Adaptive memory-aware flushing
                if (flushStrategy.ShouldFlush(itemCount, writer.BytesPending))
                {
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await writer.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Private - Core Implementation for Non-Generic

    private static async Task CoreSerializeAsyncPagedNonGenericAsync<TSerializer>(
        object output,
        IAsyncPagedEnumerable pagedEnumerable,
        JsonSerializerOptions options,
        TSerializer serializer,
        CancellationToken cancellationToken)
        where TSerializer : struct, IItemSerializer<object?>
    {
        cancellationToken.ThrowIfCancellationRequested();

        JsonWriterOptions writerOptions = new()
        {
            Indented = options.WriteIndented,
            Encoder = options.Encoder,
            SkipValidation = !options.WriteIndented
        };

        Utf8JsonWriter writer = output switch
        {
            PipeWriter pipeWriter => new Utf8JsonWriter(pipeWriter, writerOptions),
            Stream stream => new Utf8JsonWriter(stream, writerOptions),
            _ => throw new ArgumentException("Output must be either PipeWriter or Stream", nameof(output))
        };

        try
        {
            Pagination pagination = await pagedEnumerable
                .GetPaginationAsync(cancellationToken)
                .ConfigureAwait(false);

            writer.WriteStartObject();
            cancellationToken.ThrowIfCancellationRequested();

            writer.WritePropertyName("pagination"u8);
            JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);

            writer.WritePropertyName("items"u8);
            writer.WriteStartArray();

            await SerializeAsyncEnumerableItemsAsync(
                writer,
                pagedEnumerable,
                serializer,
                pagination,
                cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            writer.WriteEndArray();
            writer.WriteEndObject();

            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await writer.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Private - Reflection-based Enumeration for Non-generic

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Non-generic serialization requires runtime type information")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Non-generic serialization requires runtime type information")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is required for non-generic async enumerable serialization")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "GetType() is required for non-generic async enumerable serialization")]
    private static async Task SerializeAsyncEnumerableItemsAsync<TSerializer>(
        Utf8JsonWriter writer,
        IAsyncPagedEnumerable pagedEnumerable,
        TSerializer serializer,
        Pagination pagination,
        CancellationToken cancellationToken)
        where TSerializer : struct, IItemSerializer<object?>
    {
        Type enumerableType = pagedEnumerable.GetType();

        // PERFORMANCE: Cache interface lookup to avoid repeated searches
        Type? asyncEnumerableInterface = FindAsyncEnumerableInterface(enumerableType);
        if (asyncEnumerableInterface is null)
        {
            return;
        }

        MethodInfo? getEnumeratorMethod = asyncEnumerableInterface.GetMethod(
            "GetAsyncEnumerator",
            [typeof(CancellationToken)]);

        if (getEnumeratorMethod is null)
        {
            return;
        }

        object? enumerator = getEnumeratorMethod.Invoke(pagedEnumerable, [cancellationToken]);
        if (enumerator is null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Type enumeratorType = enumerator.GetType();
            Type? asyncEnumeratorInterface = FindAsyncEnumeratorInterface(enumeratorType);

            if (asyncEnumeratorInterface is null)
            {
                return;
            }

            MethodInfo? moveNextMethod = asyncEnumeratorInterface.GetMethod("MoveNextAsync");
            PropertyInfo? currentProperty = asyncEnumeratorInterface.GetProperty("Current");

            if (moveNextMethod is null || currentProperty is null)
            {
                return;
            }

            // PERFORMANCE: Adaptive batch flushing
            FlushStrategy flushStrategy = FlushStrategy.Create(pagination.TotalCount);
            int itemCount = 0;

            while (true)
            {
                object? moveNextResult = moveNextMethod.Invoke(enumerator, null);
                cancellationToken.ThrowIfCancellationRequested();

                if (moveNextResult is not ValueTask<bool> valueTask)
                {
                    break;
                }

                bool hasNext = await valueTask.ConfigureAwait(false);
                if (!hasNext)
                {
                    break;
                }

                object? current = currentProperty.GetValue(enumerator);
                serializer.Serialize(writer, current);
                itemCount++;

                // PERFORMANCE: Adaptive memory-aware flushing
                if (flushStrategy.ShouldFlush(itemCount, writer.BytesPending))
                {
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            if (enumerator is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    // PERFORMANCE: Cache interface lookups to reduce repeated reflection
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Type.GetInterfaces is required for non-generic async enumerable serialization")]
    private static Type? FindAsyncEnumerableInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        return Array.Find(
            type.GetInterfaces(),
            static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Type.GetInterfaces is required for non-generic async enumerable serialization")]
    private static Type? FindAsyncEnumeratorInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        return Array.Find(
            type.GetInterfaces(),
            static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>));
    }

    #endregion

    #region Private - Flush Strategy (Memory-Aware Adaptive Flushing)

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

    #endregion
}
