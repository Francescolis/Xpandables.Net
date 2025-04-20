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

using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.Collections;

/// <summary>
/// Represents a collection of ElementEntry objects, allowing for addition, removal, and merging of entries. Provides
/// methods to create, clear, and convert the collection.
/// </summary>
[JsonConverter(typeof(ElementCollectionJsonConverter))]
public readonly record struct ElementCollection : IEnumerable<ElementEntry>
{
    private readonly List<ElementEntry> _entries;

    /// <summary>
    /// Gets an empty <see cref="ElementCollection"/>.
    /// </summary>
    public static ElementCollection Empty { get; } = [];

    /// <summary>  
    /// Creates a new <see cref="ElementCollection"/> with the specified key 
    /// and values.  
    /// </summary>  
    /// <param name="key">The key of the element to add.</param>  
    /// <param name="values">The values of the element to add.</param>  
    /// <returns>A new <see cref="ElementCollection"/> with the specified key 
    /// and values.</returns>  
    public static ElementCollection With(string key, params string[] values) => new(key, values);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified key 
    /// and value.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified key 
    /// and value.</returns>
    public static ElementCollection With(string key, string value) => new(key, value);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified 
    /// <see cref="ElementEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="ElementEntry"/> to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified 
    /// <see cref="ElementEntry"/>.</returns>
    public static ElementCollection With(ElementEntry entry) => new(entry);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified list 
    /// of <see cref="ElementEntry"/>.
    /// </summary>
    /// <param name="entries">The list of <see cref="ElementEntry"/> to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified list 
    /// of <see cref="ElementEntry"/>.</returns>
    public static ElementCollection With(IList<ElementEntry> entries) => [.. entries];

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> struct.
    /// </summary>
    public ElementCollection() => _entries = [];

    /// <summary>
    /// Gets the <see cref="ElementEntry"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <returns>The <see cref="ElementEntry"/> associated with the specified 
    /// key, or <c>null</c> if the key is not found.</returns>
    public ElementEntry? this[string key]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            for (int i = 0; i < _entries.Count; i++)
            {
                if (string.Equals(_entries[i].Key, key, StringComparison.Ordinal))
                {
                    return _entries[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the collection. 
    /// If an entry with the same key already exists,
    /// it merges the values of the existing entry with the new entry.
    /// </summary>
    /// <param name="entry">The <see cref="ElementEntry"/> to add.</param>
    public void Add(ElementEntry entry)
    {
        ElementEntry? existingEntry = this[entry.Key];
        if (existingEntry.HasValue)
        {
            _ = _entries.Remove(existingEntry.Value);
            entry = existingEntry.Value with
            {
                Values = StringValues.Concat(existingEntry.Value.Values, entry.Values)
            };
        }

        _entries.Add(entry);
    }

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the collection with the specified 
    /// key and values.
    /// If an entry with the same key already exists,
    /// it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="values">The values of the element to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string key, params string[] values) => Add(new ElementEntry(key, values));

    /// <summary>
    /// Adds a range of key-value pairs to the collection.
    /// If an entry with the same key already exists,
    /// it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="values">The dictionary of key-value pairs to add.</param>
    public void AddRange(IDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _entries.EnsureCapacity(_entries.Count + values.Count);

        foreach (KeyValuePair<string, string> value in values)
        {
            Add(value.Key, value.Value);
        }
    }

    /// <summary>
    /// Removes the <see cref="ElementEntry"/> with the specified key from the 
    /// collection.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>The number of elements removed from the collection.</returns>
    public int Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        int removed = 0;
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (string.Equals(_entries[i].Key, key, StringComparison.Ordinal))
            {
                _entries.RemoveAt(i);
                removed++;
            }
        }

        return removed;
    }

    /// <summary>
    /// Merges the specified <see cref="ElementCollection"/> with the current 
    /// collection.
    /// If an entry with the same key already exists, it merges the values of 
    /// the existing entry with the new entry.
    /// </summary>
    /// <param name="collection">The <see cref="ElementCollection"/> to merge 
    /// with the current collection.</param>
    public void Merge(ElementCollection collection)
    {
        _entries.EnsureCapacity(_entries.Count + collection._entries.Count);

        foreach (ElementEntry entry in collection)
        {
            Add(entry);
        }
    }

    /// <summary>  
    /// Clears all the entries from the collection.  
    /// </summary>  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _entries.Clear();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<ElementEntry> GetEnumerator() => _entries.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal ElementCollection(ElementEntry entry)
    {
        _entries = [];
        Add(entry);
    }

    internal ElementCollection(string key, params string[] values)
        : this(new ElementEntry(key, values))
    {
    }

    [JsonConstructor]
    internal ElementCollection(IList<ElementEntry> entries)
    {
        _entries = [];

        _entries.EnsureCapacity(entries.Count);

        foreach (ElementEntry entry in entries)
        {
            Add(entry);
        }
    }

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="collection">The collection to which the entry will be added.</param>
    /// <param name="entry">The entry to add to the collection.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry 
    /// added.</returns>
    public static ElementCollection operator +(ElementCollection collection, ElementEntry entry)
    {
        collection.Add(entry);
        return collection;
    }

    /// <summary>
    /// Merges two <see cref="ElementCollection"/> instances.
    /// </summary>
    /// <param name="left">The first collection to merge.</param>
    /// <param name="right">The second collection to merge.</param>
    /// <returns>A new <see cref="ElementCollection"/> that contains the 
    /// entries from both collections.</returns>
    public static ElementCollection operator +(ElementCollection left, ElementCollection right)
    {
        left.Merge(right);
        return left;
    }

    /// <summary>
    /// Removes the <see cref="ElementEntry"/> with the specified key from 
    /// the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entry.</param>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry 
    /// removed.</returns>
    public static ElementCollection operator -(ElementCollection collection, string key)
    {
        _ = collection.Remove(key);
        return collection;
    }

    /// <summary>
    /// Removes the specified <see cref="ElementEntry"/> from the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entry.</param>
    /// <param name="entry">The entry to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry 
    /// removed.</returns>
    public static ElementCollection operator -(ElementCollection collection, ElementEntry entry)
    {
        _ = collection.Remove(entry.Key);
        return collection;
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
        Dictionary<string, StringValues> dict = new(collection._entries.Count);

        foreach (var entry in collection._entries)
        {
            dict[entry.Key] = entry.Values;
        }

        return new ReadOnlyDictionary<string, StringValues>(dict);
    }

    /// <summary>
    /// Removes elements from a collection based on another collection's entries. The modified collection is returned
    /// after the operation.
    /// </summary>
    /// <param name="left">The collection from which elements will be removed.</param>
    /// <param name="right">The collection containing entries that specify which elements to remove.</param>
    /// <returns>The updated collection after specified elements have been removed.</returns>
    public static ElementCollection Subtract(ElementCollection left, ElementCollection right)
    {
        foreach (ElementEntry entry in right)
        {
            _ = left.Remove(entry.Key);
        }

        return left;
    }

    /// <summary>
    /// Converts a collection of entries into a read-only dictionary with string keys and StringValues.
    /// </summary>
    /// <returns>Returns a ReadOnlyDictionary containing the entries.</returns>
    public ReadOnlyDictionary<string, StringValues> ToReadOnlyDictionary()
    {
        Dictionary<string, StringValues> dict = new(_entries.Count);

        foreach (var entry in _entries)
        {
            dict[entry.Key] = entry.Values;
        }

        return new ReadOnlyDictionary<string, StringValues>(dict);
    }

    /// <summary>
    /// Returns a string representation of the collection, where entries are formatted as "key=value1,value2" 
    /// and separated by semicolons.
    /// </summary>
    /// <returns>A string representation of the collection. Returns an empty string if the collection is empty.</returns>
    public override string ToString()
    {
        if (_entries == null || _entries.Count == 0)
            return string.Empty;

        var builder = new System.Text.StringBuilder();

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];

            if (i > 0)
                builder.Append(';');

            builder.Append(entry.Key);
            builder.Append('=');

            if (entry.Values.Count > 0)
            {
                for (int j = 0; j < entry.Values.Count; j++)
                {
                    if (j > 0)
                        builder.Append(',');
                    builder.Append(entry.Values[j]);
                }
            }
        }

        return builder.ToString();
    }
}