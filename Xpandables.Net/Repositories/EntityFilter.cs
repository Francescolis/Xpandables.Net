
/*******************************************************************************
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
********************************************************************************/

// Ignore Spelling: Queryable

using System.Linq.Expressions;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Specifies base criteria with projection for entities.
/// </summary>
/// <typeparam name="TEntity">The type of the target entity.</typeparam>
public record EntityFilter<TEntity>
    : EntityFilter<TEntity, TEntity>, IEntityFilter<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Creates a new instance of <see cref="EntityFilter{TEntity}"/> 
    /// to filter entities 
    /// of <typeparamref name="TEntity"/> type 
    /// and return result of <typeparamref name="TEntity"/> type.
    /// </summary>
    public EntityFilter() : base() { }

    /// <summary>
    /// Specifies the projection filter to be applied on specific type entities.
    /// The value is : x => x;
    /// </summary>
    public sealed override Expression<Func<TEntity, TEntity>>
        Selector
    { get; init; } = x => x;
}

/// <summary>
/// Specifies criteria with projection for entities to a specific result.
/// </summary>
/// <typeparam name="TEntity">The type of the target entity.</typeparam>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>You must at least provides a value for the "Selector".</remarks>
public record EntityFilter<TEntity, TResult> : IEntityFilter<TEntity, TResult>
    where TEntity : class
{
    /// <summary>
    /// Creates a new instance of 
    /// <see cref="EntityFilter{TEntity, TResult}"/> to filter entities 
    /// of <typeparamref name="TEntity"/> type 
    /// and return result of <typeparamref name="TResult"/> type.
    /// </summary>
    /// <remarks>You must define the 
    /// <see cref="EntityFilter{TEntity, TResult}.Selector"/> property, 
    /// otherwise an error will occur.</remarks>
    public EntityFilter() { }

    ///<inheritdoc/>
    public virtual Expression<Func<TEntity, bool>>? Criteria { get; init; }

    ///<inheritdoc/>
    public virtual Expression<Func<TEntity, TResult>> Selector { get; init; }
        = default!;

    ///<inheritdoc/>
    public virtual Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>?
        OrderBy
    { get; init; }

    ///<inheritdoc/>
    public virtual Pagination? Paging { get; set; }

    /// <summary>
    /// Applies the filters to the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>The queryable instance where the filters have been applied.</returns>
    public IQueryable<TResult> Apply(
        IQueryable<TEntity> queryable)
    {
        if (Criteria is not null)
        {
            queryable = queryable.Where(Criteria);
        }

        if (OrderBy is not null)
        {
            queryable = OrderBy(queryable);
        }

        if (Paging is not null)
        {
            queryable = queryable
                .Skip(Paging.Value.Index * Paging.Value.Size)
                .Take(Paging.Value.Size);
        }

        return queryable.Select(Selector);
    }
}
