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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Primitives;

namespace System.Collections;

/// <summary>
/// Represents a collection of ElementEntry objects, allowing for addition, removal, and merging of entries. 
/// Provides methods to create, clear, and convert the collection with optimized performance for .NET 10.
/// </summary>
/// <remarks>This collection provides O(1) lookup performance through internal indexing and is optimized 
/// for scenarios where entries are frequently accessed by key. All mutating operations ensure consistency 
/// and merge values when duplicate keys are encountered.</remarks>
[JsonConverter(typeof(ElementCollectionJsonConverterFactory))]
public readonly record struct ElementCollection : IEnumerable<ElementEntry>, IReadOnlyCollection<ElementEntry>
{
    private readonly List<ElementEntry> _entries;
    private readonly Dictionary<string, int>? _keyIndex;

    /// <summary>
    /// Gets an empty <see cref="ElementCollection"/>.
    /// </summary>
    public static ElementCollection Empty { get; } = [];

    /// <summary>
    /// Gets the number of entries in the collection.
    /// </summary>
    public int Count => _entries?.Count ?? 0;

    /// <summary>
    /// Gets a value indicating whether this collection is empty.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Count == 0;

    /// <summary>  
    /// Creates a new <see cref="ElementCollection"/> with the specified key and values.  
    /// </summary>  
    /// <param name="key">The key of the element to add.</param>  
    /// <param name="values">The values of the element to add.</param>  
    /// <returns>A new <see cref="ElementCollection"/> with the specified key and values.</returns>  
    public static ElementCollection With(string key, params string[] values) => new(key, values);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified key and value.</returns>
    public static ElementCollection With(string key, string value) => new(key, value);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified <see cref="ElementEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="ElementEntry"/> to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified <see cref="ElementEntry"/>.</returns>
    public static ElementCollection With(ElementEntry entry) => new(entry);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified list of <see cref="ElementEntry"/>.
    /// </summary>
    /// <param name="entries">The list of <see cref="ElementEntry"/> to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified list of <see cref="ElementEntry"/>.</returns>
    public static ElementCollection With(IEnumerable<ElementEntry> entries) => [.. entries];

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> from a dictionary of key-value pairs.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert to an ElementCollection.</param>
    /// <returns>A new <see cref="ElementCollection"/> containing the dictionary entries.</returns>
    public static ElementCollection FromDictionary(IReadOnlyDictionary<string, StringValues> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        var collection = new ElementCollection(dictionary.Count);
        foreach (var kvp in dictionary)
        {
            collection.AddInternal(new ElementEntry { Key = kvp.Key, Values = kvp.Value });
        }
        return collection;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> struct.
    /// </summary>
    public ElementCollection()
    {
        _entries = [];
        _keyIndex = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> struct with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the collection.</param>
    private ElementCollection(int capacity)
    {
        _entries = new List<ElementEntry>(capacity);
        _keyIndex = new Dictionary<string, int>(capacity, StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the <see cref="ElementEntry"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <returns>The <see cref="ElementEntry"/> associated with the specified key, or <c>null</c> if the key is not found.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public ElementEntry? this[string key]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            return TryGetValue(key, out var entry) ? entry : null;
        }
    }

    /// <summary>
    /// Tries to get the <see cref="ElementEntry"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="entry">When this method returns, contains the entry associated with the specified key, if the key is found; otherwise, the default value.</param>
    /// <returns><c>true</c> if the collection contains an entry with the specified key; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(string key, out ElementEntry entry)
    {
        if (_keyIndex?.TryGetValue(key, out var index) == true && index < _entries.Count)
        {
            entry = _entries[index];
            return true;
        }

        entry = default;
        return false;
    }

    /// <summary>
    /// Determines whether the collection contains an entry with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns><c>true</c> if the collection contains an entry with the specified key; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(string key) => _keyIndex?.ContainsKey(key) == true;

    /// <summary>
    /// Gets all keys in the collection.
    /// </summary>
    /// <returns>An enumerable of all keys in the collection.</returns>
    public IEnumerable<string> Keys => _entries?.Select(e => e.Key) ?? [];

    /// <summary>
    /// Gets all values in the collection.
    /// </summary>
    /// <returns>An enumerable of all StringValues in the collection.</returns>
    public IEnumerable<StringValues> Values => _entries?.Select(e => e.Values) ?? [];

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the collection. 
    /// If an entry with the same key already exists, it merges the values of the existing entry with the new entry.
    /// </summary>
    /// <param name="entry">The <see cref="ElementEntry"/> to add.</param>
    /// <exception cref="ArgumentException">Thrown when the entry key is null or empty.</exception>
    public void Add(ElementEntry entry)
    {
        ArgumentException.ThrowIfNullOrEmpty(entry.Key);

        if (_keyIndex?.TryGetValue(entry.Key, out var existingIndex) == true)
        {
            // Merge with existing entry
            var existingEntry = _entries[existingIndex];
            var mergedEntry = existingEntry with { Values = StringValues.Concat(existingEntry.Values, entry.Values) };
            _entries[existingIndex] = mergedEntry;
        }
        else
        {
            AddInternal(entry);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddInternal(ElementEntry entry)
    {
        var index = _entries.Count;
        _entries.Add(entry);
        _keyIndex![entry.Key] = index;
    }

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the collection with the specified key and values.
    /// If an entry with the same key already exists, it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="values">The values of the element to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string key, params string[] values) => Add(new ElementEntry(key, values));

    /// <summary>
    /// Adds a single value to an existing key or creates a new entry if the key doesn't exist.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <param name="value">The single value to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string key, string value) => Add(new ElementEntry(key, value));

    /// <summary>
    /// Adds a range of key-value pairs to the collection.
    /// If an entry with the same key already exists, it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="values">The dictionary of key-value pairs to add.</param>
    public void AddRange(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _entries.EnsureCapacity(_entries.Count + values.Count);

        foreach (var kvp in values)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Adds a range of key-StringValues pairs to the collection.
    /// If an entry with the same key already exists, it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="values">The dictionary of key-StringValues pairs to add.</param>
    public void AddRange(IReadOnlyDictionary<string, StringValues> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _entries.EnsureCapacity(_entries.Count + values.Count);

        foreach (var kvp in values)
        {
            Add(new ElementEntry { Key = kvp.Key, Values = kvp.Value });
        }
    }

    /// <summary>
    /// Adds a range of ElementEntry objects to the collection.
    /// If an entry with the same key already exists, it merges the values.
    /// </summary>
    /// <param name="entries">The entries to add.</param>
    public void AddRange(IEnumerable<ElementEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries is ICollection<ElementEntry> collection)
        {
            _entries.EnsureCapacity(_entries.Count + collection.Count);
        }

        foreach (var entry in entries)
        {
            Add(entry);
        }
    }

    /// <summary>
    /// Removes the <see cref="ElementEntry"/> with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns><c>true</c> if the element was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_keyIndex?.TryGetValue(key, out var index) == true)
        {
            _entries.RemoveAt(index);
            _keyIndex.Remove(key);

            // Rebuild index for items after removed index
            RebuildIndexFrom(index);
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RebuildIndexFrom(int startIndex)
    {
        for (int i = startIndex; i < _entries.Count; i++)
        {
            _keyIndex![_entries[i].Key] = i;
        }
    }

    /// <summary>
    /// Merges the specified <see cref="ElementCollection"/> with the current collection.
    /// If an entry with the same key already exists, it merges the values of the existing entry with the new entry.
    /// </summary>
    /// <param name="collection">The <see cref="ElementCollection"/> to merge with the current collection.</param>
    public void Merge(ElementCollection collection)
    {
        if (collection.IsEmpty) return;

        _entries.EnsureCapacity(_entries.Count + collection.Count);

        foreach (var entry in collection)
        {
            Add(entry);
        }
    }

    /// <summary>  
    /// Clears all the entries from the collection.  
    /// </summary>  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _entries.Clear();
        _keyIndex?.Clear();
    }

    /// <summary>
    /// Creates a shallow copy of the current collection.
    /// </summary>
    /// <returns>A new <see cref="ElementCollection"/> that is a copy of the current collection.</returns>
    public ElementCollection Copy()
    {
        if (IsEmpty) return Empty;

        var copy = new ElementCollection(_entries.Count);
        foreach (var entry in _entries)
        {
            copy.AddInternal(entry);
        }
        return copy;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<ElementEntry> GetEnumerator() => (_entries ?? []).GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal ElementCollection(ElementEntry entry) : this(1)
    {
        AddInternal(entry);
    }

    internal ElementCollection(string key, params string[] values)
        : this(new ElementEntry(key, values))
    {
    }

    [JsonConstructor]
    internal ElementCollection(IEnumerable<ElementEntry> entries) : this()
    {
        if (entries is not null)
        {
            if (entries is ICollection<ElementEntry> collection)
            {
                _entries.EnsureCapacity(collection.Count);
            }

            foreach (var entry in entries)
            {
                Add(entry);
            }
        }
    }

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="collection">The collection to which the entry will be added.</param>
    /// <param name="entry">The entry to add to the collection.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry added.</returns>
    public static ElementCollection operator +(ElementCollection collection, ElementEntry entry)
    {
        var result = collection.Copy();
        result.Add(entry);
        return result;
    }

    /// <summary>
    /// Merges two <see cref="ElementCollection"/> instances.
    /// </summary>
    /// <param name="left">The first collection to merge.</param>
    /// <param name="right">The second collection to merge.</param>
    /// <returns>A new <see cref="ElementCollection"/> that contains the entries from both collections.</returns>
    public static ElementCollection operator +(ElementCollection left, ElementCollection right)
    {
        if (left.IsEmpty) return right.Copy();
        if (right.IsEmpty) return left.Copy();

        var result = left.Copy();
        result.Merge(right);
        return result;
    }

    /// <summary>
    /// Removes the <see cref="ElementEntry"/> with the specified key from the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entry.</param>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry removed.</returns>
    public static ElementCollection operator -(ElementCollection collection, string key)
    {
        var result = collection.Copy();
        result.Remove(key);
        return result;
    }

    /// <summary>
    /// Removes the specified <see cref="ElementEntry"/> from the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entry.</param>
    /// <param name="entry">The entry to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry removed.</returns>
    public static ElementCollection operator -(ElementCollection collection, ElementEntry entry)
    {
        var result = collection.Copy();
        result.Remove(entry.Key);
        return result;
    }

    /// <summary>
    /// Removes the specified <see cref="ElementEntry"/> objects from the collection.
    /// </summary>
    /// <param name="left">The collection from which to remove the entries.</param>
    /// <param name="right">The collection containing the entries to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entries removed.</returns>
    public static ElementCollection operator -(ElementCollection left, ElementCollection right) =>
        Subtract(left, right);

    /// <summary>
    /// Converts an ElementCollection into a ReadOnlyDictionary with string keys and StringValues.
    /// </summary>
    /// <param name="collection">Holds a collection of entries that are transformed into a dictionary format.</param>
    public static implicit operator ReadOnlyDictionary<string, StringValues>(ElementCollection collection)
    {
        if (collection.IsEmpty)
            return new ReadOnlyDictionary<string, StringValues>(new Dictionary<string, StringValues>());

        var dict = new Dictionary<string, StringValues>(collection.Count, StringComparer.Ordinal);
        foreach (var entry in collection._entries)
        {
            dict[entry.Key] = entry.Values;
        }

        return new ReadOnlyDictionary<string, StringValues>(dict);
    }

    /// <summary>
    /// Removes elements from a collection based on another collection's entries. 
    /// The modified collection is returned after the operation.
    /// </summary>
    /// <param name="left">The collection from which elements will be removed.</param>
    /// <param name="right">The collection containing entries that specify which elements to remove.</param>
    /// <returns>The updated collection after specified elements have been removed.</returns>
    public static ElementCollection Subtract(ElementCollection left, ElementCollection right)
    {
        if (left.IsEmpty || right.IsEmpty) return left.Copy();

        var result = left.Copy();
        foreach (var entry in right)
        {
            result.Remove(entry.Key);
        }

        return result;
    }

    /// <summary>
    /// Converts a collection of entries into a read-only dictionary with string keys and StringValues.
    /// </summary>
    /// <returns>Returns a ReadOnlyDictionary containing the entries.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyDictionary<string, StringValues> ToReadOnlyDictionary() => this;

    /// <summary>
    /// Converts the collection to a regular Dictionary.
    /// </summary>
    /// <returns>A new Dictionary containing the entries.</returns>
    public Dictionary<string, StringValues> ToDictionary()
    {
        if (IsEmpty) return [];

        var dict = new Dictionary<string, StringValues>(Count, StringComparer.Ordinal);
        foreach (var entry in _entries)
        {
            dict[entry.Key] = entry.Values;
        }
        return dict;
    }

    /// <summary>
    /// Returns a string representation of the collection, where entries are formatted as "key=value1,value2" 
    /// and separated by semicolons.
    /// </summary>
    /// <returns>A string representation of the collection. Returns an empty string if the collection is empty.</returns>
    public override string ToString()
    {
        if (IsEmpty) return string.Empty;

        var estimatedCapacity = Count * 32;
        var builder = new StringBuilder(estimatedCapacity);
        bool first = true;

        foreach (var entry in _entries)
        {
            if (!first) builder.Append(Environment.NewLine);
            first = false;

            builder.Append(entry.Key);
            builder.Append('=');

            if (entry.Values.Count > 0)
            {
                builder.Append(entry.Values.StringJoin(","));
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns a detailed string representation useful for debugging.
    /// </summary>
    /// <returns>A detailed string representation of the collection.</returns>
    public string ToDebugString()
    {
        if (IsEmpty) return "ElementCollection { Empty }";

        var estimatedCapacity = Count * 32;
        var builder = new StringBuilder(estimatedCapacity);
        builder.Append(CultureInfo.InvariantCulture, $"ElementCollection {{ Count = {Count}, Entries = [ ");

        bool first = true;
        foreach (var entry in _entries)
        {
            if (!first) builder.Append(Environment.NewLine);
            first = false;
            builder.Append(CultureInfo.InvariantCulture, $"{{ Key = \"{entry.Key}\", Values = [{entry.Values.StringJoin(", ")}] }}");
        }

        builder.Append(" ] }");
        return builder.ToString();
    }
}

/// <summary>
/// Provides a source generation context for serializing and deserializing ElementCollection objects using System.Text.Json.
/// </summary>
/// <remarks>This context enables efficient, compile-time generation of serialization logic for the ElementCollection
/// type and its associated ElementEntry and StringValues types. Use this context with System.Text.Json serialization APIs 
/// to improve performance and reduce runtime reflection when working with ElementCollection instances. This approach is 
/// preferred over custom JsonConverter implementations for better AOT compatibility and performance in .NET 10.</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ElementCollection))]
[JsonSerializable(typeof(ElementCollection[]))]
[JsonSerializable(typeof(List<ElementCollection>))]
[JsonSerializable(typeof(IEnumerable<ElementCollection>))]
[JsonSerializable(typeof(Dictionary<string, ElementCollection>))]
[JsonSerializable(typeof(ElementEntry))]
[JsonSerializable(typeof(ElementEntry[]))]
[JsonSerializable(typeof(List<ElementEntry>))]
[JsonSerializable(typeof(IEnumerable<ElementEntry>))]
[JsonSerializable(typeof(StringValues))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(ReadOnlyDictionary<string, StringValues>))]
[JsonSerializable(typeof(Dictionary<string, StringValues>))]
public partial class ElementCollectionContext : JsonSerializerContext { }