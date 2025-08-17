
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
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Collections;

/// <summary>
/// Provides an asynchronous enumerator that supports paginated enumeration of items and updates pagination metadata
/// upon completion of enumeration.
/// </summary>
/// <remarks>This enumerator is designed to work with <see cref="AsyncPagedEnumerable{T}"/> to facilitate
/// paginated data retrieval. It tracks the number of items enumerated and updates the parent pagination metadata when
/// enumeration is completed. The enumerator ensures proper disposal and finalization of resources, even in the presence
/// of exceptions or early disposal.</remarks>
/// <typeparam name="T">The type of elements being enumerated.</typeparam>
/// <param name="sourceEnumerator"></param>
/// <param name="parent"></param>
/// <param name="cancellationToken"></param>
public sealed class AsyncPagedEnumerator<T>(
    IAsyncEnumerator<T> sourceEnumerator,
    AsyncPagedEnumerable<T> parent,
    CancellationToken cancellationToken) : IAsyncEnumerator<T>
{
    private readonly IAsyncEnumerator<T> _sourceEnumerator = sourceEnumerator ?? throw new ArgumentNullException(nameof(sourceEnumerator));
    private readonly AsyncPagedEnumerable<T> _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    private readonly CancellationToken _cancellationToken = cancellationToken;

    private long _itemCount;
    private bool _enumerationCompleted;
    private bool _disposed;

    /// <inheritdoc />
    public T Current => _sourceEnumerator.Current;

    /// <inheritdoc />
    public async ValueTask<bool> MoveNextAsync()
    {
        if (_disposed)
        {
            return false;
        }

        try
        {
            var hasNext = await _sourceEnumerator
                .MoveNextAsync(_cancellationToken)
                .ConfigureAwait(false);

            if (hasNext)
            {
                _itemCount++;
                return true;
            }
            else
            {
                // Enumeration completed - update parent's pagination
                await FinalizeEnumerationAsync().ConfigureAwait(false);
                return false;
            }
        }
        catch
        {
            // Even on exception, try to finalize if we haven't already
            if (!_enumerationCompleted)
            {
                await FinalizeEnumerationAsync().ConfigureAwait(false);
            }
            throw;
        }
    }

    /// <summary>
    /// Finalizes the enumeration by updating the parent's pagination with the actual count.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask FinalizeEnumerationAsync()
    {
        if (_enumerationCompleted)
        {
            return;
        }

        _enumerationCompleted = true;

        try
        {
            // Get the original pagination (if available) to preserve skip/take info
            var originalPagination = await _parent.GetPaginationAsync().ConfigureAwait(false);

            // Create new pagination with the actual counted total
            var updatedPagination = originalPagination.TotalCount == 0
                ? CreatePaginationWithCount(originalPagination, _itemCount)
                : originalPagination;

            _parent.UpdatePaginationAfterEnumeration(updatedPagination);
        }
        catch
        {
            var fallbackPagination = Pagination.Without(_itemCount);
            _parent.UpdatePaginationAfterEnumeration(fallbackPagination);
        }
    }

    /// <summary>
    /// Creates pagination info with the counted total while preserving skip/take values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Pagination CreatePaginationWithCount(Pagination original, long actualCount) =>
        original.Skip.HasValue || original.Take.HasValue
            ? Pagination.With(original.Skip, original.Take, actualCount)
            : Pagination.Without(actualCount);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_enumerationCompleted)
        {
            await FinalizeEnumerationAsync().ConfigureAwait(false);
        }

        await _sourceEnumerator.DisposeAsync().ConfigureAwait(false);
    }
}