﻿/*******************************************************************************
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
using System.Runtime.CompilerServices;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Adds exception handling to the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <param name="decoratee">The repository to decorate.</param>
public class RepositoryDecorator<TEntity>
    (IRepository<TEntity> decoratee) : IRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc/>
    public ValueTask<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => decoratee.CountAsync(filter, cancellationToken)
            .ThrowInvalidOperationException();

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
    public async IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on 
        // the awaited task
        await using IAsyncEnumerator<TResult> asyncEnumerator = decoratee
            .FetchAsync(filter, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on 
        // the awaited task

        TResult? result = default;

        for (bool resultExist = true; resultExist;)
        {
            try
            {
                resultExist = await asyncEnumerator
                              .MoveNextAsync()
                              .ConfigureAwait(false);

                if (resultExist)
                {
                    result = asyncEnumerator.Current;
                }
            }
            catch (Exception exception)
               when (exception is not InvalidOperationException
                               and not OperationCanceledException
                               and not ArgumentNullException)
            {
                throw new InvalidOperationException(
                    exception.Message, exception);
            }

            if (resultExist)
            {
                yield return result!;
            }
            else
            {
                yield break;
            }
        }
    }

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
    public ValueTask<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => decoratee.TryFindAsync(filter, cancellationToken)
            .ThrowInvalidOperationException();

    /// <inheritdoc/>
    public ValueTask<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => decoratee.TryFindByKeyAsync(key, cancellationToken)
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
/// /// Adds exception handling to the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDataContext">The type of data context</typeparam>
/// <param name="decoratee">The repository to decorate.</param>
public sealed class RepositoryDecorator<TEntity, TDataContext>
    (IRepository<TEntity, TDataContext> decoratee) :
    RepositoryDecorator<TEntity>(decoratee),
    IRepository<TEntity, TDataContext>
    where TEntity : class
    where TDataContext : class;