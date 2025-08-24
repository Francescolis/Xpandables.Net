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
namespace Xpandables.Net.Collections;

/// <summary>
/// Provides an asynchronous enumerator for iterating over a collection of items of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>This enumerator supports asynchronous iteration over a read-only collection of items. It maintains
/// the current position within the collection and provides access to the current item via the <see cref="Current"/>
/// property.</remarks>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
/// <param name="items"></param>
public sealed class AsyncPagedEnumerator<T>(IReadOnlyList<T> items) : IAsyncEnumerator<T>
{
    private readonly IReadOnlyList<T> _items = items ?? throw new ArgumentNullException(nameof(items));
    private int _index = -1;

    /// <summary>
    /// Gets the current element in the collection or sequence.
    /// </summary>
    public T Current { get; private set; } = default!;

    /// <summary>
    /// Advances the enumerator to the next element in the collection asynchronously.
    /// </summary>
    /// <remarks>After the enumerator advances past the last element in the collection, the <see
    /// cref="Current"/> property is set to its default value.</remarks>
    /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to <see langword="true"/> if the enumerator successfully
    /// advanced to the next element; otherwise, <see langword="false"/> if the end of the collection has been
    /// reached.</returns>
    public ValueTask<bool> MoveNextAsync()
    {
        var next = _index + 1;
        if ((uint)next < (uint)_items.Count)
        {
            _index = next;
            Current = _items[next];
            return ValueTask.FromResult(true);
        }

        Current = default!;
        return ValueTask.FromResult(false);
    }

    /// <summary>
    /// Asynchronously releases the unmanaged resources used by the object and performs other cleanup operations.
    /// </summary>
    /// <remarks>This method should be called when the object is no longer needed to ensure proper resource
    /// cleanup.  It is recommended to use this method within a `await using` statement or explicitly call it when 
    /// asynchronous disposal is required.</remarks>
    /// <returns></returns>
    public ValueTask DisposeAsync() => default;
}