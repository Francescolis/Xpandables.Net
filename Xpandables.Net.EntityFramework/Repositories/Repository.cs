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

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Repositories.Filters;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a repository that provides data access functionality for a
/// specific data context.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
public class Repository<TDataContext>(TDataContext context) : AsyncDisposable, IRepository
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this repository.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected TDataContext Context { get; private set; } = context;

    /// <inheritdoc />
    public virtual Task DeleteAsync<TEntity>(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = filter.Apply(Context.Set<TEntity>()).OfType<TEntity>();

        Context.RemoveRange(query);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(filter);

        IAsyncEnumerable<TResult> results =
            (filter is IEntityFilter<TEntity>) switch
            {
                true => filter.FetchAsync(Context.Set<TEntity>(), cancellationToken),
                _ => filter.FetchAsync(Context.Set<TEntity>().AsNoTracking(), cancellationToken)
            };

        return results;
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, TResult>> selector,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes = null,
        ushort pageIndex = 0,
        ushort pageSize = 0,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        EntityFilter<TEntity, TResult> filter = new()
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Selector = selector,
            Where = where,
            OrderBy = orderBy,
            Includes = includes
        };

        return FetchAsync(filter, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        Context.AddRange(entities);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(entities);
        Context.UpdateRange(entities);
        return Task.CompletedTask;
    }
}