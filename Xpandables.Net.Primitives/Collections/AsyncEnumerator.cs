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

namespace Xpandables.Net.Primitives.Collections;

/// <summary>
/// Represents a helper class that adds asynchronous iteration support to a generic collection.
/// This class implements <see cref="IAsyncEnumerator{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AsyncEnumerator{T}"/> class with the specified enumerator.
/// </remarks>
/// <param name="inner">The enumerator to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="inner"/> is null.</exception>
public sealed class AsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    public T Current => _inner.Current;

    /// <summary>
    ///  Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    /// <summary>
    ///  Advances the enumerator asynchronously to the next element of the collection.
    /// </summary>
    /// <returns> A <see cref="ValueTask{TResult}"/>  that will complete with
    /// a result of <see langword="true"/> if the enumerator was successfully
    /// advanced to the next element, or <see langword="false"/>
    /// if the enumerator has passed the end of the collection.</returns>
    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
}

