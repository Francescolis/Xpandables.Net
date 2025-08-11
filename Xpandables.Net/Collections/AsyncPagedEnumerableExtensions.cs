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
/// Represents an <see cref="IAsyncPagedEnumerable{T}"/> materialized data for JSON serialization.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <param name="Data">The materialized data items.</param>
/// <param name="Pagination">The pagination metadata.</param>
public readonly record struct AsyncPagedEnumerableData<T>(IReadOnlyList<T> Data, Pagination Pagination);

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

        return source.WithPagination(Pagination.Without());
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
    /// Converts an <see cref="IQueryable{TSource}"/> to an <see cref="IAsyncPagedEnumerable{TSource}"/>
    /// with default pagination based on the query's Skip and Take values.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>An async paged enumerable with default pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IQueryable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var paginationSource = source.ExtractPagination();
        var asyncEnumerable = source as IAsyncEnumerable<TSource> ?? source.ToAsyncEnumerable();
        return asyncEnumerable.WithPagination(() =>
        {
            long totalCount = paginationSource.QueryWithoutPagination.LongCount();
            return Task.FromResult(Pagination.With(paginationSource.Skip, paginationSource.Take, totalCount));
        });
    }

    /// <summary>
    /// Converts an <see cref="IQueryable{TSource}"/> to an <see cref="IAsyncPagedEnumerable{TSource}"/>
    /// with pagination based on the provided <see cref="Pagination"/> object.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="pagination">The pagination information to apply.</param>
    /// <returns>An async paged enumerable with pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IQueryable<TSource> source, Pagination pagination)
    {
        ArgumentNullException.ThrowIfNull(source);

        var asyncEnumerable = source as IAsyncEnumerable<TSource> ?? source.ToAsyncEnumerable();

        return asyncEnumerable.WithPagination(pagination);
    }

    /// <summary>
    /// Extracts Skip and Take values from a query and returns a version without pagination.
    /// </summary>
    /// <typeparam name="TSource">The type of the query.</typeparam>
    /// <param name="source">The query to analyze.</param>
    /// <returns>Pagination extraction result containing Skip/Take values and unpaginated query.</returns>
    public static PaginationSource<TSource> ExtractPagination<TSource>(this IQueryable<TSource> source)
    {
        var visitor = new PaginationSourceExtractor.PaginationExtractionVisitor();

        var modifiedExpression = visitor.Visit(source.Expression);

        var queryWithoutPagination = source.Provider.CreateQuery<TSource>(modifiedExpression);

        return Pagination.WithSource(visitor.Skip, visitor.Take, queryWithoutPagination);
    }

    /// <summary>
    /// Converts an <see cref="IAsyncPagedEnumerable{TSource}"/> to an <see cref="AsyncPagedEnumerableData{TSource}"/>
    /// containing the materialized data and pagination metadata for JSON serialization.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async paged enumerable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="AsyncPagedEnumerableData{TSource}"/> containing the materialized data and pagination metadata.</returns>
    public static async Task<AsyncPagedEnumerableData<TSource>> ToAsyncPagedEnumerableDataAsync<TSource>(
        this IAsyncPagedEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var data = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
        var pagination = await source.GetPaginationAsync().ConfigureAwait(false);

        return new AsyncPagedEnumerableData<TSource>(data, pagination);
    }
}
