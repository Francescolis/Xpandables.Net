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
using System.Net.Http.Json;
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
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of type
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The <see cref="Stream"/> containing the UTF-8 encoded JSON data to deserialize.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization. Cannot be null.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream. The enumerable may be empty if the stream contains no items.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonSerializerOptions? options = null,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            return DeserializeAsyncPagedEnumerable<TValue>(utf8Json, topLovelValues: false, options, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON stream into an asynchronous paged enumerable of values of type TValue.
        /// </summary>
        /// <remarks>The returned enumerable supports asynchronous iteration and may be used to
        /// efficiently process large or paged JSON payloads without loading the entire content into memory. The caller
        /// is responsible for disposing the input stream when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of the elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The stream containing the UTF-8 encoded JSON data to deserialize. The stream must be readable and positioned
        /// at the start of the JSON content.</param>
        /// <param name="topLovelValues">true to deserialize values from the top-level of the JSON array or object; otherwise, false to use the
        /// default deserialization behavior.</param>
        /// <param name="options">The options to use for JSON deserialization. Cannot be null.</param>
        /// <param name="strategy">The pagination strategy to use when reading paged results from the JSON stream. Use PaginationStrategy.None
        /// for non-paged data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration.</param>
        /// <returns>An asynchronous paged enumerable of deserialized values of type TValue. The enumerable yields each item as
        /// it is read from the JSON stream.</returns>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            bool topLovelValues,
            JsonSerializerOptions? options = null,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)HttpContentExtensions.GetJsonTypeInfo(typeof(TValue), options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLovelValues, strategy, cancellationToken);
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
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerable(utf8Json, topLovelValues: false, jsonTypeInfo, strategy, cancellationToken);
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
        /// <param name="topLovelValues">true to deserialize values from the top-level of the JSON array or object; otherwise, false to use the
        /// normal deserialization behavior.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of objects of type TValue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration operation.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized TValue elements from the provided JSON stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            Stream utf8Json,
            bool topLovelValues,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLovelValues, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values of type
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The <see cref="Stream"/> containing the UTF-8 encoded JSON data to deserialize.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization. Cannot be null.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncPagedEnumerable{TValue}"/> that asynchronously yields deserialized values from the JSON
        /// stream. The enumerable may be empty if the stream contains no items.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonSerializerOptions? options = null,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            return DeserializeAsyncPagedEnumerable<TValue>(utf8Json, topLovelValues: false, options, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of values of type TValue.
        /// </summary>
        /// <remarks>The returned enumerable supports asynchronous iteration and may be used to
        /// efficiently process large or paged JSON payloads without loading the entire content into memory. The caller
        /// is responsible for disposing the input pipe reader when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of the elements to deserialize from the JSON stream.</typeparam>
        /// <param name="utf8Json">The pipe reader containing the UTF-8 encoded JSON data to deserialize. The pipe reader must be readable and positioned
        /// at the start of the JSON content.</param>
        /// <param name="topLovelValues">true to deserialize values from the top-level of the JSON array or object; otherwise, false to use the
        /// default deserialization behavior.</param>
        /// <param name="options">The options to use for JSON deserialization. Cannot be null.</param>
        /// <param name="strategy">The pagination strategy to use when reading paged results from the JSON stream. Use PaginationStrategy.None
        /// for non-paged data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration.</param>
        /// <returns>An asynchronous paged enumerable of deserialized values of type TValue. The enumerable yields each item as
        /// it is read from the JSON stream.</returns>
        [RequiresUnreferencedCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Net.Http.Json.HttpContentExtensions.GetJsonTypeInfo(Type, JsonSerializerOptions)")]
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            bool topLovelValues,
            JsonSerializerOptions? options = null,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(options);

            JsonTypeInfo<TValue> jsonTypeInfo = (JsonTypeInfo<TValue>)HttpContentExtensions.GetJsonTypeInfo(typeof(TValue), options);
            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLovelValues, strategy, cancellationToken);
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
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerable(utf8Json, topLovelValues: false, jsonTypeInfo, strategy, cancellationToken);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON pipe reader into an asynchronous paged enumerable of elements of type TValue.
        /// </summary>
        /// <remarks>The returned enumerable reads and deserializes items from the pipe reader as they are
        /// requested, enabling efficient processing of large or paged JSON datasets. The caller is responsible for
        /// disposing the pipe reader when enumeration is complete.</remarks>
        /// <typeparam name="TValue">The type of elements to deserialize from the JSON pipe reader.</typeparam>
        /// <param name="utf8Json">The pipe reader containing UTF-8 encoded JSON data representing a paged collection of TValue elements. Must not
        /// be null.</param>
        /// <param name="topLovelValues">true to deserialize values from the top-level of the JSON array or object; otherwise, false to use the
        /// normal deserialization behavior.</param>
        /// <param name="jsonTypeInfo">Metadata used to control the deserialization of objects of type TValue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous enumeration operation.</param>
        /// <param name="strategy">The pagination strategy to use when deserializing the JSON data.</param>
        /// <returns>An asynchronous paged enumerable that yields deserialized TValue elements from the provided JSON stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="utf8Json"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        public static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerable<TValue>(
            PipeReader utf8Json,
            bool topLovelValues,
            JsonTypeInfo<TValue> jsonTypeInfo,
            PaginationStrategy strategy = PaginationStrategy.None,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            ArgumentNullException.ThrowIfNull(jsonTypeInfo);

            return DeserializeAsyncPagedEnumerableCore(utf8Json, jsonTypeInfo, topLovelValues, strategy, cancellationToken);
        }

    }

    private static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerableCore<TValue>(
        Stream utf8Json,
        JsonTypeInfo<TValue> jsonTypeInfo,
        bool topLovelValues,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {

        return AsyncPagedEnumerable.Empty<TValue>();
    }

    private static IAsyncPagedEnumerable<TValue?> DeserializeAsyncPagedEnumerableCore<TValue>(
        PipeReader utf8Json,
        JsonTypeInfo<TValue> jsonTypeInfo,
        bool topLovelValues,
        PaginationStrategy strategy,
        CancellationToken cancellationToken)
    {

        return AsyncPagedEnumerable.Empty<TValue>();
    }
}