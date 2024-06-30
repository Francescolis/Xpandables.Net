
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

using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.EntityFramework.Repositories;

/// <summary>
/// An implementation of <see cref="IRepositoryWrite{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="RepositoryWrite{TEntity}"/> 
/// with the context to act on.
/// </remarks>
/// <param name="context">The data context to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="context"/> 
/// is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepositoryWrite{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class RepositoryWrite<TEntity>(DataContext context)
    : IRepositoryWrite<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected DataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));

    ///<inheritdoc/>
    public virtual async Task InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => _ = await Context.Set<TEntity>()
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

    ///<inheritdoc/>
    public virtual async Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        _ = Context.Set<TEntity>().Update(entity);
        await Task.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async Task DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IEnumerable<TEntity> deleteTableResult = filter
            .GetQueryableFiltered(Context.Set<TEntity>())
            .AsEnumerable();

        if (deleteTableResult.Any())
        {
            Context.RemoveRange(deleteTableResult);
        }

        await Task.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Context
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        Context.UpdateRange(entities);

        await Task.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async Task UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(updater);

        Func<TEntity, object> compiledUpdated = updater.Compile();

        await foreach (TEntity entity
            in filter.GetQueryableFiltered(Context.Set<TEntity>())
            .AsAsyncEnumerable()
            .ConfigureAwait(false))
        {
            object updated = compiledUpdated(entity);
            Context.Entry(entity).CurrentValues.SetValues(updated);
        }
    }

    ///<inheritdoc/>
    public virtual async Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _ = Context.Remove(entity);

        await Task.CompletedTask
            .ConfigureAwait(false);
    }
}

/// <summary>
/// An implementation of <see cref="IRepositoryWrite{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
/// <remarks>
/// Initializes a new instance of 
/// <see cref="RepositoryRead{TEntity, TDataContext}"/> with the context to act on.
/// </remarks>
/// <param name="context">The data context to act on.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="context"/> is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepositoryWrite{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class RepositoryWrite<TEntity, TDataContext>(TDataContext context)
    : RepositoryWrite<TEntity>(context), IRepositoryWrite<TEntity, TDataContext>
    where TEntity : class
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected new TDataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));
}