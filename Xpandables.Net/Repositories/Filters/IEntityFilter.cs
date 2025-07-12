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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Defines a filter for entities, providing options to customize query behavior.
/// </summary>
/// <remarks>This interface includes a static property to control whether a total count of entities should be
/// forced during filtering operations, even if the underlying data source does not natively support counting.</remarks>
public interface IEntityFilter
{
    /// <summary>
    /// A static boolean property that indicates whether to force a total count when applying the filter.
    /// If set to true, the total count will be calculated even if the queryable does not support it.
    /// </summary>
    public static bool ForceTotalCount { get; set; } = true;
}
/// <summary>
/// Represents a filter for entities with pagination support and specific 
/// selection criteria.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IEntityFilter<TEntity, TResult> : IEntityFilter
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets the index of the page (1-based).
    /// </summary>
    /// <remarks>Use 0 or 1 to start from the first page. Values less than 1 will disable pagination.</remarks>
    ushort PageIndex { get; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    /// <remarks>Use 0 to disable pagination.</remarks>
    ushort PageSize { get; }

    /// <summary>
    /// Gets or sets the total number of elements in the collection.
    /// </summary>
    /// <remarks>
    /// This value is automatically set when the filter is applied to a queryable.
    /// You can control the total count calculation using the <see cref="IEntityFilter.ForceTotalCount"/> property.
    /// </remarks>
    int TotalCount { get; set; }

    /// <summary>
    /// Gets the selector expression for projecting the entity to the result type.
    /// </summary>
    Expression<Func<TEntity, TResult>> Selector { get; }

    /// <summary>
    /// Gets the predicate expression for filtering entities.
    /// </summary>
    Expression<Func<TEntity, bool>>? Where { get; }

    /// <summary>
    /// Gets the function for ordering the entities.
    /// </summary>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }

    /// <summary>
    /// Gets the function for including related entities in the query.
    /// </summary>
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? Includes { get; }

    /// <summary>
    /// Applies the filter to the given queryable and returns the filtered queryable with projection.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered and projected queryable.</returns>
    public IQueryable<TResult> Apply(IQueryable<TEntity> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        IQueryable<TEntity> query = queryable;

        if (Where is not null)
        {
            query = query.Where(Where);
        }

        if (OrderBy is not null)
        {
            query = OrderBy(query);
        }

        if (Includes is not null)
        {
            query = Includes(query);
        }

        SetTotalCount(query);

        if (PageIndex > 0 && PageSize > 0)
        {
            query = query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize);
        }

        return query.Select(Selector);
    }

    /// <summary>
    /// Applies the filter to the given non-generic queryable.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IQueryable Apply(IQueryable queryable) => Apply((IQueryable<TEntity>)queryable);

    /// <summary>
    /// Fetches a collection of results from the given queryable asynchronously.
    /// </summary>
    /// <param name="queryable">The queryable to fetch results from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous collection of results.</returns>
    public IAsyncEnumerable<TResult> FetchAsync(
        IQueryable<TEntity> queryable,
        CancellationToken cancellationToken = default) =>
        Apply(queryable).ToAsyncEnumerable();

    /// <summary>
    /// Gets the string representation of the query after applying the filter.
    /// This method is useful for debugging or creating ADO.NET queries.
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>A string representation of the query, or the queryable's ToString() if no specific query string method is available.</returns>
    /// <remarks>
    /// This method attempts to use Entity Framework Core's ToQueryString() extension method if available.
    /// For non-EF queryables, it falls back to the standard ToString() method.
    /// The returned string may not be suitable for direct execution and is intended primarily for debugging purposes.
    /// </remarks>
    public string ToQueryString(IQueryable<TEntity> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        IQueryable<TResult> filteredQuery = Apply(queryable);

        // Try to use EF Core's ToQueryString() method if available
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            // Check if EntityFramework ToQueryString extension is available
            var efCoreAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Microsoft.EntityFrameworkCore");

            if (efCoreAssembly is not null)
            {
                var extensionsType = efCoreAssembly.GetType("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
                var method = extensionsType?.GetMethod("ToQueryString",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    [typeof(IQueryable)],
                    null);

                if (method is not null)
                {
                    return (string)method.Invoke(null, [filteredQuery])!;
                }
            }
        }
        catch
        {
            // Fall back to ToString() if EF Core is not available or method fails
        }
#pragma warning restore CA1031 // Do not catch general exception types

        // Fallback to the standard ToString() method
        return filteredQuery.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Sets the total count by attempting to get it efficiently.
    /// </summary>
    /// <param name="query">The query to count.</param>
    protected void SetTotalCount(IQueryable<TEntity> query)
    {
        if (query.TryGetNonEnumeratedCount(out int count))
        {
            TotalCount = count;
        }
        else if (ForceTotalCount)
        {
            TotalCount = query.Count();
        }
    }
}

/// <summary>
/// Represents a filter for entities with pagination support where the entity type 
/// is both the input and output type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntityFilter<TEntity> : IEntityFilter<TEntity, TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Applies the filter to the given queryable without projection (returns entities directly).
    /// </summary>
    /// <param name="queryable">The queryable to apply the filter to.</param>
    /// <returns>The filtered queryable of entities.</returns>
    public new IQueryable<TEntity> Apply(IQueryable<TEntity> queryable)
    {
        // Cast to the generic interface to access the implementation
        var genericFilter = (IEntityFilter<TEntity, TEntity>)this;
        return genericFilter.Apply(queryable);
    }

    /// <summary>
    /// Fetches entities from the given queryable asynchronously.
    /// </summary>
    /// <param name="queryable">The queryable to fetch entities from.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous collection of entities.</returns>
    public new IAsyncEnumerable<TEntity> FetchAsync(
        IQueryable<TEntity> queryable,
        CancellationToken cancellationToken = default) =>
        Apply(queryable).ToAsyncEnumerable();
}
