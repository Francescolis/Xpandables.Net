
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
using System.Collections.Concurrent;

namespace Xpandables.Net.Primitives.Cache;

/// <summary>
/// Represents a thread-safe cache that stores values using weak references and automatically removes expired or
/// garbage-collected items based on configurable cleanup intervals and maximum item age.
/// </summary>
/// <remarks>The cache is designed to minimize memory usage by allowing items to be garbage collected when no
/// longer referenced elsewhere. Items are periodically cleaned up according to the specified cleanup interval and
/// maximum age. The cache is safe for concurrent access from multiple threads. After disposal, the cache should not be
/// used.</remarks>
/// <typeparam name="TKey">The type of keys used to identify cached items. Keys must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache. Values must be reference types.</typeparam>
public sealed class MemoryAwareCache<TKey, TValue> : IDisposable
    where TKey : notnull
    where TValue : class
{
    private readonly ConcurrentDictionary<TKey, WeakCacheEntry<TValue>> _cache = new();
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _maxAge;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryAwareCache{TKey, TValue}"/> class with the specified cleanup interval and
    /// maximum item age.
    /// </summary>
    /// <remarks>The cache uses a timer to periodically clean up expired items based on the specified
    /// <paramref name="cleanupInterval"/>.  Items older than the specified <paramref name="maxAge"/> will be removed
    /// during cleanup.</remarks>
    /// <param name="cleanupInterval">The interval at which the cache performs cleanup operations to remove expired items.  If not specified, the
    /// default value is 5 minutes.</param>
    /// <param name="maxAge">The maximum age an item can remain in the cache before it is considered expired and eligible for removal.  If
    /// not specified, the default value is 1 hour.</param>
    public MemoryAwareCache(TimeSpan cleanupInterval = default, TimeSpan maxAge = default)
    {
        _cleanupInterval = cleanupInterval == default ? TimeSpan.FromMinutes(5) : cleanupInterval;
        _maxAge = maxAge == default ? TimeSpan.FromHours(1) : maxAge;

        _cleanupTimer = new Timer(Cleanup, null, _cleanupInterval, _cleanupInterval);
    }

    /// <summary>
    /// Retrieves the value associated with the specified key from the cache, or adds a new value if the key does not
    /// exist.
    /// </summary>
    /// <remarks>If the key already exists in the cache, the existing value is returned. Otherwise, the
    /// <paramref name="factory"/>  function is invoked to create a new value, which is then added to the cache and
    /// returned.</remarks>
    /// <param name="key">The key of the value to retrieve or add.</param>
    /// <param name="factory">A function that generates a new value to add to the cache if the key does not exist. The function takes the key
    /// as a parameter and returns the value to be added.</param>
    /// <returns>The value associated with the specified key. If the cache has been disposed, returns <see langword="null"/>.</returns>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.TryGetValue(key, out var entry)
            && entry.TryGetValue(out var existingValue))
        {
            return existingValue!;
        }

        var newValue = factory(key);
        _cache.TryAdd(key, new WeakCacheEntry<TValue>(newValue));

        return newValue;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key,  if the key is found and the
    /// cache is not disposed; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the key is found and the cache is not disposed;  otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        value = null;
        return !_disposed
            && _cache.TryGetValue(key, out var entry)
            && entry.TryGetValue(out value);
    }

    private void Cleanup(object? state)
    {
        if (_disposed) return;

        var keysToRemove = new List<TKey>();

        foreach (var kvp in _cache)
        {
            var entry = kvp.Value;

            // Remove if expired or garbage collected
            if (entry.IsExpired(_maxAge) || !entry.TryGetValue(out _))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method should be called when the instance is no longer needed to free up resources. 
    /// After calling this method, the instance is considered disposed and should not be used further.</remarks>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cleanupTimer?.Dispose();
        _cache.Clear();
    }
}