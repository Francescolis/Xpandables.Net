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
namespace Xpandables.Net;

/// <summary>
/// Represents a cache entry that holds a weak reference to a target object of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>This structure is designed to manage cached objects without preventing their garbage collection.  It
/// provides functionality to check the availability of the target object and determine whether the cache entry has
/// expired.</remarks>
/// <typeparam name="T">The type of the target object. Must be a reference type.</typeparam>
/// <param name="value"></param>
public readonly struct WeakCacheEntry<T>(T value) : IEquatable<T>, IEquatable<WeakCacheEntry<T>>
    where T : class
{
    private readonly WeakReference<T> _weakReference = new(value);
    private readonly DateTime _lastAccessTime = DateTime.UtcNow;

    /// <summary>
    /// Attempts to retrieve the target object referenced by the current instance.
    /// </summary>
    /// <param name="value">When this method returns, contains the target object if it is still available; otherwise, <see
    /// langword="null"/>.</param>
    /// <returns><see langword="true"/> if the target object is available and successfully retrieved; otherwise, <see
    /// langword="false"/>.</returns>
    public readonly bool TryGetValue(out T? value) => _weakReference.TryGetTarget(out value);

    /// <summary>
    /// Determines whether the current instance is considered expired based on the specified maximum age.
    /// </summary>
    /// <param name="maxAge">The maximum allowable age as a <see cref="TimeSpan"/>. 
    /// This value represents the duration after which the
    /// instance is considered expired.</param>
    /// <returns><see langword="true"/> if the instance is expired (i.e., the time since the last access exceeds <paramref
    /// name="maxAge"/>); otherwise, <see langword="false"/>.</returns>
    public readonly bool IsExpired(TimeSpan maxAge) => DateTime.UtcNow - _lastAccessTime > maxAge;

    /// <summary>
    /// Gets the date and time when the resource was last accessed.
    /// </summary>
    public readonly DateTime LastAccessTime => _lastAccessTime;

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance. This can be another <see cref="WeakCacheEntry{T}"/> or an
    /// instance of type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see
    /// langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is WeakCacheEntry<T> other)
        {
            return Equals(other);
        }

        if (obj is T target)
        {
            return Equals(target);
        }

        return false;
    }

    /// <summary>
    /// Returns a hash code for the object referenced by the weak reference, or 0 if the target is no longer available.
    /// </summary>
    /// <remarks>This method attempts to retrieve the target of the weak reference and returns its hash code. 
    /// If the target is no longer available (e.g., it has been garbage collected), the method returns 0.</remarks>
    /// <returns>The hash code of the target object if it is still accessible; otherwise, 0.</returns>
    public override int GetHashCode()
    {
        if (_weakReference.TryGetTarget(out T? target))
        {
            return target.GetHashCode();
        }

        return 0;
    }

    /// <summary>
    /// Determines whether the current object is equal to the specified object.
    /// </summary>
    /// <param name="other">The object to compare with the current object. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified object is not <see langword="null"/> and is the same instance as the
    /// target of the weak reference;  otherwise, <see langword="false"/>.</returns>
    public bool Equals(T? other) =>
        other is not null
        && _weakReference.TryGetTarget(out T? target)
        && ReferenceEquals(target, other);

    /// <summary>
    /// Determines whether the current object is equal to another <see cref="WeakCacheEntry{T}"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="WeakCacheEntry{T}"/> instance to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the target objects referenced by both instances are the same; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Equals(WeakCacheEntry<T> other) =>
        _weakReference.TryGetTarget(out T? target)
        && other._weakReference.TryGetTarget(out T? otherTarget)
        && ReferenceEquals(target, otherTarget);

    /// <summary>
    /// Determines whether two <see cref="WeakCacheEntry{T}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="WeakCacheEntry{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="WeakCacheEntry{T}"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="WeakCacheEntry{T}"/> instances are equal;  otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator ==(WeakCacheEntry<T> left, WeakCacheEntry<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="WeakCacheEntry{T}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="WeakCacheEntry{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="WeakCacheEntry{T}"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="WeakCacheEntry{T}"/> instances are not equal;  otherwise, <see
    /// langword="false"/>.</returns>
    public static bool operator !=(WeakCacheEntry<T> left, WeakCacheEntry<T> right) => !left.Equals(right);
}
