/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Async;

/// <summary>
/// Provides extension methods for the JsonSerializer class to simplify JSON <see cref="IAsyncPagedEnumerable{T}"/> serialization tasks.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
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
        public static async Task SerializeAsyncPaged<TValue>(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            Stream stream = utf8Json.AsStream();
            await using (stream.ConfigureAwait(false))
            {
                await SerializeAsyncPagedCore(stream, pagedEnumerable, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
            }
        }

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
        public static async Task SerializeAsyncPaged<TValue>(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));

            await SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
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
        public static async Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonTypeInfo jsonTypeInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            Stream stream = utf8Json.AsStream();
            await using (stream.ConfigureAwait(false))
            {
                await SerializeAsyncPagedNonGenericCore(stream, pagedEnumerable, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
            }
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
        public static async Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);
            ArgumentNullException.ThrowIfNull(context);

            JsonTypeInfo jsonTypeInfo = context.GetTypeInfo(pagedEnumerable.Type)
                ?? throw new InvalidOperationException($"Type information for '{pagedEnumerable.Type}' not found in the provided context.");

            await SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
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
        [RequiresDynamicCode("Dynamic serialization is required for this operation.")]
        [RequiresUnreferencedCode("Dynamic serialization is required for this operation.")]
        public static async Task SerializeAsyncPaged(
            PipeWriter utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(pagedEnumerable);

            options ??= JsonSerializerOptions.Default;
            JsonTypeInfo jsonTypeInfo = options.GetTypeInfo(pagedEnumerable.Type);

            await SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
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

            return SerializeAsyncPagedCore(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken);
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
            JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));

            return SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken);
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

            return SerializeAsyncPagedNonGenericCore(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken);
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

            JsonTypeInfo jsonTypeInfo = context.GetTypeInfo(pagedEnumerable.Type)
                ?? throw new InvalidOperationException($"Type information for '{pagedEnumerable.Type}' not found in the provided context.");

            return SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken);
        }

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
            JsonTypeInfo jsonTypeInfo = options.GetTypeInfo(pagedEnumerable.Type);

            return SerializeAsyncPaged(utf8Json, pagedEnumerable, jsonTypeInfo, cancellationToken);
        }

        private static async Task SerializeAsyncPagedCore<TValue>(
            Stream utf8Json,
            IAsyncPagedEnumerable<TValue> pagedEnumerable,
            JsonTypeInfo<TValue> jsonTypeInfo,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Utf8JsonWriter writer = new(utf8Json, new JsonWriterOptions
            {
                Indented = jsonTypeInfo.Options.WriteIndented,
                Encoder = jsonTypeInfo.Options.Encoder
            });

            cancellationToken.ThrowIfCancellationRequested();
            await using (writer.ConfigureAwait(false))
            {
                Pagination pagination = await pagedEnumerable
                    .GetPaginationAsync(cancellationToken)
                    .ConfigureAwait(false);

                writer.WriteStartObject();
                cancellationToken.ThrowIfCancellationRequested();
                writer.WritePropertyName("pagination");
                JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);

                writer.WritePropertyName("items");
                writer.WriteStartArray();

                await foreach (TValue item in pagedEnumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    JsonSerializer.Serialize(writer, item, jsonTypeInfo);
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Non-generic serialization requires runtime type information")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Non-generic serialization requires runtime type information")]
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is required for non-generic async enumerable serialization")]
        private static async Task SerializeAsyncPagedNonGenericCore(
            Stream utf8Json,
            IAsyncPagedEnumerable pagedEnumerable,
            JsonTypeInfo jsonTypeInfo,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Utf8JsonWriter writer = new(utf8Json, new JsonWriterOptions
            {
                Indented = jsonTypeInfo.Options.WriteIndented,
                Encoder = jsonTypeInfo.Options.Encoder
            });

            cancellationToken.ThrowIfCancellationRequested();
            await using (writer.ConfigureAwait(false))
            {
                Pagination pagination = await pagedEnumerable
                    .GetPaginationAsync(cancellationToken)
                    .ConfigureAwait(false);

                writer.WriteStartObject();
                cancellationToken.ThrowIfCancellationRequested();
                writer.WritePropertyName("pagination");
                JsonSerializer.Serialize(writer, pagination, PaginationSourceGenerationContext.Default.Pagination);

                writer.WritePropertyName("items");
                writer.WriteStartArray();

                // Get the strongly-typed generic interface for enumeration
                Type enumerableType = pagedEnumerable.GetType();
                Type? asyncEnumerableInterface = Array.Find(
                    enumerableType.GetInterfaces(),
                    static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
                cancellationToken.ThrowIfCancellationRequested();
                if (asyncEnumerableInterface is not null)
                {
                    // Get GetAsyncEnumerator method from the interface
                    System.Reflection.MethodInfo? getEnumeratorMethod = asyncEnumerableInterface.GetMethod("GetAsyncEnumerator");

                    if (getEnumeratorMethod is not null)
                    {
                        object? enumerator = getEnumeratorMethod.Invoke(pagedEnumerable, [cancellationToken]);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (enumerator is not null)
                        {
                            Type enumeratorType = enumerator.GetType();
                            Type? asyncEnumeratorInterface = Array.Find(
                                enumeratorType.GetInterfaces(),
                                static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>));
                            cancellationToken.ThrowIfCancellationRequested();
                            if (asyncEnumeratorInterface is not null)
                            {
                                System.Reflection.MethodInfo? moveNextMethod = asyncEnumeratorInterface.GetMethod("MoveNextAsync");
                                System.Reflection.PropertyInfo? currentProperty = asyncEnumeratorInterface.GetProperty("Current");
                                cancellationToken.ThrowIfCancellationRequested();
                                if (moveNextMethod is not null && currentProperty is not null)
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            object? moveNextResult = moveNextMethod.Invoke(enumerator, null);
                                            cancellationToken.ThrowIfCancellationRequested();
                                            if (moveNextResult is ValueTask<bool> valueTask)
                                            {
                                                bool hasNext = await valueTask.ConfigureAwait(false);
                                                if (!hasNext)
                                                    break;

                                                object? current = currentProperty.GetValue(enumerator);
                                                JsonSerializer.Serialize(writer, current, jsonTypeInfo);
                                                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                                            }
                                            else
                                            {
                                                break;
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
                            }
                        }
                    }
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }
    }
}
