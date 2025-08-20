
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

using Xpandables.Net.Collections;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a repository that provides data access functionality for a
/// specific data context.
/// </summary>
public class Repository<TDataContext> : Repository, IRepository<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this repository.
    /// </summary>
    public required TDataContext Context { get; set; }

    /// <summary>
    /// Asynchronously deletes entities from the database that match the specified filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to delete.</typeparam>
    /// <param name="filter">A function to filter the entities to be deleted. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public override async Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = filter(Context.Set<TEntity>());

        Context.RemoveRange(query);

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously fetches a sequence of results from the database based on the specified filter.
    /// </summary>
    /// <remarks>The method uses asynchronous streaming to yield results as they are retrieved from the
    /// database. If <typeparamref name="TEntity"/> and <typeparamref name="TResult"/> are different types, the query is
    /// executed with no tracking to improve performance.</remarks>
    /// <typeparam name="TEntity">The type of the entity to query.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="filter">A function to apply a filter to the queryable entity set.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous sequence of results of type <typeparamref name="TResult"/>.</returns>
    public override IAsyncPagedEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> baseQuery = typeof(TEntity) != typeof(TResult)
            ? Context.Set<TEntity>().AsNoTracking()
            : Context.Set<TEntity>();

        var filteredQuery = filter(baseQuery);

        return filteredQuery.WithPagination();
    }

    /// <summary>
    /// Asynchronously adds or updates a collection of entities in the database.
    /// </summary>
    /// <remarks>This method uses the underlying database context to add the specified entities. The operation
    /// is performed asynchronously and can be cancelled using the provided <paramref
    /// name="cancellationToken"/>.</remarks>
    /// <typeparam name="TEntity">The type of the entities to add or update. Must implement <see cref="IEntity"/>.</typeparam>
    /// <param name="entities">The collection of entities to add or update. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task AddOrUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities is not ICollection<TEntity> collection || collection.Count == 0)
        {
            return;
        }

        // Convert to array to avoid multiple enumeration
        var entitiesArray = collection as TEntity[] ?? [.. collection];

        // Get entities to add (those with default KeyId values)
        var entitiesToAdd = entitiesArray
            .Where(e => !Context.Entry(e).IsKeySet)
            .ToArray();

        // Get entities to update using Except
        var entitiesToUpdate = entitiesArray.Except(entitiesToAdd).ToArray();

        // Bulk add new entities
        if (entitiesToAdd.Length > 0)
        {
            Context.AddRange(entitiesToAdd);
        }

        // Bulk update existing entities
        if (entitiesToUpdate.Length > 0)
        {
            Context.UpdateRange(entitiesToUpdate);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a repository that provides persistent data operations for a specified data context.
/// </summary>
/// <remarks>This class extends the functionality of <see cref="Repository{TDataContext}"/> by ensuring that
/// changes made to the data context are persisted to the database. It provides methods for deleting entities and adding
/// or updating entities, with changes automatically committed to the underlying data source.</remarks>
/// <typeparam name="TDataContext">The type of the data context used by the repository. 
/// Must derive from <see cref="DataContext"/>.</typeparam>
public class RepositoryPersistent<TDataContext> : Repository<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Deletes entities from the data source that match the specified filter.
    /// </summary>
    /// <remarks>This method deletes the entities that match the specified filter and commits the changes to
    /// the database.</remarks>
    /// <typeparam name="TEntity">The type of the entities to delete.</typeparam>
    /// <param name="filter">A function that applies a queryable filter to select the entities to delete. The function should return a
    /// filtered <see cref="IQueryable{T}"/> of the entities to be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns></returns>
    public override async Task DeleteAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken)
    {
        await base.DeleteAsync(filter, cancellationToken).ConfigureAwait(false);
        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds new entities or updates existing entities in the database asynchronously.
    /// </summary>
    /// <remarks>This method ensures that changes are persisted to the database by calling <see
    /// cref="DbContext.SaveChangesAsync(CancellationToken)"/>.</remarks>
    /// <typeparam name="TEntity">The type of the entities to add or update.</typeparam>
    /// <param name="entities">The collection of entities to be added or updated. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public override async Task AddOrUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        await base.AddOrUpdateAsync(entities, cancellationToken).ConfigureAwait(false);

        try
        {
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency exceptions by treating failed updates as inserts
            foreach (var entry in ex.Entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    // Detach the entity and try to add it instead
                    entry.State = EntityState.Detached;
                    Context.Add(entry.Entity);
                }
            }

            // Retry saving changes
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}