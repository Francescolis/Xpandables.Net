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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Collections;

/// <summary>
/// Provides extension methods for working with <see cref="IAsyncPagedEnumerable{T}"/>.
/// </summary>
public static partial class AsyncPagedEnumerableExtensions
{
    /// <summary>
    /// Determines whether the specified type implements the <see cref="IAsyncPagedEnumerable{T}"/> interface.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsyncPagedEnumerable(Type type) =>
        type.GetInterfaces()
            .Any(i => i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>));

    /// <summary>
    /// Determines whether the specified asynchronous enumerable is an instance of <see cref="IAsyncPagedEnumerable{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsyncPagedEnumerable<T>(this IAsyncEnumerable<T> source) =>
        source is IAsyncPagedEnumerable<T>;

    /// <summary>
    /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> to enable pagination
    /// metadata, such as page size and total count.
    /// </summary>
    /// <remarks>If <paramref name="source"/> is already an <see cref="IAsyncPagedEnumerable{T}"/>, it is
    /// returned as-is. When <paramref name="alwaysCount"/> is <see langword="true"/>, the method throws a <see
    /// cref="NotSupportedException"/> if the source is a plain <see cref="IAsyncEnumerable{T}"/>, as calculating the
    /// total count generically requires buffering or double enumeration, which may lead to unexpected memory pressure.
    /// In such cases, consider using an <see cref="IQueryable{T}"/> source instead.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The asynchronous sequence to convert. Cannot be <see langword="null"/>.</param>
    /// <param name="alwaysCount">A value indicating whether the total count of items in the sequence should always be calculated. If <see
    /// langword="true"/>, the method attempts to calculate the total count, which may require buffering or double
    /// enumeration. Defaults to <see langword="false"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> that wraps the source sequence and provides pagination metadata.</returns>
    /// <exception cref="NotSupportedException">Thrown if <paramref name="alwaysCount"/> is <see langword="true"/> and <paramref name="source"/> is a plain <see
    /// cref="IAsyncEnumerable{T}"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncPagedEnumerable<TSource> AsAsyncPagedEnumerable<TSource>(
        this IAsyncEnumerable<TSource> source, bool alwaysCount = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IAsyncPagedEnumerable<TSource> paged) return paged;

        if (alwaysCount)
        {
            // WARNING: Counting a plain IAsyncEnumerable<T> generically requires buffering or double enumeration.
            // To prevent unexpected memory pressure, we fail fast. Prefer IQueryable<T> or set alwaysCount=false.
            throw new NotSupportedException("alwaysCount=true is not supported for plain IAsyncEnumerable<T>. " +
                "Use IQueryable<T> or leave alwaysCount=false to stream with default metadata.");
        }

        return new AsyncPagedEnumerable<TSource>(
            source,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            buffer: null);
    }

    /// <summary>
    /// Enables asynchronous pagination over an <see cref="IAsyncQueryable{T}"/> source.
    /// Treated as a streaming async sequence without total count computation.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">Async queryable source. Cannot be null.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> with minimal pagination metadata.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncPagedEnumerable<TSource> AsAsyncPagedEnumerable<TSource>(
        this IAsyncQueryable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // IAsyncQueryable<T> should be asynchronously enumerable
        if (source is not IAsyncEnumerable<TSource> asyncEnumerable)
        {
            throw new NotSupportedException(
                "The provided IAsyncQueryable<T> does not implement IAsyncEnumerable<T>. " +
                "Convert to an async sequence first or use IQueryable<T> overloads.");
        }

        // No counting here to avoid buffering/double enumeration.
        return new AsyncPagedEnumerable<TSource>(
            asyncEnumerable,
            static _ => new ValueTask<Pagination>(Pagination.Without()),
            buffer: null);
    }

    /// <summary>
    /// Enables asynchronous pagination over an <see cref="IAsyncQueryable{T}"/> source.
    /// When alwaysCount is true, throws to avoid generic double enumeration on unknown providers.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">Async queryable source. Cannot be null.</param>
    /// <param name="alwaysCount">
    /// If true, attempts to compute total count. Not supported for IAsyncQueryable to avoid buffering/double enumeration.
    /// </param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> with pagination metadata.</returns>
    public static IAsyncPagedEnumerable<TSource> AsAsyncPagedEnumerable<TSource>(
        this IAsyncQueryable<TSource> source, bool alwaysCount)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (alwaysCount)
        {
            // Symmetric to IAsyncEnumerable<T> behavior: avoid generic counting that may cause buffering.
            throw new NotSupportedException(
                "alwaysCount=true is not supported for IAsyncQueryable<T>. " +
                "Use IQueryable<T> overloads to enable provider-side counting.");
        }

        return AsAsyncPagedEnumerable(source);
    }

    /// <summary>
    /// Enables asynchronous pagination over the elements of the specified queryable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the queryable data source.</typeparam>
    /// <param name="source">The queryable data source to paginate. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{TSource}"/> that allows asynchronous enumeration of the paginated data.</returns>
    public static IAsyncPagedEnumerable<TSource> AsAsyncPagedEnumerable<TSource>(
        this IQueryable<TSource> source) => AsAsyncPagedEnumerable(source, alwaysCount: true);

    /// <summary>
    /// Enables pagination for an <see cref="IQueryable{T}"/> source, optionally including a total count of items.
    /// </summary>
    /// <remarks>This method supports both paginated and unpaginated queries. If the query does not include
    /// pagination (i.e., no skip or take), the behavior depends on the value of <paramref name="alwaysCount"/>: <list
    /// type="bullet"> <item> <description>If <paramref name="alwaysCount"/> is <see langword="false"/>, the enumerable
    /// streams the query without buffering or counting.</description> </item> <item> <description>If <paramref
    /// name="alwaysCount"/> is <see langword="true"/>, the total count is calculated before streaming the
    /// query.</description> </item> </list> When pagination is present, the method projects the query to include the
    /// total count in a single query, if supported by the underlying provider.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source query.</typeparam>
    /// <param name="source">The queryable source to apply pagination to. Cannot be <see langword="null"/>.</param>
    /// <param name="alwaysCount">A value indicating whether to always calculate the total count of items in the source. If <see
    /// langword="true"/>, the total count is included in the pagination metadata even if no pagination is applied. If
    /// <see langword="false"/>, the total count is omitted unless pagination is explicitly present in the query.</param>
    /// <returns>An <see cref="IAsyncPagedEnumerable{T}"/> that represents the paginated query. The enumerable includes
    /// pagination metadata such as the total count of items and the applied skip/take values, if applicable.</returns>
    public static IAsyncPagedEnumerable<TSource> AsAsyncPagedEnumerable<TSource>(
        this IQueryable<TSource> source, bool alwaysCount)
    {
        ArgumentNullException.ThrowIfNull(source);

        var extraction = ExtractPagination(source);
        var skip = extraction.Skip;
        var take = extraction.Take;
        var unpaginated = extraction.QueryWithoutPagination;

        var asyncEnumerable = source as IAsyncEnumerable<TSource> ?? source.ToAsyncEnumerable();

        if (!skip.HasValue && !take.HasValue)
        {
            if (!alwaysCount)
            {
                return new AsyncPagedEnumerable<TSource>(
                    asyncEnumerable,
                    static _ => new ValueTask<Pagination>(Pagination.Without()),
                    buffer: null);
            }
            else
            {
                return new AsyncPagedEnumerable<TSource>(
                    asyncEnumerable,
                    async ct =>
                    {
                        var total = await ExecuteCountAsync(unpaginated, ct).ConfigureAwait(false);
                        return Pagination.Without(total);
                    },
                    buffer: null);
            }
        }

        var projected = ProjectWithTotal(source, unpaginated);

        IAsyncEnumerable<PrimedResult<TSource>> projectedAsync =
            projected as IAsyncEnumerable<PrimedResult<TSource>>
            ?? projected.ToAsyncEnumerable();

        return new AsyncPagedPrimedEnumerable<TSource>(
            projectedAsync,
            skip,
            take,
            fallbackTotalFactory: ct => ExecuteCountAsync(unpaginated, ct));
    }

    /// <summary>
    /// Extracts Skip/Take for an <see cref="IAsyncQueryable{T}"/> if available.
    /// Note: the returned pagination source cannot expose a provider-specific query shape here,
    /// so consumers should rely on IQueryable overloads when they need the query without pagination.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">Async queryable source. Cannot be null.</param>
    /// <returns>A <see cref="PaginationSource{TSource}"/> built from extracted skip/take, with an empty query body.</returns>
    public static PaginationSource<TSource> ExtractPagination<TSource>(
        this IAsyncQueryable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Analyze the expression to extract Skip/Take
        var visitor = new PaginationSourceExtractor.PaginationExtractionVisitor();
        _ = visitor.Visit(source.Expression);

        // We cannot reliably create an IQueryProvider-based query without pagination from IAsyncQueryable<T>
        // without provider-specific APIs. Expose skip/take; provide a no-op IQueryable to satisfy the type.
        IQueryable<TSource> placeholder = Array.Empty<TSource>().AsQueryable();
        return Pagination.WithSource(visitor.Skip, visitor.Take, placeholder);
    }

    /// <summary>
    /// Extracts Skip and Take values from a query and returns a version without pagination.
    /// </summary>
    public static PaginationSource<TSource> ExtractPagination<TSource>(this IQueryable<TSource> source)
    {
        var visitor = new PaginationSourceExtractor.PaginationExtractionVisitor();

        var modifiedExpression = visitor.Visit(source.Expression);

        var queryWithoutPagination = source.Provider.CreateQuery<TSource>(modifiedExpression);

        return Pagination.WithSource(visitor.Skip, visitor.Take, queryWithoutPagination);
    }

    private static IQueryable<PrimedResult<TSource>> ProjectWithTotal<TSource>(
        IQueryable<TSource> pageQuery,
        IQueryable<TSource> unpaginatedQuery)
    {
        var p = Expression.Parameter(typeof(TSource), "item");

        var longCountCall = Expression.Call(
            QueryableLongCount<TSource>.Method,
            unpaginatedQuery.Expression);

        var tupleCtor = PrimedFactory<TSource>.Ctor;
        var newTuple = Expression.New(tupleCtor, p, longCountCall);

        var selector = Expression.Lambda<Func<TSource, PrimedResult<TSource>>>(newTuple, p);
        return Queryable.Select(pageQuery, selector);
    }

#pragma warning disable CA1031 // Do not catch general exception types
    private static async ValueTask<long> ExecuteCountAsync<TSource>(
        IQueryable<TSource> query,
        CancellationToken cancellationToken)
    {
        var longCountExpr = Expression.Call(
            QueryableLongCount<TSource>.Method,
            query.Expression);

        if (query.Provider is not null)
        {
            try
            {
                var asyncResult = await TryExecuteAsync<long>(
                    query.Provider,
                    longCountExpr,
                    cancellationToken)
                    .ConfigureAwait(false);

                return asyncResult;
            }
            catch
            {
                // fall through
            }
        }

        // sync fallback
        try
        {
            return query.Provider!.Execute<long>(longCountExpr);
        }
        catch
        {
            var countExpr = Expression.Call(
                QueryableCount<TSource>.Method,
                query.Expression);

            if (query.Provider is not null)
            {
                try
                {
                    var asyncCount = await TryExecuteAsync<int>(
                        query.Provider,
                        countExpr,
                        cancellationToken)
                        .ConfigureAwait(false);

                    return asyncCount;
                }
                catch
                {
                    // fall through
                }
            }

            return query.Provider!.Execute<int>(countExpr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<T> TryExecuteAsync<T>(IQueryProvider provider, Expression expression, CancellationToken ct)
    {
        try
        {
            // Try to call provider.ExecuteAsync<T>(Expression, CancellationToken) without hard provider dependency.
            dynamic dyn = provider;
            var task = (Task<T>)dyn.ExecuteAsync<T>(expression, ct);
            return await task.ConfigureAwait(false);
        }
        catch
        {
            // No async; use sync on worker to keep API async.
            return await Task
                .Run(() => provider.Execute<T>(expression), ct)
                .ConfigureAwait(false);
        }
    }

#pragma warning restore CA1031 // Do not catch general exception types

    // Expression targets cached via method groups (no string-based reflection)
    private static class QueryableLongCount<T>
    {
        public static readonly System.Reflection.MethodInfo Method =
            ((Func<IQueryable<T>, long>)Queryable.LongCount).Method;
    }

    private static class QueryableCount<T>
    {
        public static readonly System.Reflection.MethodInfo Method =
            ((Func<IQueryable<T>, int>)Queryable.Count).Method;
    }

    private static class PrimedFactory<T>
    {
        public static readonly System.Reflection.ConstructorInfo Ctor =
            typeof(PrimedResult<T>).GetConstructor([typeof(T), typeof(long)])
            ?? throw new InvalidOperationException("Could not locate PrimedResult<T> constructor.");
    }
}