
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
using Microsoft.EntityFrameworkCore;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Repositories;

/// <summary>
/// An implementation of <see cref="IRepositoryRead{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="RepositoryRead{TEntity}"/> 
/// with the context to act on.
/// </remarks>
/// <param name="context">The data context to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="context"/> 
/// is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepositoryRead{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class RepositoryRead<TEntity>(DataContext context)
    : IRepositoryRead<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected DataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));

    ///<inheritdoc/>
    public virtual async Task<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => await Context
            .Set<TEntity>()
            .FindAsync([key], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

    ///<inheritdoc/>
    public virtual async Task<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TResult> queryableResult = typeof(IEntity)
            .IsAssignableFrom(typeof(TResult))
                ? filter.GetQueryableFiltered(Context.Set<TEntity>())
                : filter.GetQueryableFiltered(Context.Set<TEntity>()
                    .AsNoTracking());

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

        IQueryable<TResult> queryableResult = typeof(IEntity)
            .IsAssignableFrom(typeof(TResult))
                ? filter.GetQueryableFiltered(Context.Set<TEntity>())
                : filter.GetQueryableFiltered(Context.Set<TEntity>()
                    .AsNoTracking());

        return queryableResult.AsAsyncEnumerable();
    }

    ///<inheritdoc/>
    public virtual async Task<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await filter
            .GetQueryableFiltered(Context.Set<TEntity>().AsNoTracking())
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
    }
}

/// <summary>
/// An implementation of <see cref="IRepositoryRead{TEntity}"/> for EFCore.
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
/// An implementation of <see cref="IRepositoryRead{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class RepositoryRead<TEntity, TDataContext>(TDataContext context)
    : RepositoryRead<TEntity>(context), IRepositoryRead<TEntity, TDataContext>
    where TEntity : class
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the current context instance.
    /// </summary>
    protected new TDataContext Context { get; init; } = context
        ?? throw new ArgumentNullException(nameof(context));
}