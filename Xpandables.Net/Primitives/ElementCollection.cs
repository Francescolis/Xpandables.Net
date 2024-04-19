
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
using System.Collections;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Represents a collection of <see cref="ElementEntry"/>s.
/// </summary>
[JsonConverter(typeof(ElementCollectionJsonConverter))]
public readonly record struct ElementCollection : IEnumerable<ElementEntry>
{
    private readonly List<ElementEntry> _items = [];

    /// <summary>
    /// Creates a new instance of <see cref="ElementCollection"/> with the 
    /// specified key and value.
    /// </summary>
    /// <param name="key">The key associated with the element.</param>
    /// <param name="value">The string that represents the value.</param>
    /// <returns>A new instance of <see cref="ElementCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> 
    /// or <paramref name="value"/> is null.</exception>"
    public static ElementCollection With(string key, string value)
        => new(key, [value]);

    /// <summary>
    /// Creates a new instance of <see cref="ElementCollection"/> with the
    /// specified key and values.
    /// </summary>
    /// <param name="key">The key associated with the element.</param>
    /// <param name="values">The collection of strings that represents the 
    /// values.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> or
    /// <paramref name="values"/> is null.</exception>
    public static ElementCollection With(string key, params string[] values)
        => new(key, values);

    /// <summary>
    /// Creates a new instance of <see cref="ElementCollection"/> with the
    /// specified element.
    /// </summary>
    /// <param name="entry">The element to add.</param>
    /// <returns>A new instance of <see cref="ElementCollection"/>.</returns>
    public static ElementCollection With(ElementEntry entry) => new(entry);

    /// <summary>
    /// Creates a new instance of <see cref="ElementCollection"/> with the
    /// specified elements.
    /// </summary>
    /// <param name="entries">The elements to add.</param>
    /// <returns>A new instance of <see cref="ElementCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="entries"/> 
    /// is null.</exception>"
    public static ElementCollection With(IList<ElementEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return new(entries.ToList());
    }

    /// <summary>
    /// Constructs a new instance of <see cref="ElementCollection"/>.
    /// </summary>
    public ElementCollection() => _items = [];

    internal ElementCollection(ElementEntry element) => Add(element);
    internal ElementCollection(string key, string[] values)
        : this(new ElementEntry(key, values)) { }

    [JsonConstructor]
    internal ElementCollection(IList<ElementEntry> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items ??= [];

        foreach (ElementEntry item in items)
        {
            _items.Add(item);
        }
    }

    /// <summary>
    ///  Gets the element at the specified index.
    ///  </summary>
    ///  <param name="key">The index name of the element to get.</param>
    ///  <returns>The element at the specified index.</returns>
    public ElementEntry? this[string key]
        => TryGet(key, out ElementEntry? element)
            ? element
            : null;

    /// <summary>
    /// Adds the specified element to the collection.
    /// </summary>
    /// <param name="element">element to add.</param>
    public void Add(ElementEntry element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (_items.Find(i => i.Key.Equals(
            element.Key,
            StringComparison.OrdinalIgnoreCase)) is { Key: { } } existsItem)
        {
            _ = _items.Remove(existsItem);
            element = existsItem = existsItem with
            {
                Values = existsItem.Values.Union(element.Values).ToArray()
            };
        }

        _items.Add(element);
    }

    /// <summary>
    /// Adds the specified elements to the collection.
    /// </summary>
    /// <param name="key">the element key.</param>
    /// <param name="values">the element value.</param>
    public void Add(string key, params string[] values)
        => Add(new ElementEntry(key, values));

    /// <summary>
    /// Merges the specified elements into the current collection.
    /// </summary>
    /// <param name="elements">elements to merge</param>
    public void Merge(ElementCollection elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        foreach (ElementEntry item in elements)
        {
            _items.Add(item);
        }
    }

    internal readonly void Clear() => _items?.Clear();

    ///<inheritdoc/>
    public IEnumerator<ElementEntry> GetEnumerator()
        => _items.GetEnumerator();

    /// <summary>
    /// Tries to get the element with the specified key.
    /// If the element is found, the method returns <see langword="true"/>,
    /// otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="result">The element associated with the key.</param>
    /// <returns><see langword="true"/> if the element is found; otherwise, 
    /// <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is 
    /// null.</exception>"
    public bool TryGet(string key, out ElementEntry? result)
    {
        ArgumentNullException.ThrowIfNull(key);

        result = _items.Find(i => i.Key.Equals(
            key,
            StringComparison.OrdinalIgnoreCase));

        return result.HasValue;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}