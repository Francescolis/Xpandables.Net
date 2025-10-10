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
namespace Xpandables.Net.Async;

using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using Xpandables.Net;
using Xpandables.Net.Async;
using Xpandables.Net.Text;

/// <summary>
/// Provides efficient, streaming JSON deserialization from a UTF-8 encoded stream with support for
/// extracting pagination metadata and array elements without loading the entire payload into memory.
/// </summary>
/// <remarks>
/// This reader is optimized for processing large JSON responses incrementally, making it ideal for
/// paginated API responses. It supports both array-root and object-with-items JSON structures,
/// and can extract pagination metadata on-the-fly during streaming.
/// </remarks>
public sealed class Utf8JsonStreamReader : IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private readonly int _bufferSize;
    private readonly JsonSerializerOptions _options;
    private readonly bool _leaveOpen;

    private int _bytesInBuffer;
    private int _consumed;
    private JsonReaderState _state;
    private Pagination? _pagination;
    private bool _paginationExtracted;

    private const int DefaultBufferSize = 16 * 1024; // 16KB default buffer

    /// <summary>
    /// Initializes a new instance of the <see cref="Utf8JsonStreamReader"/> class.
    /// </summary>
    /// <param name="stream">The UTF-8 encoded JSON stream to read from. Cannot be null.</param>
    /// <param name="options">The JSON serializer options to use for deserialization. If null, uses web defaults.</param>
    /// <param name="bufferSize">The size of the internal buffer in bytes. Defaults to 16KB.</param>
    /// <param name="leaveOpen">If true, the stream will not be disposed when this reader is disposed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bufferSize"/> is less than 256.</exception>
    public Utf8JsonStreamReader(
        Stream stream,
        JsonSerializerOptions? options = null,
        int bufferSize = DefaultBufferSize,
        bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 256);

        _stream = stream;
        _bufferSize = bufferSize;
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _leaveOpen = leaveOpen;
        _state = default;
    }

    /// <summary>
    /// Asynchronously reads a JSON array from the stream and yields each element as type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This method supports two JSON structures:
    /// <list type="bullet">
    /// <item><description>Array-root: <c>[{...}, {...}]</c></description></item>
    /// <item><description>Object-with-array: <c>{ "propertyName": [{...}, {...}], "pagination": {...} }</c></description></item>
    /// </list>
    /// If pagination metadata is found during streaming, it will be available via <see cref="GetPagination"/>.
    /// </remarks>
    /// <typeparam name="T">The type of elements to deserialize from the JSON array.</typeparam>
    /// <param name="propertyName">
    /// The name of the property containing the array when the JSON root is an object. 
    /// Common values are "items", "data", "results". If null or empty, expects array-root JSON.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An async enumerable sequence of deserialized elements of type <typeparamref name="T"/>.</returns>
    /// <exception cref="JsonException">Thrown when the JSON structure is invalid or doesn't match expectations.</exception>
    /// <exception cref="InvalidOperationException">Thrown when type information cannot be resolved.</exception>
    public async IAsyncEnumerable<T> ReadArrayAsync<T>(
        string? propertyName = "items",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        JsonTypeInfo<T> typeInfo = ResolveTypeInfo<T>();

        byte[]? propertyUtf8 = string.IsNullOrEmpty(propertyName)
            ? null
            : Encoding.UTF8.GetBytes(propertyName);

        bool insideTargetArray = false;
        bool foundRoot = false;
        int depth = 0;

        while (true)
        {
            // Refill buffer if needed
            if (_consumed >= _bytesInBuffer)
            {
                if (!await TryRefillBufferAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield break; // End of stream
                }
            }

            ReadOnlySpan<byte> availableData = new ReadOnlySpan<byte>(_buffer, _consumed, _bytesInBuffer - _consumed);
            bool isFinalBlock = _bytesInBuffer < _bufferSize;

            while (TryProcessChunk(availableData, isFinalBlock, typeInfo, propertyUtf8, ref insideTargetArray, ref foundRoot, ref depth, out T? item, out int bytesConsumed))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _consumed += bytesConsumed;

                if (item is not null)
                {
                    yield return item;
                }

                availableData = new ReadOnlySpan<byte>(_buffer, _consumed, _bytesInBuffer - _consumed);
            }

            // If we've finished the target array and aren't looking for more data, we can exit early
            if (!insideTargetArray && foundRoot && propertyUtf8 is not null)
            {
                // Continue reading to potentially find pagination metadata
                if (_paginationExtracted || isFinalBlock)
                {
                    yield break;
                }
            }
        }
    }

    private bool TryProcessChunk<T>(
        ReadOnlySpan<byte> availableData,
        bool isFinalBlock,
        JsonTypeInfo<T> typeInfo,
        byte[]? propertyUtf8,
        ref bool insideTargetArray,
        ref bool foundRoot,
        ref int depth,
        out T? item,
        out int bytesConsumed)
    {
        item = default;
        bytesConsumed = 0;

        var reader = new Utf8JsonReader(availableData, isFinalBlock, _state);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray when !foundRoot && propertyUtf8 is null:
                    // Array-root JSON
                    foundRoot = true;
                    insideTargetArray = true;
                    depth = reader.CurrentDepth;
                    break;

                case JsonTokenType.StartObject when !foundRoot:
                    foundRoot = true;
                    break;

                case JsonTokenType.PropertyName:
                    // Check if this is the target array property
                    if (propertyUtf8 is not null &&
                        reader.CurrentDepth == 1 &&
                        reader.ValueTextEquals(propertyUtf8))
                    {
                        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
                        {
                            throw new JsonException($"Property is not an array.");
                        }
                        insideTargetArray = true;
                        depth = reader.CurrentDepth;
                    }
                    // Check for pagination metadata
                    else if (!_paginationExtracted &&
                             reader.CurrentDepth == 1 &&
                             reader.ValueTextEquals("pagination"))
                    {
                        if (reader.Read())
                        {
                            _pagination = TryDeserializePagination(ref reader);
                            _paginationExtracted = true;
                        }
                    }
                    break;

                case JsonTokenType.StartObject when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.StartArray when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.String when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.Number when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.True when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.False when insideTargetArray && reader.CurrentDepth == depth + 1:
                case JsonTokenType.Null when insideTargetArray && reader.CurrentDepth == depth + 1:
                    // Deserialize array element
                    item = JsonSerializer.Deserialize(ref reader, typeInfo);
                    bytesConsumed = (int)reader.BytesConsumed;
                    _state = reader.CurrentState;
                    return true;

                case JsonTokenType.EndArray when insideTargetArray && reader.CurrentDepth == depth:
                    // End of target array
                    insideTargetArray = false;
                    break;
            }
        }

        // Update state for next iteration
        bytesConsumed = (int)reader.BytesConsumed;
        _state = reader.CurrentState;
        return false;
    }

    /// <summary>
    /// Gets the pagination metadata extracted from the JSON stream, if available.
    /// </summary>
    /// <returns>
    /// The <see cref="Pagination"/> metadata if found in the stream; otherwise, null.
    /// </returns>
    /// <remarks>
    /// This method returns the pagination metadata that was extracted during streaming via
    /// <see cref="ReadArrayAsync{T}"/>. It does not perform any I/O operations.
    /// </remarks>
#pragma warning disable CA1024 // Use properties where appropriate
    public Pagination? GetPagination() => _pagination;
#pragma warning restore CA1024 // Use properties where appropriate

    /// <summary>
    /// Asynchronously reads and extracts pagination metadata from the stream without deserializing array elements.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the pagination
    /// metadata if found; otherwise, null.
    /// </returns>
    /// <remarks>
    /// This method is useful when you only need pagination metadata without processing the data array.
    /// It will scan the stream looking for a "pagination" property at the root level.
    /// </remarks>
    public async ValueTask<Pagination?> ExtractPaginationAsync(CancellationToken cancellationToken = default)
    {
        if (_paginationExtracted)
        {
            return _pagination;
        }

        while (true)
        {
            if (_consumed >= _bytesInBuffer)
            {
                if (!await TryRefillBufferAsync(cancellationToken).ConfigureAwait(false))
                {
                    return null; // End of stream
                }
            }

            ReadOnlySpan<byte> availableData = new ReadOnlySpan<byte>(_buffer, _consumed, _bytesInBuffer - _consumed);
            bool isFinalBlock = _bytesInBuffer < _bufferSize;

            var reader = new Utf8JsonReader(availableData, isFinalBlock, _state);

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.TokenType == JsonTokenType.PropertyName &&
                    reader.CurrentDepth == 1 &&
                    reader.ValueTextEquals("pagination"))
                {
                    if (reader.Read())
                    {
                        _pagination = TryDeserializePagination(ref reader);
                        _paginationExtracted = true;
                        return _pagination;
                    }
                }
            }

            _consumed += (int)reader.BytesConsumed;
            _state = reader.CurrentState;

            if (isFinalBlock)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Releases the resources used by the <see cref="Utf8JsonStreamReader"/>.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        ArrayPool<byte>.Shared.Return(_buffer);

        if (_leaveOpen)
        {
            return ValueTask.CompletedTask;
        }

        return _stream.DisposeAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<bool> TryRefillBufferAsync(CancellationToken cancellationToken)
    {
        _bytesInBuffer = await _stream.ReadAsync(_buffer.AsMemory(0, _bufferSize), cancellationToken)
            .ConfigureAwait(false);
        _consumed = 0;
        return _bytesInBuffer > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsonTypeInfo<T> ResolveTypeInfo<T>()
    {
        JsonTypeInfo? typeInfo = _options.GetTypeInfo(typeof(T));

        if (typeInfo is not JsonTypeInfo<T> typedInfo)
        {
            throw new InvalidOperationException(
                $"Cannot resolve JsonTypeInfo for type {typeof(T)}. " +
                $"Ensure the type is registered in the JsonSerializerOptions.TypeInfoResolver.");
        }

        return typedInfo;
    }

    private Pagination? TryDeserializePagination(ref Utf8JsonReader reader)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            JsonTypeInfo<Pagination> paginationTypeInfo = ResolveTypeInfo<Pagination>();
            return JsonSerializer.Deserialize(ref reader, paginationTypeInfo);
        }
        catch (JsonException)
        {
            // If pagination deserialization fails, we continue without it
            return null;
        }
        catch (InvalidOperationException)
        {
            // Type info resolution failed
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}