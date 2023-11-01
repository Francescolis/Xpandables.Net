﻿
/************************************************************************************************************
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
************************************************************************************************************/
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Xpandables.Net.Operations;

/// <summary>
/// Represents a wrapper for collections.
/// </summary>
[JsonConverter(typeof(OperationResultCollectionJsonConverter))]
public readonly record struct ElementCollection : IEnumerable<ElementEntry>
{
    private readonly List<ElementEntry> _items = [];

    ///<inheritdoc/>
    public static ElementCollection With(string key, string value) => new(key, [value]);
    ///<inheritdoc/>
    public static ElementCollection With(string key, params string[] values) => new(key, values);
    ///<inheritdoc/>
    public static ElementCollection With(ElementEntry header) => new(header);
    ///<inheritdoc/>
    public static ElementCollection With(IList<ElementEntry> headers) => new(headers.ToList());

    ///<inheritdoc/>
    public ElementCollection() => _items = [];
    internal ElementCollection(ElementEntry element) => Add(element);
    internal ElementCollection(string key, string[] values) : this(new ElementEntry(key, values)) { }
    [JsonConstructor]
    internal ElementCollection(IList<ElementEntry> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items ??= [];

        foreach (var item in items)
        {
            _items.Add(item);
        }
    }

    /// <inheritdoc/>
    public ElementEntry? this[string key] => TryGet(key, out var element) ? element : null;

    /// <summary>
    /// Determines whether or not the errors collection contains an undefined key. Mostly used for exception.
    /// if so, returns the error.
    /// </summary>
    /// <param name="elementEntry">the output error if found.</param>
    /// <returns><see langword="true"/> if collection contains key named 
    /// <see cref="ElementEntry.UndefinedKey"/>, otherwise <see langword="false"/>.</returns>
    public bool TryGetUndefinedErrorEntry([NotNullWhen(true)] out ElementEntry elementEntry)
    {
        elementEntry = _items.Find(i => i.Key.Equals(ElementEntry.UndefinedKey, StringComparison.OrdinalIgnoreCase));
        return elementEntry is { Key: { } };
    }

    internal void Add(ElementEntry element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (_items.Find(i => i.Key.Equals(element.Key, StringComparison.OrdinalIgnoreCase)) is { Key: { } } existsItem)
        {
            _items.Remove(existsItem);
            element = existsItem = existsItem with { Values = existsItem.Values.Union(element.Values).ToArray() };
        }

        _items.Add(element);
    }

    internal void Add(string key, params string[] values) => Add(new ElementEntry(key, values));

    internal void Merge(ElementCollection elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        foreach (var item in elements)
        {
            _items.Add(item);
        }
    }

    internal readonly void Clear() => _items?.Clear();

    ///<inheritdoc/>
    public IEnumerator<ElementEntry> GetEnumerator() => _items.GetEnumerator();

    ///<inheritdoc/>
    public bool TryGet(string key, out ElementEntry? result)
    {
        ArgumentNullException.ThrowIfNull(key);

        result = _items.Find(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        return result.HasValue;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}