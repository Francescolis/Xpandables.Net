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
/// Represents materialized paged data for JSON serialization.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <param name="Data">The materialized data items.</param>
/// <param name="Pagination">The pagination metadata.</param>
public readonly record struct MaterializedPagedData<T>(IReadOnlyList<T> Data, Pagination Pagination);

/// <summary>
/// Provides extension methods for working with <see cref="IAsyncPagedEnumerable{T}"/>.
/// </summary>
public static class AsyncPagedEnumerableExtensions
{
    /// <summary>
    /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with default pagination.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <returns>An async paged enumerable with default pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IAsyncEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AsyncPagedEnumerable<TSource>(
            source,
            () => Task.FromResult(Pagination.WithoutPagination()));
    }

    /// <summary>
    /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with pagination.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="paginationFactory">A factory function that provides pagination information.</param>
    /// <returns>An async paged enumerable with pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<Task<Pagination>> paginationFactory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(paginationFactory);

        return new AsyncPagedEnumerable<TSource>(source, paginationFactory);
    }

    /// <summary>
    /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/>
    /// with immediate pagination info.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="pagination">The pagination information.</param>
    /// <returns>An async paged enumerable with pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IAsyncEnumerable<TSource> source,
        Pagination pagination)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AsyncPagedEnumerable<TSource>(source, () => Task.FromResult(pagination));
    }

    /// <summary>
    /// Converts an <see cref="IAsyncPagedEnumerable{TSource}"/> to an <see cref="IAsyncPagedEnumerable{TResult}"/>
    /// with pagination by applying a transformation function.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the source collection.</typeparam>
    /// <typeparam name="TResult">The type of items in the result collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    /// <param name="transformFactory">The transformation function that takes the source and returns a new async paged enumerable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An async paged enumerable with transformed items and pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TResult> WithPagination<TSource, TResult>(
        this IAsyncPagedEnumerable<TSource> source,
        Func<IAsyncPagedEnumerable<TSource>, CancellationToken, IAsyncEnumerable<TResult>> transformFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(transformFactory);

        return new AsyncPagedEnumerable<TResult>(
            transformFactory(source, cancellationToken),
            source.GetPaginationAsync);
    }

    /// <summary>
    /// Materializes the async paged enumerable to a list with pagination.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the materialized list and pagination info.</returns>
    public static async Task<MaterializedPagedData<TSource>> ToListWithPaginationAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        var pagination = await source.GetPaginationAsync().ConfigureAwait(false);

        return new MaterializedPagedData<TSource>(items, pagination);
    }
}
