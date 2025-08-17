
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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.Tasks;

/// <summary>
/// Represents a context for a request, encapsulating the request itself and providing a collection for storing 
/// items related to the request.
/// </summary>
/// <typeparam name="TRequest">The type of the request, which must implement <see cref="IRequest"/>.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestContext{TRequest}"/> class with the specified request.
/// </remarks>
/// <param name="request">The request to associate with this context.</param>
/// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
public sealed class RequestContext<TRequest>(TRequest request) : IEnumerable<KeyValuePair<string, object>>
    where TRequest : class, IRequest
{
    private readonly ConcurrentDictionary<string, object> Items = new();

    /// <summary>
    /// The request associated with this context.
    /// </summary>
    public TRequest Request { get; } = request
        ?? throw new ArgumentNullException(nameof(request), "Request cannot be null.");

    /// <summary>
    /// Gets or sets the value associated with the specified key in the collection.
    /// </summary>
    /// <param name="key">The key of the item to get or set. Cannot be <see langword="null"/>.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key/value is <see langword="null"/>.</exception>
    public object? this[string key]
    {
        get => Items.TryGetValue(key, out var value) ? value : null;
        set => Items[key] = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
    }

    /// <summary>
    /// Adds an item to the request context with the specified key and value.
    /// </summary>
    /// <param name="key">The key for the item.</param>
    /// <param name="value">The value to associate with the key.</param>
    public void AddItem(string key, object value) => Items[key] = value;

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key from the collection.
    /// </summary>
    /// <remarks>This method does not throw an exception if the key is not found. Use this method to safely
    /// attempt retrieval without requiring prior validation of the key's existence.</remarks>
    /// <param name="key">The key of the item to retrieve. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key if the key is found; otherwise,
    /// <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the collection contains an item with the specified key; otherwise, <see
    /// langword="false"/>.</returns>
    public bool TryGetItem(string key, [NotNullWhen(true)] out object? value) =>
        Items.TryGetValue(key, out value);

    /// <summary>
    /// Attempts to retrieve an item of the specified type from the collection using the provided key.
    /// </summary>
    /// <remarks>This method does not throw an exception if the key is not found or if the item is not of the
    /// expected type. Instead, it returns <see langword="false"/> and sets <paramref name="item"/> to its default
    /// value.</remarks>
    /// <typeparam name="TItem">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the item to retrieve. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="item">When this method returns, contains the item of type <typeparamref name="TItem"/> associated with the specified
    /// key, if the key exists and the item is of the correct type; otherwise, the default value for <typeparamref
    /// name="TItem"/>.</param>
    /// <returns><see langword="true"/> if an item with the specified key exists in the collection and is of type <typeparamref
    /// name="TItem"/>; otherwise, <see langword="false"/>.</returns>
    public bool TryGetItem<TItem>(string key, [NotNullWhen(true)] out TItem? item)
    {
        if (Items.TryGetValue(key, out var value) && value is TItem castedItem)
        {
            item = castedItem;
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Retrieves an item of the specified type associated with the given key.
    /// </summary>
    /// <typeparam name="TItem">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the item to retrieve. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>The item of type <typeparamref name="TItem"/> associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no item is found with the specified key or if the item cannot be cast to the specified type
    /// <typeparamref name="TItem"/>.</exception>
    public TItem GetItem<TItem>(string key)
    {
        if (TryGetItem<TItem>(key, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"No item found with key '{key}'.");
    }

    /// <summary>
    /// Attempts to remove the item with the specified key from the request context.
    /// </summary>
    /// <param name="key">The key of the item to remove. Cannot be <see langword="null"/> or empty.</param>
    /// <returns><see langword="true"/> if the item was successfully removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveItem(string key) => Items.TryRemove(key, out _);

    /// <summary>
    /// Removes all items from the request context.
    /// </summary>
    public void ClearItems() => Items.Clear();

    /// <summary>
    /// Returns an enumerator that iterates through the collection of key-value pairs.
    /// </summary>
    /// <remarks>Use this method to enumerate the items in the collection using a `foreach` loop or manual
    /// iteration.</remarks>
    /// <returns>An enumerator for the collection of key-value pairs, where each pair consists of a string key and an associated
    /// object value.</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
