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

using Microsoft.EntityFrameworkCore;

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
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = filter(Context.Set<TEntity>());

        Context.RemoveRange(query);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> queryable = typeof(TEntity) != typeof(TResult)
            ? Context.Set<TEntity>().AsNoTracking()
            : Context.Set<TEntity>();

        IQueryable<TResult> query = filter(queryable);

        return query.AsAsyncEnumerable();
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity =>
        await Context
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);

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