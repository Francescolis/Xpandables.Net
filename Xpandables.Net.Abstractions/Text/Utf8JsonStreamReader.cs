namespace Xpandables.Net.Text;

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 
/// </summary>
public sealed class Utf8JsonStreamReader : IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private int _bytesInBuffer;
    private int _consumed;
    private JsonReaderState _state;
    private readonly JsonSerializerOptions _options;
    private Pagination? _pagination;

    // Cache for resolved JsonTypeInfo<T>
    private static readonly ConcurrentDictionary<(JsonSerializerOptions, Type), JsonTypeInfo> _typeInfoCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <param name="bufferSize"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Utf8JsonStreamReader(Stream stream, JsonSerializerOptions? options = null, int bufferSize = 16 * 1024)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonException"></exception>
    public async IAsyncEnumerable<T> ReadArrayAsync<T>(
        string propertyName,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var typeInfo = (JsonTypeInfo<T>)_typeInfoCache.GetOrAdd(
            (_options, typeof(T)),
            key =>
            {
                var info = key.Item1.GetTypeInfo(key.Item2);
                if (info is null)
                    throw new InvalidOperationException($"No JsonTypeInfo for {key.Item2}");
                return info;
            });

        var propertyUtf8 = Encoding.UTF8.GetBytes(propertyName);

        while (true)
        {
            if (_consumed >= _bytesInBuffer)
            {
                _bytesInBuffer = await _stream.ReadAsync(_buffer, ct).ConfigureAwait(false);
                _consumed = 0;
                if (_bytesInBuffer == 0) yield break;
            }

            var reader = new Utf8JsonReader(
                new ReadOnlySequence<byte>(_buffer, _consumed, _bytesInBuffer - _consumed),
                isFinalBlock: _bytesInBuffer < _buffer.Length,
                _state);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(propertyUtf8))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new JsonException($"Property '{propertyName}' is not an array.");

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        yield return JsonSerializer.Deserialize(ref reader, typeInfo)!;
                    }
                }

                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("pageContext"u8))
                {
                    reader.Read();
                    _pagination = JsonSerializer.Deserialize(ref reader,
                        (JsonTypeInfo<Pagination>)_typeInfoCache.GetOrAdd((_options, typeof(Pagination)),
                            key => key.Item1.GetTypeInfo(key.Item2)!));
                }
            }

            _consumed += (int)reader.BytesConsumed;
            _state = reader.CurrentState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
#pragma warning disable CA1024 // Use properties where appropriate
    public Pagination? GetPageContext() => _pagination;
#pragma warning restore CA1024 // Use properties where appropriate

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ValueTask DisposeAsync()
    {
        ArrayPool<byte>.Shared.Return(_buffer);
        return _stream.DisposeAsync();
    }
}