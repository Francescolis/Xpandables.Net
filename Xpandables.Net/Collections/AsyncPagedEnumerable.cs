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
public sealed class AsyncPagedEnumerable<T>(
    IAsyncEnumerable<T> source,
    Func<Task<Pagination>> paginationFactory) : IAsyncPagedEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly Lazy<Task<Pagination>> _lazyPagination = new(() => paginationFactory());

    /// <inheritdoc />
    public Pagination PaginationInfo =>
        _lazyPagination.Value.IsCompletedSuccessfully
            ? _lazyPagination.Value.Result
            : throw new InvalidOperationException(
                $"Pagination info is not yet available. Use {nameof(GetPaginationAsync)} for async access.");

    /// <inheritdoc />
    public Task<Pagination> GetPaginationAsync() => _lazyPagination.Value;

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        _source.GetAsyncEnumerator(cancellationToken);
}