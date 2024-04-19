
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
namespace Xpandables.Net.Primitives.Collections;

/// <summary>
/// Provides a set of <see langword="static"/> methods for 
/// <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
public static class AsyncEnumerable
{
    /// <summary>
    /// Returns an empty async-enumerable.
    /// </summary>
    /// <returns>An async-enumerable sequence with no elements.</returns>
    public static IAsyncEnumerable<T> EmptyAsync<T>()
        => new AsyncEnumerable<T>([]);
}

/// <summary>
/// Represents a helper class that allows a generic collection to be 
/// asynchronously enumerated.
/// This class implements <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
public sealed class AsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly
        Func<CancellationToken, IAsyncEnumerator<T>> _asyncEnumerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> 
    /// class with the collection to be asynchronously enumerated.
    /// </summary>
    /// <param name="collection">The collection to act on.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="collection"/> is null.</exception>
    public AsyncEnumerable(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        _asyncEnumerator = _
            => new AsyncEnumerator<T>(collection.GetEnumerator());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEnumerable{T}"/> 
    /// class with the delegate enumerator.
    /// </summary>
    /// <param name="asyncEnumerator">The delegate that provides 
    /// the async enumerator.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="asyncEnumerator"/> is null.</exception>
    public AsyncEnumerable(
        Func<CancellationToken, IAsyncEnumerator<T>> asyncEnumerator)
        => _asyncEnumerator = asyncEnumerator
            ?? throw new ArgumentNullException(nameof(asyncEnumerator));

    /// <summary>
    /// Returns an enumerator that iterates asynchronously through 
    /// the collection.
    /// </summary>
    /// <param name="cancellationToken">A System.Threading.CancellationToken 
    /// that may be used to cancel the asynchronous iteration.</param>
    /// <returns>An enumerator that can be used to iterate asynchronously 
    /// through the collection.</returns>
    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => _asyncEnumerator(cancellationToken);
}
