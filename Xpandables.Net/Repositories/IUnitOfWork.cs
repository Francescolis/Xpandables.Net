
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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides with the base unit of work interface.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Persists all pending objects to the data storage according 
    /// to the database provider/ORM.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the number of persisted objects
    /// .</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    /// <exception cref="OperationCanceledException">The 
    /// operation has been canceled.</exception>
    ValueTask PersistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the repository implementation that matches 
    /// the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <returns>An instance of an object that 
    /// implements <see cref="IRepository{TEntity}"/> interface.</returns>
    IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Returns the repository read implementation that matches 
    /// the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <returns>An instance of an object that 
    /// implements <see cref="IRepository{TEntity}"/> interface.</returns>
    IRepositoryRead<TEntity> GetRepositoryRead<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Returns the repository write implementation that matches 
    /// the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <returns>An instance of an object that 
    /// implements <see cref="IRepository{TEntity}"/> interface.</returns>
    IRepositoryWrite<TEntity> GetRepositoryWrite<TEntity>()
        where TEntity : class;
}

/// <summary>
/// Provides with the base unit of work interface for a specific data context.
/// </summary>
/// <typeparam name="TDataContext">The type of the context.</typeparam>
public interface IUnitOfWork<TDataContext> : IUnitOfWork
    where TDataContext : class
{
    /// <summary>
    /// Returns the repository read implementation that matches 
    /// the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <returns>An instance of an object that 
    /// implements <see cref="IRepository{TEntity}"/> interface.</returns>
    new IRepositoryRead<TEntity, TDataContext> GetRepositoryRead<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Returns the repository write implementation that matches 
    /// the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the target entity.</typeparam>
    /// <returns>An instance of an object that 
    /// implements <see cref="IRepository{TEntity}"/> interface.</returns>
    new IRepositoryWrite<TEntity, TDataContext> GetRepositoryWrite<TEntity>()
        where TEntity : class;
}