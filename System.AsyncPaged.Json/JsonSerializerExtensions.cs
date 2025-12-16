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
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
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
        /// Asynchronously serializes the elements of a paged asynchronous enumerable to UTF-8 encoded JSON using the
        /// provided writer.
        /// </summary>
        /// <typeparam name="TValue">The type of the elements in the paged enumerable to serialize.</typeparam>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized.</param>
        /// <param name="options">Options to control the behavior of the JSON serialization.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
        [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
        public static Task SerializeAsyncPaged<TValue>(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(options);

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
                cancellationToken);
        }

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

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo,
                cancellationToken);
        }

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

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                context,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to UTF-8 encoded JSON using the
        /// specified writer.
        /// </summary>
        /// <param name="utf8Json">The writer to which the UTF-8 encoded JSON output will be written.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON.</param>
        /// <param name="options">Options to control the behavior of the JSON serialization. If null, default serialization options are used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresUnreferencedCode("Serialization may require types that are trimmed.")]
        [RequiresDynamicCode("Serialization may require types that are generated dynamically.")]
        public static Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(options);

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
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
        [RequiresUnreferencedCode("This method requires dynamic code generation and may not be compatible with all trimming or AOT scenarios.")]
        [RequiresDynamicCode("This method requires dynamic code generation and may not be compatible with all trimming or AOT scenarios.")]
        public static Task SerializeAsyncPaged<TValue>(
            Stream utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(options);

            return SerializeAsyncPagedCoreGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
                cancellationToken);
        }

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

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                jsonTypeInfo,
                cancellationToken);
        }

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

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                context,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the elements of an asynchronous paged enumerable to a UTF-8 encoded JSON stream.
        /// </summary>
        /// <param name="utf8Json">The stream to which the UTF-8 encoded JSON data will be written. The stream must be writable.</param>
        /// <param name="pagedEnumerable">The asynchronous paged enumerable whose elements are to be serialized to JSON. Cannot be null.</param>
        /// <param name="options">Optional serialization options to control JSON formatting and behavior. If null, default options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous serialization operation.</returns>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public static Task SerializeAsyncPaged(
            Stream utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(options);

            return SerializeAsyncPagedCoreNonGenericAsync(
                utf8Json,
                pagedEnumerable,
                options,
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

        JsonWriterOptions writerOptions = new()
        {
            Indented = jsonTypeInfo.Options.WriteIndented,
            Encoder = jsonTypeInfo.Options.Encoder,
            SkipValidation = !jsonTypeInfo.Options.WriteIndented
        };

        using Utf8JsonWriter writer = output switch
        {
            PipeWriter pipe => new Utf8JsonWriter(pipe, writerOptions),
            Stream stream => new Utf8JsonWriter(stream, writerOptions),
            _ => throw new ArgumentException("Output must be PipeWriter or Stream", nameof(output))
        };

        Pagination pagination = await paged
            .GetPaginationAsync(cancellationToken)
            .ConfigureAwait(false);

        writer.WriteStartObject();
        writer.WritePropertyName("pagination"u8);
        JsonSerializer.Serialize(writer, pagination, PaginationJsonContext.Default.Pagination);

        writer.WritePropertyName("items"u8);
        writer.WriteStartArray();

        FlushStrategy flushStrategy = FlushStrategy.Create(pagination.TotalCount);
        int itemCount = 0;

        await foreach (T item in paged.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
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
    }

    [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
    private static Task SerializeAsyncPagedCoreGenericAsync<T>(
        object output,
        IAsyncPagedEnumerable<T> paged,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var jsonTypeInfo = (JsonTypeInfo<T>)HttpContentExtensions.GetJsonTypeInfo(typeof(T), options);
        return SerializeAsyncPagedCoreGenericAsync(
            output,
            paged,
            jsonTypeInfo,
            cancellationToken);
    }

    [RequiresDynamicCode("Calls System.Reflection.MethodInfo.MakeGenericMethod(params Type[])")]
    [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
    private static Task SerializeAsyncPagedCoreNonGenericAsync(
        object output,
        IAsyncPagedEnumerable paged,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var method = SerializeAsyncPagedMethod.MakeGenericMethod(paged.GetArgumentType());
        var jsonTypeInfo = HttpContentExtensions.GetJsonTypeInfo(paged.GetArgumentType(), options);
        var task = (Task)method.Invoke(null, [output, paged, jsonTypeInfo, cancellationToken])!;
        return task;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2070:Calling members annotated with 'RequiresUnreferencedCodeAttribute' may break functionality when trimming application code.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2060:Calling members annotated with 'RequiresAssemblyFilesAttribute' may break functionality when trimming application code.", Justification = "<Pending>")]
    private static Task SerializeAsyncPagedCoreNonGenericAsync(
        object output,
        IAsyncPagedEnumerable paged,
        JsonSerializerContext context,
        CancellationToken cancellationToken = default)
    {
        var method = SerializeAsyncPagedMethod.MakeGenericMethod(paged.GetArgumentType());
        var jsonTypeInfo = context.GetTypeInfo(paged.GetArgumentType());
        var task = (Task)method.Invoke(null, [output, paged, jsonTypeInfo, cancellationToken])!;
        return task;
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2070:Calling members annotated with 'RequiresUnreferencedCodeAttribute' may break functionality when trimming application code.", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2060:Calling members annotated with 'RequiresAssemblyFilesAttribute' may break functionality when trimming application code.", Justification = "<Pending>")]
    private static Task SerializeAsyncPagedCoreNonGenericAsync(
        object output,
        IAsyncPagedEnumerable paged,
        JsonTypeInfo jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        var method = SerializeAsyncPagedMethod.MakeGenericMethod(paged.GetArgumentType());
        var task = (Task)method.Invoke(null, [output, paged, jsonTypeInfo, cancellationToken])!;
        return task;
    }

    private static readonly MethodInfo SerializeAsyncPagedMethod =
        ((MethodCallExpression)((Expression<Func<Task>>)(() =>
            SerializeAsyncPagedCoreGenericAsync<int>(null!, paged: null!, jsonTypeInfo: null!, cancellationToken: default))).Body)
        .Method.GetGenericMethodDefinition();

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
