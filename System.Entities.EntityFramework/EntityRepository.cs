/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

namespace System.Entities.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of the IEntityRepository interface.
/// </summary>
/// <remarks>This implementation provides Entity Framework Core specific operations for managing entities
/// in a relational database. It uses <see cref="DataContext"/> to interact with the database and supports all standard
/// repository operations including querying, adding, updating, and deleting entities asynchronously.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="EntityRepository{TEntity}"/> class.
/// </remarks>
/// <param name="context">The Entity Framework DbContext to use for database operations.</param>
/// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
/// <typeparam name="TEntity">The type of the entity to query from the data source. Must be a reference type.</typeparam>
public class EntityRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>(DataContext context) :
    IEntityRepository<TEntity>, IAmbientContextReceiver<DataContext>
    where TEntity : class
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>")]
    private DataContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets or sets the data context used for database operations.
    /// </summary>
    /// <remarks>The context must be provided and cannot be null. It is used to manage the database connection
    /// and track changes to entities.</remarks>
    protected DataContext Context => _context;

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    /// <remarks>Use this property to determine if the object is no longer usable due to disposal. Accessing
    /// members of a disposed object may result in exceptions or undefined behavior.</remarks>
    protected bool IsDisposed { get; set; }

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "<Pending>")]
    void IAmbientContextReceiver<DataContext>.SetAmbientContext(DataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public virtual IAsyncPagedEnumerable<TResult> FetchAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return query.ToAsyncPagedEnumerable();
    }

    /// <inheritdoc />
    public virtual async Task<TResult> FetchSingleAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.SingleAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> FetchSingleOrDefaultAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<TResult> FetchFirstAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.FirstAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> FetchFirstOrDefaultAsync<TResult>(
        IQuerySpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> AddAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities as IList<TEntity> ?? [.. entities];
        ArgumentOutOfRangeException.ThrowIfLessThan(entityList.Count, 1, nameof(entities));

        if (entityList.Count == 1)
        {
            await Context.AddAsync(entityList[0], cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await Context.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);
        }

        return entityList.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities as IList<TEntity> ?? [.. entities];
        ArgumentOutOfRangeException.ThrowIfLessThan(entityList.Count, 1, nameof(entities));

        Context.Set<TEntity>().UpdateRange(entityList);

        return entityList.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(updateExpression);

        var query = ApplyEntitySpecification(specification);

        var compiled = updateExpression.Compile();
        var entities = new List<TEntity>();
        await foreach (TEntity entity in query.AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            TEntity updated = compiled(entity);
            Context.Entry(entity).State = EntityState.Detached;
            entities.Add(updated);
        }

        if (entities.Count > 0)
        {
            Context.Set<TEntity>().UpdateRange(entities);
        }

        return entities.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(updateAction);

        var query = ApplyEntitySpecification(specification);
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
            Context.Set<TEntity>().UpdateRange(entities);
        }

        return entities.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        EntityUpdater<TEntity> updater,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentOutOfRangeException.ThrowIfLessThan(updater.Updates.Count, 1, nameof(updater.Updates));

        var query = ApplyEntitySpecification(specification);

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

        return entities.Count;
    }

    /// <inheritdoc/>
    public virtual async Task<int> UpdateBulkAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        EntityUpdater<TEntity> updater,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentOutOfRangeException.ThrowIfLessThan(updater.Updates.Count, 1, nameof(updater.Updates));

        var query = ApplyEntitySpecification(specification);

        var setters = updater.ToSetPropertyCalls();
        return await query
            .ExecuteUpdateAsync(setters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplyEntitySpecification(specification);

        var entityList = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        Context.RemoveRange(entityList);
        return entityList.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteBulkAsync(
        IQuerySpecification<TEntity, TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, Context);
        ArgumentNullException.ThrowIfNull(specification);

        var query = ApplyEntitySpecification(specification);
        return await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the specification to build a projected query.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="specification">The query specification to apply.</param>
    /// <returns>A queryable with the specification applied and projected to the result type.</returns>
    private IQueryable<TResult> ApplySpecification<TResult>(
        IQuerySpecification<TEntity, TResult> specification)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        // Apply tracking behavior
        query = specification.AsTracking
            ? query.AsTracking()
            : query.AsNoTracking();

        // Apply includes
        query = ApplyIncludes(query, specification.Includes);

        // Apply predicate
        if (specification.Predicate is not null)
        {
            query = query.Where(specification.Predicate);
        }

        // Apply ordering
        query = ApplyOrdering(query, specification.OrderBy);

        // Apply distinct (before skip/take)
        if (specification.IsDistinct)
        {
            query = query.Distinct();
        }

        // Apply skip
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        // Apply take
        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        // Apply projection
        return query.Select(specification.Selector);
    }

    /// <summary>
    /// Applies the specification to build an entity query (without projection).
    /// Used for update and delete operations.
    /// </summary>
    /// <param name="specification">The query specification to apply.</param>
    /// <returns>A queryable with the specification applied.</returns>
    private IQueryable<TEntity> ApplyEntitySpecification(
        IQuerySpecification<TEntity, TEntity> specification)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        // Apply includes
        query = ApplyIncludes(query, specification.Includes);

        // Apply predicate
        if (specification.Predicate is not null)
        {
            query = query.Where(specification.Predicate);
        }

        // Apply ordering
        query = ApplyOrdering(query, specification.OrderBy);

        // Apply skip
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        // Apply take
        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies include specifications to a query using string-based navigation paths.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="includes">The collection of include specifications.</param>
    /// <returns>The query with includes applied.</returns>
    private static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        IReadOnlyList<IIncludeSpecification<TEntity>> includes)
    {
        if (includes.Count == 0)
        {
            return query;
        }

        foreach (var include in includes)
        {
            // Extract the navigation path from the expression
            var navigationPath = GetNavigationPath(include.IncludeExpression);
            query = query.Include(navigationPath);

            // Apply ThenIncludes by building the full path
            foreach (var thenInclude in include.ThenIncludes)
            {
                var thenPath = $"{navigationPath}.{GetNavigationPath(thenInclude.ThenIncludeExpression)}";
                query = query.Include(thenPath);
            }
        }

        return query;
    }

    /// <summary>
    /// Extracts the navigation property path from a lambda expression.
    /// </summary>
    /// <param name="expression">The lambda expression representing the navigation property.</param>
    /// <returns>The dot-separated navigation path string.</returns>
    private static string GetNavigationPath(LambdaExpression expression)
    {
        var path = new System.Text.StringBuilder();
        var current = expression.Body;

        // Handle Convert expressions (for value types or interfaces)
        while (current is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            current = unary.Operand;
        }

        // Build the path from member access chain
        while (current is MemberExpression member)
        {
            if (path.Length > 0)
            {
                path.Insert(0, '.');
            }

            path.Insert(0, member.Member.Name);
            current = member.Expression!;

            // Handle Convert expressions in the chain
            while (current is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                current = unary.Operand;
            }
        }

        return path.ToString();
    }

    /// <summary>
    /// Applies ordering specifications to a query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="orderSpecs">The collection of order specifications.</param>
    /// <returns>The query with ordering applied.</returns>
    private static IQueryable<TEntity> ApplyOrdering(
        IQueryable<TEntity> query,
        IReadOnlyList<IOrderSpecification<TEntity>> orderSpecs)
    {
        if (orderSpecs.Count == 0)
        {
            return query;
        }

        // Apply first ordering
        var orderedQuery = orderSpecs[0].ApplyFirst(query);

        // Apply subsequent orderings
        for (int i = 1; i < orderSpecs.Count; i++)
        {
            orderedQuery = orderSpecs[i].ApplySubsequent(orderedQuery);
        }

        return orderedQuery;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases resources used by the object, optionally performing a full disposal based on the
    /// specified flag.
    /// </summary>
    /// <remarks>This method does not dispose the associated DbContext. The DbContext should be managed by the
    /// UnitOfWork or dependency injection container. Override this method to dispose additional resources as
    /// needed.</remarks>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    protected virtual ValueTask DisposeAsync(bool disposing)
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
    /// <returns>A task that represents the asynchronous save operation with the number of affected entities.</returns>
    private async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.State = EntityState.Detached;
                    Context.Add(entry.Entity);
                }
            }

            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Applies a property update to an entity instance.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="update">The property update to apply.</param>
    private static void ApplyPropertyUpdate(
        TEntity entity,
        IEntityPropertyUpdate<TEntity> update)
    {
        // Get the property from the expression
        if (update.PropertyExpression.Body is MemberExpression memberExpression)
        {
            var property = memberExpression.Member as Reflection.PropertyInfo;
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
