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

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents the base class for the repository pattern.
/// </summary>
public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc/>
    public virtual Task<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult(-1);

    /// <inheritdoc/>
    public virtual Task DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => AsyncEnumerable.EmptyAsync<TResult>();

    /// <inheritdoc/>
    public virtual Task InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Optional.Empty<TResult>());

    /// <inheritdoc/>
    public virtual Task<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => Task.FromResult(Optional.Empty<TEntity>());

    /// <inheritdoc/>
    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual Task UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
