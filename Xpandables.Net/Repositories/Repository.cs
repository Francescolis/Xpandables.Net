
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

namespace Xpandables.Net.Repositories;

/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="Repository{TEntity}"/> 
/// with the context to act on.
/// </remarks>
/// <param name="repositoryRead">The read repository to act on.</param>
/// <param name="repositoryWrite">The write repository to act on.</param>
/// <exception cref="ArgumentNullException">The <paramref name="repositoryRead"/> 
/// or <paramref name="repositoryWrite"/> is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class Repository<TEntity>(
    IRepositoryRead<TEntity> repositoryRead,
    IRepositoryWrite<TEntity> repositoryWrite) : IRepository<TEntity>
    where TEntity : class
{
    ///<inheritdoc/>
    public virtual Task<Optional<TEntity>> TryFindByKeyAsync<TKey>(
        TKey key,
        CancellationToken cancellationToken = default)
        where TKey : notnull, IComparable
        => repositoryRead.TryFindByKeyAsync(key, cancellationToken);

    ///<inheritdoc/>
    public virtual Task<Optional<TResult>> TryFindAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => repositoryRead.TryFindAsync(filter, cancellationToken);

    ///<inheritdoc/>
    public IAsyncEnumerable<TResult> FetchAsync<TResult>(
        IEntityFilter<TEntity, TResult> filter,
        CancellationToken cancellationToken = default)
        => repositoryRead.FetchAsync(filter, cancellationToken);

    ///<inheritdoc/>
    public virtual Task<int> CountAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => repositoryRead.CountAsync(filter, cancellationToken);

    ///<inheritdoc/>
    public virtual Task InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => repositoryWrite.InsertAsync(entity, cancellationToken);

    ///<inheritdoc/>
    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => repositoryWrite.UpdateAsync(entity, cancellationToken);

    ///<inheritdoc/>
    public virtual Task DeleteAsync(
        IEntityFilter<TEntity> filter,
        CancellationToken cancellationToken = default)
        => repositoryWrite.DeleteAsync(filter, cancellationToken);

    ///<inheritdoc/>
    public virtual Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => repositoryWrite.InsertManyAsync(entities, cancellationToken);

    ///<inheritdoc/>
    public virtual Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => repositoryWrite.UpdateManyAsync(entities, cancellationToken);

    ///<inheritdoc/>
    public virtual Task UpdateManyAsync(
        IEntityFilter<TEntity> filter,
        Expression<Func<TEntity, object>> updater,
        CancellationToken cancellationToken = default)
        => repositoryWrite.UpdateManyAsync(filter, updater, cancellationToken);

    ///<inheritdoc/>
    public virtual Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => repositoryWrite.DeleteAsync(entity, cancellationToken);
}

/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
/// <typeparam name="TEntity">The Domain object type.</typeparam>
/// <typeparam name="TDataContext">The type of the data context.</typeparam>
/// <remarks>
/// Initializes a new instance of 
/// <see cref="Repository{TEntity, TDataContext}"/> with the context to act on.
/// </remarks>
/// <param name="repositoryRead">The read repository to act on.</param>
/// <param name="repositoryWrite">The write repository to act on.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="repositoryWrite"/> or <paramref name="repositoryRead"/> 
/// is null.</exception>
/// <summary>
/// An implementation of <see cref="IRepository{TEntity}"/> for EFCore.
/// You must derive from this class to customize its behaviors.
/// </summary>
public class Repository<TEntity, TDataContext>(
    IRepositoryRead<TEntity, TDataContext> repositoryRead,
    IRepositoryWrite<TEntity, TDataContext> repositoryWrite) :
    Repository<TEntity>(repositoryRead, repositoryWrite),
    IRepository<TEntity, TDataContext>
    where TEntity : class
    where TDataContext : class
{
}