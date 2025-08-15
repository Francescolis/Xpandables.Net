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

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    #region Cached Reflection Operations

    // Memory-aware caches that automatically clean up unused entries
    private static readonly MemoryAwareCache<Type, bool> _asyncPagedEnumerableTypeCache = 
        new(TimeSpan.FromMinutes(10), TimeSpan.FromHours(2));

    private static readonly MemoryAwareCache<Type, MethodInfo?> _entityFrameworkCountMethodCache = 
        new(TimeSpan.FromMinutes(15), TimeSpan.FromHours(4));

    private static readonly MemoryAwareCache<Type, QueryableExtractionInfo?> _queryableExtractionCache = 
        new(TimeSpan.FromMinutes(10), TimeSpan.FromHours(2));

    // Use ConditionalWeakTable for delegate caching - automatically cleaned when key is collected
    private static readonly ConditionalWeakTable<Type, Func<object, IQueryable?>> _queryableExtractors = new();

    // Static constructor to handle cleanup on app shutdown
    static AsyncPagedEnumerableExtensions()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanupCaches();
        AppDomain.CurrentDomain.DomainUnload += (_, _) => CleanupCaches();
    }

    private static void CleanupCaches()
    {
        _asyncPagedEnumerableTypeCache?.Dispose();
        _entityFrameworkCountMethodCache?.Dispose();
        _queryableExtractionCache?.Dispose();
    }

    #endregion

    #region Public API Methods

    /// <summary>
    /// Determines whether the specified type implements the <see cref="IAsyncPagedEnumerable{T}"/> interface.
    /// </summary>
    /// <param name="type">The type to check for implementation of <see cref="IAsyncPagedEnumerable{T}"/>.</param>
    /// <returns><see langword="true"/> if the specified type implements <see cref="IAsyncPagedEnumerable{T}"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsyncPagedEnumerable(Type type) =>
        _asyncPagedEnumerableTypeCache.GetOrAdd(type, static t =>
            t.GetInterfaces().Any(i => i.IsGenericType &&
                                      i.GetGenericTypeDefinition() == typeof(IAsyncPagedEnumerable<>))) ?? false;

    /// <summary>
    /// Determines whether the specified asynchronous enumerable is an instance of <see
    /// cref="IAsyncPagedEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the asynchronous enumerable.</typeparam>
    /// <param name="source">The asynchronous enumerable to check. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified <paramref name="source"/> is an <see cref="IAsyncPagedEnumerable{T}"/>;
    /// otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsyncPagedEnumerable<T>(this IAsyncEnumerable<T> source) =>
        source is IAsyncPagedEnumerable<T>;

    /// <summary>
    /// Converts an <see cref="IAsyncEnumerable{T}"/> to an <see cref="IAsyncPagedEnumerable{T}"/> with smart pagination.
    /// Uses memory-efficient caching and automatic cleanup to prevent memory leaks.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <returns>An async paged enumerable with optimized pagination metadata.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncPagedEnumerable<TSource> WithPagination<TSource>(
        this IAsyncEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Fast path: if already paged, return as-is
        if (source is IAsyncPagedEnumerable<TSource> existingPaged)
        {
            return existingPaged;
        }

        return source.WithPagination(CreateOptimizedPaginationFactory<TSource>(source));
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// High-performance method to attempt extracting a count from various async enumerable sources.
    /// Uses memory-aware caching to prevent memory leaks.
    /// </summary>
    /// <typeparam name="TSource">The type of items in the collection.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The total count if available without enumeration, otherwise null.</returns>
    public static async ValueTask<long?> TryGetCountAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Fast path: Check if it's already paged with known count
        if (source is IAsyncPagedEnumerable<TSource> pagedSource)
        {
            try
            {
                var pagination = await pagedSource.GetPaginationAsync().ConfigureAwait(false);
                return pagination.TotalCount >= 0 ? pagination.TotalCount : null;
            }
            catch
            {
                // Pagination retrieval failed, continue to other strategies
            }
        }

        // Try to extract queryable for efficient counting
        var queryable = TryExtractQueryableUsingCache<TSource>(source);
        if (queryable is not null)
        {
            try
            {
                // Try Entity Framework's CountAsync if available
                var efCountMethod = GetEntityFrameworkCountMethod<TSource>();
                if (efCountMethod is not null)
                {
                    var task = (Task<int>)efCountMethod.Invoke(null, [queryable, cancellationToken])!;
                    return await task.ConfigureAwait(false);
                }

                // Fall back to synchronous count for LINQ-to-Objects
                return queryable.LongCount();
            }
            catch
            {
                // Count operation failed
            }
        }

        // No efficient count method available
        return null;
    }

    #endregion

    #region Memory-Efficient Private Methods

    /// <summary>
    /// Creates an optimized pagination factory that attempts various strategies for getting count efficiently.
    /// </summary>
    private static Func<Task<Pagination>> CreateOptimizedPaginationFactory<TSource>(
        IAsyncEnumerable<TSource> source)
    {
        return async () =>
        {
            // Try to get count efficiently first
            var count = await source.TryGetCountAsync().ConfigureAwait(false);
            
            // If we got a count, use it; otherwise indicate unknown count
            return count.HasValue ? Pagination.Without(count.Value) : Pagination.Without();
        };
    }

    /// <summary>
    /// Gets Entity Framework's CountAsync method with memory-aware caching.
    /// </summary>
    private static MethodInfo? GetEntityFrameworkCountMethod<TSource>()
    {
        return _entityFrameworkCountMethodCache.GetOrAdd(typeof(TSource), static _ =>
        {
            try
            {
                var efAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Microsoft.EntityFrameworkCore");

                if (efAssembly is null) return null;

                var extensionsType = efAssembly.GetType("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
                if (extensionsType is null) return null;

                var countMethods = extensionsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "CountAsync" && 
                               m.IsGenericMethod && 
                               m.GetParameters().Length == 2)
                    .FirstOrDefault();

                return countMethods?.MakeGenericMethod(typeof(TSource));
            }
            catch
            {
                return null;
            }
        });
    }

    #endregion

    /// <summary>
    /// Cached information about queryable extraction capabilities.
    /// </summary>
    private sealed record QueryableExtractionInfo(
        bool CanExtract,
        FieldInfo? Field,
        PropertyInfo? Property)
    {
        /// <summary>
        /// Represents a queryable extraction info that indicates no extraction is possible.
        /// </summary>
        public static readonly QueryableExtractionInfo None = new(false, null, null);
    }
}