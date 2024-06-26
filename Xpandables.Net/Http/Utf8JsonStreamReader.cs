﻿
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

// Ignore Spelling: Deserialize Json

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Xpandables.Net.Http;

// Taken from https://github.com/evil-dr-nick/utf8jsonstreamreader/blob/master/Utf8JsonStreamReader/Utf8JsonStreamReader.cs


internal ref struct Utf8JsonStreamReader(Stream stream, int bufferSize)
{
    private readonly Stream _stream = stream;
    // note: buffers will often be bigger than this -
    // do not ever use this number for calculations.
    private readonly int _bufferSize = bufferSize;

    private SequenceSegment? _firstSegment = null;
    private int _firstSegmentStartIndex = 0;
    private SequenceSegment? _lastSegment = null;
    private int _lastSegmentEndIndex = -1;

    private Utf8JsonReader _jsonReader = default;
    private bool _keepBuffers = false;
    private bool _isFinalBlock = false;

    public bool Read()
    {
        // read could be unsuccessful due to insufficient
        // buffer size, retrying in loop with additional buffer segments
        while (!_jsonReader.Read())
        {
            if (_isFinalBlock)
            {
                return false;
            }

            MoveNext();
        }
        return true;
    }

    private void MoveNext()
    {
        _firstSegmentStartIndex += (int)_jsonReader.BytesConsumed;

        // release previous segments if possible
        while (_firstSegmentStartIndex > 0
            && _firstSegment?.Memory.Length <= _firstSegmentStartIndex)
        {
            SequenceSegment currFirstSegment = _firstSegment;
            _firstSegmentStartIndex -= _firstSegment.Memory.Length;
            _firstSegment = (SequenceSegment?)_firstSegment.Next;
            if (!_keepBuffers)
            {
                currFirstSegment.Dispose();
            }
        }

        // create new segment
        SequenceSegment newSegment = new(_bufferSize, _lastSegment);
        _lastSegment?.SetNext(newSegment);
        _lastSegment = newSegment;

        if (_firstSegment == null)
        {
            _firstSegment = newSegment;
            _firstSegmentStartIndex = 0;
        }

        // read data from stream
        _lastSegmentEndIndex = 0;
        int bytesRead;
        do
        {
            bytesRead = _stream.Read(
                newSegment.Buffer.Memory.Span[_lastSegmentEndIndex..]);
            _lastSegmentEndIndex += bytesRead;
        } while (bytesRead > 0
        && _lastSegmentEndIndex < newSegment.Buffer.Memory.Length);
        _isFinalBlock = _lastSegmentEndIndex < newSegment.Buffer.Memory.Length;
        ReadOnlySequence<byte> data
            = new(_firstSegment, _firstSegmentStartIndex, _lastSegment,
            _lastSegmentEndIndex);
        _jsonReader =
            new Utf8JsonReader(data, _isFinalBlock, _jsonReader.CurrentState);
    }

    private void DeserializePost()
    {
        // release memory if possible
        SequenceSegment? firstSegment = _firstSegment;
        int firstSegmentStartIndex = _firstSegmentStartIndex
            + (int)_jsonReader.BytesConsumed;

        while (firstSegment?.Memory.Length < firstSegmentStartIndex)
        {
            firstSegmentStartIndex -= firstSegment.Memory.Length;
            firstSegment.Dispose();
            firstSegment = (SequenceSegment?)firstSegment.Next;
        }

        if (firstSegment != _firstSegment)
        {
            _firstSegment = firstSegment;
            _firstSegmentStartIndex = firstSegmentStartIndex;
            ReadOnlySequence<byte> data
                = new(_firstSegment!, _firstSegmentStartIndex, _lastSegment!,
                _lastSegmentEndIndex);
            _jsonReader =
                new Utf8JsonReader(data, _isFinalBlock, _jsonReader.CurrentState);
        }
    }

    private long DeserializePre(
        out SequenceSegment? firstSegment,
        out int firstSegmentStartIndex)
    {
        // JsonSerializer.Deserialize can read only a single object.
        // We have to extract
        // object to be deserialized into separate Utf8JsonReader.
        // This incurs one additional
        // pass through data (but data is only passed, not parsed).
        long tokenStartIndex = _jsonReader.TokenStartIndex;
        firstSegment = _firstSegment;
        firstSegmentStartIndex = _firstSegmentStartIndex;

        // loop through data until end of object is found
        _keepBuffers = true;
        int depth = 0;

        if (TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
        {
            depth++;
        }

        while (depth > 0 && Read())
        {
            if (TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            {
                depth++;
            }
            else if (TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray)
            {
                depth--;
            }
        }

        _keepBuffers = false;
        return tokenStartIndex;
    }

    [return: MaybeNull]
    public T Deserialize<T>(JsonSerializerOptions? options = null)
    {
        long tokenStartIndex = DeserializePre(
            out SequenceSegment? firstSegment,
            out int firstSegmentStartIndex);

        Utf8JsonReader newJsonReader =
            new(new ReadOnlySequence<byte>(
                firstSegment!,
                firstSegmentStartIndex,
                _lastSegment!,
                _lastSegmentEndIndex)
            .Slice(tokenStartIndex, _jsonReader.Position), true, default);

        // deserialize value
        T? result = JsonSerializer.Deserialize<T>(ref newJsonReader, options);

        DeserializePost();
        return result;
    }

    public JsonDocument GetJsonDocument()
    {
        long tokenStartIndex = DeserializePre(
            out SequenceSegment? firstSegment,
            out int firstSegmentStartIndex);

        Utf8JsonReader newJsonReader =
            new(new ReadOnlySequence<byte>(
                firstSegment!,
                firstSegmentStartIndex,
                _lastSegment!,
                _lastSegmentEndIndex)
            .Slice(tokenStartIndex, _jsonReader.Position), true, default);

        // deserialize value
        JsonDocument result = JsonDocument.ParseValue(ref newJsonReader);
        DeserializePost();
        return result;
    }

    public readonly void Dispose()
    {
        _stream?.Dispose();
        _firstSegment?.Dispose();
        _lastSegment?.Dispose();
    }

    public readonly int CurrentDepth => _jsonReader.CurrentDepth;
    public readonly bool HasValueSequence => _jsonReader.HasValueSequence;
    public readonly long TokenStartIndex => _jsonReader.TokenStartIndex;
    public readonly JsonTokenType TokenType => _jsonReader.TokenType;
    public readonly ReadOnlySequence<byte> ValueSequence
        => _jsonReader.ValueSequence;
    public readonly ReadOnlySpan<byte> ValueSpan => _jsonReader.ValueSpan;

    public bool GetBoolean() => _jsonReader.GetBoolean();
    public byte GetByte() => _jsonReader.GetByte();
    public byte[] GetBytesFromBase64() => _jsonReader.GetBytesFromBase64();
    public string GetComment() => _jsonReader.GetComment();
    public DateTime GetDateTime() => _jsonReader.GetDateTime();
    public DateTimeOffset GetDateTimeOffset()
        => _jsonReader.GetDateTimeOffset();
    public decimal GetDecimal() => _jsonReader.GetDecimal();
    public double GetDouble() => _jsonReader.GetDouble();
    public Guid GetGuid() => _jsonReader.GetGuid();
    public short GetInt16() => _jsonReader.GetInt16();
    public int GetInt32() => _jsonReader.GetInt32();
    public long GetInt64() => _jsonReader.GetInt64();
    public sbyte GetSByte() => _jsonReader.GetSByte();
    public float GetSingle() => _jsonReader.GetSingle();
    public string? GetString() => _jsonReader.GetString();
    public uint GetUInt32() => _jsonReader.GetUInt32();
    public ulong GetUInt64() => _jsonReader.GetUInt64();
    public bool TryGetDecimal(out byte value)
        => _jsonReader.TryGetByte(out value);
    public bool TryGetDecimal(out decimal value)
        => _jsonReader.TryGetDecimal(out value);
    public bool TryGetBytesFromBase64(out byte[]? value)
        => _jsonReader.TryGetBytesFromBase64(out value);
    public bool TryGetDateTime(out DateTime value)
        => _jsonReader.TryGetDateTime(out value);
    public bool TryGetDateTimeOffset(out DateTimeOffset value)
        => _jsonReader.TryGetDateTimeOffset(out value);
    public bool TryGetDouble(out double value)
        => _jsonReader.TryGetDouble(out value);
    public bool TryGetGuid(out Guid value)
        => _jsonReader.TryGetGuid(out value);
    public bool TryGetInt16(out short value)
        => _jsonReader.TryGetInt16(out value);
    public bool TryGetInt32(out int value)
        => _jsonReader.TryGetInt32(out value);
    public bool TryGetInt64(out long value)
        => _jsonReader.TryGetInt64(out value);
    public bool TryGetSByte(out sbyte value)
        => _jsonReader.TryGetSByte(out value);
    public bool TryGetSingle(out float value)
        => _jsonReader.TryGetSingle(out value);
    public bool TryGetUInt16(out ushort value)
        => _jsonReader.TryGetUInt16(out value);
    public bool TryGetUInt32(out uint value)
        => _jsonReader.TryGetUInt32(out value);
    public bool TryGetUInt64(out ulong value)
        => _jsonReader.TryGetUInt64(out value);

    private sealed class SequenceSegment
        : ReadOnlySequenceSegment<byte>, IDisposable
    {
        internal IMemoryOwner<byte> Buffer { get; }
        private SequenceSegment? Previous { get; }
        private bool _disposed;

        public SequenceSegment(int size, SequenceSegment? previous)
        {
            Buffer = MemoryPool<byte>.Shared.Rent(size);
            Previous = previous;

            Memory = Buffer.Memory;
            RunningIndex =
                previous?.RunningIndex + previous?.Memory.Length ?? 0;
        }

        public void SetNext(SequenceSegment next) => Next = next;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Buffer.Dispose();
            Previous?.Dispose();
        }
    }
}
