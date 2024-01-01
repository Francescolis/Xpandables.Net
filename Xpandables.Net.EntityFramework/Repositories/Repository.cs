
/************************************************************************************************************
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
************************************************************************************************************/
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Repositories;

/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <remarks>This is considered as anti-pattern, DbContext already provides abstracted data access.</remarks>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="Repository{TEntity}"/> with the context to act on.
/// </remarks>
/// <param name="context">The data context to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="context"/> is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <remarks>This is considered as anti-pattern, DbContext already provides abstracted data access.</remarks>
public class Repository<TEntity>(DataContext context) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected DataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));

    ///<inheritdoc/>
    public virtual async ValueTask<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => await Context
            .Set<TEntity>()
            .FindAsync([key], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

    ///<inheritdoc/>
    public virtual async ValueTask<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var queryableResult = typeof(IEntity)
            .IsAssignableFrom(typeof(TResult))
                ? filter.GetQueryableFiltered(Context.Set<TEntity>())
                : filter.GetQueryableFiltered(Context.Set<TEntity>().AsNoTracking());

        return await queryableResult
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var queryableResult = typeof(IEntity)
            .IsAssignableFrom(typeof(TResult))
                ? filter.GetQueryableFiltered(Context.Set<TEntity>())
                : filter.GetQueryableFiltered(Context.Set<TEntity>().AsNoTracking());

        return queryableResult.AsAsyncEnumerable();
    }

    ///<inheritdoc/>
    public virtual async ValueTask<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await filter
            .GetQueryableFiltered(Context.Set<TEntity>().AsNoTracking())
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>()
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        Context.Set<TEntity>().Update(entity);
        await ValueTask.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var deleteTableResult = filter
            .GetQueryableFiltered(Context.Set<TEntity>())
            .AsEnumerable();

        if (deleteTableResult.Any())
            Context.RemoveRange(deleteTableResult);

        await ValueTask.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Context
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        Context.UpdateRange(entities);

        await ValueTask.CompletedTask
            .ConfigureAwait(false);
    }

    ///<inheritdoc/>
    public virtual async ValueTask UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(updater);

        var compiledUpdated = updater.Compile();

        await foreach (var entity in FetchAsync(filter, cancellationToken).ConfigureAwait(false))
        {
            var updated = compiledUpdated(entity);
            Context.Entry(entity).CurrentValues.SetValues(updated);
        }
    }
}

/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <remarks>This is considered as anti-pattern, DbContext already provides abstracted data access.</remarks>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="Repository{TEntity}"/> with the context to act on.
/// </remarks>
/// <param name="context">The data context to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="context"/> is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <remarks>This is considered as anti-pattern, DbContext already provides abstracted data access.</remarks>
public class Repository<TEntity, TDataContext>(TDataContext context)
    : Repository<TEntity>(context)
    where TEntity : class, IEntity
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected new TDataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));
}