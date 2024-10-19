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
using System.Text.Json.Serialization;

namespace Xpandables.Net.Collections;

/// <summary>
/// Represents a collection of <see cref="ElementEntry"/> objects.
/// </summary>
[JsonConverter(typeof(ElementCollectionJsonConverter))]
public readonly record struct ElementCollection : IEnumerable<ElementEntry>
{
    private readonly List<ElementEntry> _entries = [];

    /// <summary>
    /// Adds an <see cref="ElementEntry"/> to the <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="collection">The collection to which the entry will be added.</param>
    /// <param name="entry">The entry to add to the collection.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry 
    /// added.</returns>
    public static ElementCollection operator +(
        ElementCollection collection,
        ElementEntry entry)
    {
        collection.Add(entry);
        return collection;
    }

    /// <summary>
    /// Merges two <see cref="ElementCollection"/> instances.
    /// </summary>
    /// <param name="collection">The first collection to merge.</param>
    /// <param name="other">The second collection to merge.</param>
    /// <returns>A new <see cref="ElementCollection"/> that contains the 
    /// entries from both collections.</returns>
    public static ElementCollection operator +(
        ElementCollection collection,
        ElementCollection other)
    {
        collection.Merge(other);
        return collection;
    }

    /// <summary>
    /// Removes the <see cref="ElementEntry"/> with the specified key from 
    /// the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entry.</param>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entry 
    /// removed.</returns>
    public static ElementCollection operator -(
        ElementCollection collection,
        string key)
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
    public static ElementCollection operator -(
        ElementCollection collection,
        ElementEntry entry)
    {
        _ = collection.Remove(entry.Key);
        return collection;
    }

    /// <summary>
    /// Removes the specified <see cref="ElementEntry"/> objects from the collection.
    /// </summary>
    /// <param name="collection">The collection from which to remove the entries.</param>
    /// <param name="other">The collection containing the entries to remove.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified entries removed.</returns>
    public static ElementCollection operator -(
        ElementCollection collection,
        ElementCollection other)
    {
        foreach (ElementEntry entry in other)
        {
            _ = collection.Remove(entry.Key);
        }

        return collection;
    }

    /// <summary>
    /// Implicitly converts an <see cref="ElementCollection"/> to 
    /// a <see cref="ReadOnlyDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="collection">The <see cref="ElementCollection"/> to convert.</param>
    /// <returns>A <see cref="ReadOnlyDictionary{TKey, TValue}"/> that contains 
    /// the entries from the collection.</returns>
    public static implicit operator
        ReadOnlyDictionary<string, IReadOnlyCollection<string>>(
        ElementCollection collection) =>
        new(collection._entries.ToDictionary(
            entry => entry.Key, entry => entry.Values));

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
    public static ElementCollection With(string key, params string[] values) =>
       new(key, values);

    /// <summary>
    /// Creates a new <see cref="ElementCollection"/> with the specified key 
    /// and value.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns>A new <see cref="ElementCollection"/> with the specified key 
    /// and value.</returns>
    public static ElementCollection With(string key, string value) =>
        new(key, value);

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
    public static ElementCollection With(IList<ElementEntry> entries) =>
        new(entries);

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
    public ElementEntry? this[string key] =>
        _entries.Find(entry => entry.Key == key) is { Key: not null } entry
            ? entry
            : null;

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
                Values = existingEntry.Value.Values.Union(entry.Values).ToArray()
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
    public void Add(string key, params string[] values) =>
        Add(new ElementEntry(key, values));

    /// <summary>
    /// Adds a range of key-value pairs to the collection.
    /// If an entry with the same key already exists,
    /// it merges the values of the existing entry with the new values.
    /// </summary>
    /// <param name="values">The dictionary of key-value pairs to add.</param>
    public void AddRange(IDictionary<string, string> values)
    {
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
    public int Remove(string key) =>
        _entries.RemoveAll(entry => entry.Key == key);

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
        foreach (ElementEntry entry in collection)
        {
            Add(entry);
        }
    }

    /// <summary>  
    /// Clears all the entries from the collection.  
    /// </summary>  
    public void Clear() => _entries.Clear();

    /// <inheritdoc/>
    public IEnumerator<ElementEntry> GetEnumerator() =>
        _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    internal ElementCollection(ElementEntry entry) => Add(entry);
    internal ElementCollection(string key, params string[] values)
        : this(new ElementEntry(key, values)) { }
    [JsonConstructor]
    internal ElementCollection(IList<ElementEntry> entries)
    {
        _entries ??= [];

        foreach (ElementEntry entry in entries)
        {
            Add(entry);
        }
    }
}
