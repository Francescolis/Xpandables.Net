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
/// Represents an asynchronous paged enumerable collection.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <remarks>You can managed the way the class get serialized by registering the JSON converter
/// <see langword="MaterializedPagedDataJsonConverterFactory"/> from the "Xpandables.Net.AspNetCore" package.</remarks>
public sealed class AsyncPagedEnumerable<T>(
    IAsyncEnumerable<T> source,
    Func<Task<Pagination>> paginationFactory) : IAsyncPagedEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly Lazy<Task<Pagination>> _lazyPagination = new(() => paginationFactory());
    private volatile Pagination? _enumerationBasedPagination;

    /// <inheritdoc />
    public Pagination Pagination
    {
        get
        {
            if (_enumerationBasedPagination is not null)
            {
                return _enumerationBasedPagination;
            }
            if (!_lazyPagination.IsValueCreated)
            {
                throw new InvalidOperationException(
                    $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} for async access.");
            }

            return _lazyPagination.Value.IsCompletedSuccessfully
                ? _lazyPagination.Value.Result
                : throw new InvalidOperationException(
                    $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} for async access.");
        }
    }

    /// <inheritdoc />
    public async Task<Pagination> GetPaginationAsync()
    {
        if (_enumerationBasedPagination is not null)
        {
            return _enumerationBasedPagination;
        }

        return await _lazyPagination.Value.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // Check if we should use counting enumerator
        if (ShouldUseCountingEnumerator())
        {
            return new AsyncPagedEnumerator<T>(
                _source.GetAsyncEnumerator(cancellationToken),
                this,
                cancellationToken);
        }


        return _source.GetAsyncEnumerator(cancellationToken);
    }

    /// <summary>
    /// Determines if we should use the counting enumerator based on pagination state.
    /// </summary>
    private bool ShouldUseCountingEnumerator()
    {
        // Use counting enumerator if:
        // 1. We don't have enumeration-based pagination yet, AND
        // 2. The lazy pagination has a TotalCount of 0 (indicating unknown count)
        if (_enumerationBasedPagination is not null)
        {
            return false;
        }

        if (_lazyPagination.Value.IsCompletedSuccessfully)
        {
            var pagination = _lazyPagination.Value.Result;
            return pagination.TotalCount == 0;
        }

        return true;
    }

    /// <summary>
    /// Internal method to update pagination info after enumeration completes.
    /// Called by AsyncPagedEnumerator when enumeration finishes.
    /// </summary>
    internal void UpdatePaginationAfterEnumeration(Pagination newPagination) =>
        _enumerationBasedPagination = newPagination;
}