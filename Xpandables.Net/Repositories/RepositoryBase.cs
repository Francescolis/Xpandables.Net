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
    public virtual ValueTask<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => default;

    /// <inheritdoc/>
    public virtual ValueTask DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual ValueTask DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => AsyncEnumerable.EmptyAsync<TResult>();

    /// <inheritdoc/>
    public virtual ValueTask InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual ValueTask InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual ValueTask<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Optional.Empty<TResult>());

    /// <inheritdoc/>
    public virtual ValueTask<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => ValueTask.FromResult(Optional.Empty<TEntity>());

    /// <inheritdoc/>
    public virtual ValueTask UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual ValueTask UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual ValueTask UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
