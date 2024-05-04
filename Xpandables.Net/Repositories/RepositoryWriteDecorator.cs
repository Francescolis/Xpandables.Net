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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Adds exception handling to the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <param name="decoratee">The repository to decorate.</param>
public class RepositoryWriteDecorator<TEntity>
    (IRepositoryWrite<TEntity> decoratee) :
    IRepositoryWrite<TEntity>
    where TEntity : class
{
    /// <inheritdoc/>
    public ValueTask DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => decoratee.DeleteAsync(filter, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => decoratee.DeleteAsync(entity, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => decoratee.InsertAsync(entity, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => decoratee.InsertManyAsync(entities, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => decoratee.UpdateAsync(entity, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => decoratee.UpdateManyAsync(entities, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
        => decoratee.UpdateManyAsync(filter, updater, cancellationToken)
            .ThrowInvalidOperationException();
}

/// <summary>
/// Adds exception handling to the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
/// <param name="decoratee">The repository to decorate.</param>
public class RepositoryWriteDecorator<TEntity, TDataContext>
    (IRepositoryWrite<TEntity, TDataContext> decoratee) :
    RepositoryWriteDecorator<TEntity>(decoratee),
    IRepositoryWrite<TEntity, TDataContext>
    where TEntity : class
    where TDataContext : class;