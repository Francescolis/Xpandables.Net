
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
using System.Linq.Expressions;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents the base criteria to search for entities.
/// </summary>
/// <remarks>To be applied, just call the instance method like this :
/// <code><list type="table"><item>IEntityFilter filter 
/// = new EntityFilter ...;
/// </item><item>filter.Apply(queryable);</item></list></code>
/// where queryable is the data source that implement 
/// <see cref="IQueryable"/>.</remarks>
public interface IEntityFilter
{
    /// <summary>
    /// Used to define the pagination of the request result.
    /// </summary>
    /// <remarks>Example : <code>Paging = Paging.With(1,20); </code></remarks>
    Pagination? Paging { get; set; }

    /// <summary>
    /// Applies the filters to the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>The queryable instance where 
    /// the filters have been applied.</returns>
    IQueryable Apply(IQueryable queryable);
}

/// <summary>
/// Represents the base criteria to search for entities.
/// </summary>
/// <remarks>To be applied, just call the instance method like this :
/// <code><list type="table"><item>IEntityFilter{TEntity} filter 
/// = new EntityFilter{TEntity} ...;
/// </item><item>filter.GetQueryable(queryable);</item></list></code>
/// where queryable is the data source that implement 
/// <see cref="IQueryable{T}"/> with T as <typeparamref name="TEntity"/>.</remarks>
/// <typeparam name="TEntity">The type of the target entity.</typeparam>
/// <typeparam name="TResult">The type of result.</typeparam>
public interface IEntityFilter<TEntity, TResult> : IEntityFilter
    where TEntity : class
{
    /// <summary>
    /// Specifies the projection filter to be applied on specific type entities.
    /// </summary>
    /// <remarks>You can set the value to 
    /// <code>Selector = x => x;</code> to return the same value.</remarks>
    Expression<Func<TEntity, TResult>> Selector { get; }

    /// <summary>
    /// Specifies the filter criteria to be applied on specific type entities.
    /// </summary>
    /// <remarks>You can set the value to <code>Criteria = x => true;</code> 
    /// to return all entities of the specific type, or you can use
    /// the <see langword="Specification"/> pattern.</remarks>
    Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Used to define the sorting operation.
    /// </summary>
    /// <remarks>Example : <code>OrderBy = x 
    /// => x.OrderBy(o => o.Version);</code></remarks>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }

    /// <summary>
    /// Applies the filters to the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>The queryable instance where 
    /// the filters have been applied.</returns>
    public IQueryable<TResult> Apply(IQueryable<TEntity> queryable)
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

    IQueryable IEntityFilter.Apply(IQueryable queryable)
        => Apply(queryable.OfType<TEntity>());
}

/// <summary>
/// Represents the base criteria to search for entities.
/// </summary>
/// <remarks>To be applied, just call the instance method like this :
/// <code><list type="table"><item>IEntityFilter{TEntity} filter
/// = new EntityFilter{TEntity} ...;</item>
/// <item>filter.GetQueryableFiltered(queryable);</item></list></code>
/// where queryable is the data source that implement 
/// <see cref="IQueryable{T}"/> with T as <typeparamref name="TEntity"/>.</remarks>
/// <typeparam name="TEntity">The type of the target entity.</typeparam>
public interface IEntityFilter<TEntity> : IEntityFilter<TEntity, TEntity>
    where TEntity : class
{ }