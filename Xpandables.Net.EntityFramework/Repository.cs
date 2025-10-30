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

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Xpandables.Net;
using Xpandables.Net.Entities;

namespace Xpandables.Net;

/// <summary>
/// Entity Framework Core implementation of the IRepository interface.
/// </summary>
/// <remarks>This implementation provides Entity Framework Core specific operations for managing entities
/// in a relational database. It uses <see cref="DataContext"/> to interact with the database and supports all standard
/// repository operations including querying, adding, updating, and deleting entities asynchronously.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="Repository"/> class.
/// </remarks>
/// <param name="context">The Entity Framework DbContext to use for database operations.</param>
/// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
public class Repository(DataContext context) : DisposableAsync, IRepository
{
    /// <summary>
    /// Gets a value indicating whether operations are executed within a unit of work context.
    /// </summary>
    /// <remarks>This property indicates if the add/updates should be executed as part of a unit of work,
    /// allowing for transactional consistency across multiple operations.
    /// The default value is <see langword="true"/>.</remarks>
    public bool IsUnitOfWorkEnabled { get; set; } = true;

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TResult> FetchAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(filter);

        var query = filter(context.Set<TEntity>().AsNoTracking());

        return query.AsAsyncEnumerable();
    }

    /// <inheritdoc />
    public virtual async Task AddAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        CancellationToken cancellationToken,
        params TEntity[] entities)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentOutOfRangeException.ThrowIfLessThan(entities.Length, 1, nameof(entities));

        if (entities.Length == 1)
        {
            await context.AddAsync(entities[0], cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await context.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        if (!IsUnitOfWorkEnabled)
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        CancellationToken cancellationToken,
        params TEntity[] entities)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentOutOfRangeException.ThrowIfLessThan(entities.Length, 1, nameof(entities));

        context.Set<TEntity>().UpdateRange(entities);
        if (!IsUnitOfWorkEnabled)
        {
            await SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(updateExpression);

        var query = filter(context.Set<TEntity>());

        var compiled = updateExpression.Compile();
        var entities = new List<TEntity>();
        await foreach (TEntity entity in query.AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            TEntity updated = compiled(entity);
            context.Entry(entity).State = EntityState.Detached;
            entities.Add(updated);
        }

        if (entities.Count > 0)
        {
            context.Set<TEntity>().UpdateRange(entities);
            if (!IsUnitOfWorkEnabled)
            {
                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(updateAction);

        var query = filter(context.Set<TEntity>());
        var entities = new List<TEntity>();
        await foreach (TEntity entity in query.AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            updateAction(entity);
            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            context.Set<TEntity>().UpdateRange(entities);
            if (!IsUnitOfWorkEnabled)
            {
                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    [RequiresDynamicCode("Dynamic code generation is required for this method.")]
    [RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
    public virtual async Task UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        EntityUpdater<TEntity> updater,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentOutOfRangeException.ThrowIfLessThan(updater.Updates.Count, 1, nameof(updater.Updates));

        var query = filter(context.Set<TEntity>());

        if (!IsUnitOfWorkEnabled)
        {
            var setters = updater.ToSetPropertyCalls();
            await query.ExecuteUpdateAsync(setters, cancellationToken).ConfigureAwait(false);
            return;
        }

        var entities = new List<TEntity>();
        await foreach (TEntity entity in query.AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            foreach (IEntityPropertyUpdate<TEntity> update in updater.Updates)
            {
                ApplyPropertyUpdate(entity, update);
            }

            entities.Add(entity);
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ObjectDisposedException.ThrowIf(IsDisposed, context);
        ArgumentNullException.ThrowIfNull(filter);

        var query = filter(context.Set<TEntity>());

        if (!IsUnitOfWorkEnabled)
        {
            await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        context.RemoveRange(query);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
            return ValueTask.CompletedTask;

        if (IsDisposed)
            return ValueTask.CompletedTask;

        IsDisposed = true;

        // Note: We don't dispose the DbContext here as it should be managed by the UnitOfWork
        // or dependency injection container
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// When overridden in derived classes, this method get 
    /// called when the instance will be disposed.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to 
    /// release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <list type="bulleted">
    /// <see cref="Dispose(bool)"/> executes in two distinct scenarios.
    /// <item>If <paramref name="disposing"/> equals <c>true</c>, 
    /// the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged 
    /// resources can be disposed.</item>
    /// <item>If <paramref name="disposing"/> equals <c>false</c>, 
    /// the method has been called
    /// by the runtime from inside the finalizer and you should 
    /// not reference other objects.
    /// Only unmanaged resources can be disposed.</item></list>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Release all managed resources here
            // Need to unregister/detach yourself from the events.
            // Always make sure the object is not null first before trying to
            // unregister/detach them!
            // Failure to unregister can be a BIG source of memory leaks
        }

        // Release all unmanaged resources here and override a finalizer below.
        // Set large fields to null.

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }

    /// <summary>
    /// Asynchronously saves all changes made in the current context to the underlying database.
    /// </summary>
    /// <remarks>If a concurrency conflict occurs during the save operation, the method attempts to resolve it
    /// by detaching and re-adding modified entities, then retries the save. This method should be called after making
    /// changes to tracked entities to persist those changes to the database.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    private async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.State = EntityState.Detached;
                    context.Add(entry.Entity);
                }
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Applies a property update to an entity instance.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <param name="update">The property update to apply.</param>
    private static void ApplyPropertyUpdate<TEntity>(
        TEntity entity,
        IEntityPropertyUpdate<TEntity> update)
        where TEntity : class
    {
        // Get the property from the expression
        if (update.PropertyExpression.Body is MemberExpression memberExpression)
        {
            var property = memberExpression.Member as System.Reflection.PropertyInfo;
            if (property != null)
            {
                object? value;
                if (update.IsConstant)
                {
                    var constantExpression = (ConstantExpression)update.ValueExpression;
                    value = constantExpression.Value;
                }
                else
                {
                    // Compile and execute the value expression
                    var valueExpression = (LambdaExpression)update.ValueExpression;
                    var compiledExpression = valueExpression.Compile();
                    value = compiledExpression.DynamicInvoke(entity);
                }

                property.SetValue(entity, value);
            }
        }
    }
}

/// <summary>
/// Provides a repository implementation for Entity Framework operations using the specified data context type.
/// </summary>
/// <typeparam name="TDataContext">The type of the Entity Framework data context to be used by the repository. Must inherit from <see
/// cref="DataContext"/>.</typeparam>
/// <param name="context">The data context instance used to access and manage entities within the repository. Cannot be null.</param>
public class Repository<TDataContext>(TDataContext context) : Repository(context), IRepository<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Provides access to the underlying data context used for database operations.
    /// </summary>
    /// <remarks>This field is intended for use by derived classes to interact with the data source. The data
    /// context should be properly initialized before use.</remarks>
    protected TDataContext Context => context;
}