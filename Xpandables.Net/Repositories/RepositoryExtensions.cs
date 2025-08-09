
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Xpandables.Net.Events;

using Xpandables.Net.Repositories.Converters;

using Xpandables.Net.Text;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides extension methods for repository operations.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>  
    /// Determines whether the specified entity is active.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is active; otherwise, <c>false</c>.</returns>  
    public static bool IsActive(this IEntity entity) =>
        entity.Status == EntityStatus.ACTIVE;
    /// <summary>  
    /// Determines whether the specified entity is deleted.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is deleted; otherwise, <c>false</c>.</returns>  
    public static bool IsDeleted(this IEntity entity) =>
        entity.Status == EntityStatus.DELETED;

    /// <summary>  
    /// Determines whether the specified entity is pending.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is pending; otherwise, <c>false</c>.</returns>  
    public static bool IsPending(this IEntity entity) =>
        entity.Status == EntityStatus.PENDING;

    /// <summary>  
    /// Determines whether the specified entity is suspended.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is suspended; otherwise, <c>false</c>.</returns>  
    public static bool IsSuspended(this IEntity entity) =>
        entity.Status == EntityStatus.SUSPENDED;

    /// <summary>
    /// Determines whether the specified entity is published.
    /// </summary>
    /// <param name="entity">The entity to check. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the entity's status is <see cref="EntityStatus.PUBLISHED"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool IsPublished(this IEntity entity) =>
        entity.Status == EntityStatus.PUBLISHED;

    /// <summary>
    /// Determines whether the specified entity is in an error state.
    /// </summary>
    /// <param name="entity">The entity to evaluate. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the entity's status is <see cref="EntityStatus.ONERROR"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool IsOnError(this IEntity entity) =>
        entity.Status == EntityStatus.ONERROR;

    /// <summary>
    /// Combines two expressions into a single expression.
    /// </summary>
    /// <typeparam name="TFirstParam">The type of the first parameter.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="first">The first expression.</param>
    /// <param name="second">The second expression.</param>
    /// <returns>A combined expression.</returns>
    public static Expression<Func<TFirstParam, TResult>> Combine
        <TFirstParam, TIntermediate, TResult>(
        this Expression<Func<TFirstParam, TIntermediate>> first,
        Expression<Func<TFirstParam, TIntermediate, TResult>> second)
    {
        ParameterExpression param = Expression
            .Parameter(typeof(TFirstParam), "param");

        Expression newFirst = first.Body
            .Replace(first.Parameters[0], param);

        Expression newSecond = second.Body
            .Replace(second.Parameters[0], param)
            .Replace(second.Parameters[1], newFirst);

        return Expression
            .Lambda<Func<TFirstParam, TResult>>(newSecond, param);
    }

    /// <summary>
    /// Composes two expressions into a single expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source parameter.</typeparam>
    /// <typeparam name="TCompose">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="source">The source expression.</param>
    /// <param name="compose">The compose expression.</param>
    /// <returns>A composed expression.</returns>
    public static Expression<Func<TSource, TResult>> Compose
        <TSource, TCompose, TResult>(
        this Expression<Func<TSource, TCompose>> source,
        Expression<Func<TCompose, TResult>> compose)
        where TCompose : notnull
    {
        ParameterExpression param = Expression.Parameter(typeof(TSource), null);
        InvocationExpression invoke = Expression.Invoke(source, param);
        InvocationExpression result = Expression.Invoke(compose, invoke);

        return Expression.Lambda<Func<TSource, TResult>>(result, param);
    }

    /// <summary>  
    /// Filters a sequence of values based on a predicate applied to a property 
    /// of the elements.  
    /// </summary>  
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TParam">The type of the property to filter.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to filter.</param>
    /// <param name="propertyExpression">The property expression to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the 
    /// input sequence that satisfy the condition.</returns>
    public static IQueryable<TSource> Where<TSource, TParam>(
       this IQueryable<TSource> source,
       Expression<Func<TSource, TParam>> propertyExpression,
       Expression<Func<TParam, bool>> predicate)
       where TParam : notnull =>
       source.Where(propertyExpression.Compose(predicate));

    private static Expression Replace(
           this Expression expression,
           Expression searchEx,
           Expression replaceEx)
           => new ReplaceVisitor(searchEx, replaceEx).Visit(expression);

    private sealed class ReplaceVisitor(Expression from, Expression to) :
        ExpressionVisitor
    {
        private readonly Expression from = from, to = to;

        public override Expression Visit(Expression? node)
            => node == from ? to : base.Visit(node)!;
    }

    /// <summary>
    /// Deserializes a stream of event entity projections into concrete <see cref="IEvent"/> objects.
    /// This method should be called after the query has been executed and is being streamed from the database.
    /// </summary>
    /// <remarks>This method also disposes of the <see cref="IEntityEvent"/> instances after deserialization to free resources.</remarks>
    /// <param name="source">The asynchronous stream of event projections.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of deserialized <see cref="IEvent"/> objects.</returns>
    public static async IAsyncEnumerable<IEvent> AsEventsAsync(
        this IAsyncEnumerable<IEntityEvent> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ConcurrentDictionary<string, Type> eventTypeCache = [];

        await foreach (var entity in source.WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            Type concreteEventType = eventTypeCache.GetOrAdd(entity.FullName, fullName =>
            {
                Type? eventType = Type.GetType(fullName);
                return eventType ?? throw new InvalidOperationException(
                    $"The event type '{fullName}' could not be found. " +
                    $"Ensure it is referenced and available at runtime.");
            });

            IEvent deserializedEvent = EventConverter.DeserializeEvent(
                entity.Data,
                concreteEventType,
                DefaultSerializerOptions.Defaults);

            entity.Dispose();

            yield return deserializedEvent;
        }
    }

    /// <summary>
    /// Creates an include operation for a navigation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property to include.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="navigationPropertyPath">Expression specifying the navigation property to include.</param>
    /// <returns>A wrapped queryable that will apply the include when resolved.</returns>
    public static IQueryable<TEntity> Include<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        return new DeferredIncludeQueryable<TEntity>(source, new IncludeOperation
        {
            Type = IncludeOperationType.Include,
            Expression = navigationPropertyPath
        });
    }

    /// <summary>
    /// Creates an include operation for a collection navigation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the collection elements.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="navigationPropertyPath">Expression specifying the collection navigation property to include.</param>
    /// <returns>A wrapped queryable that will apply the include when resolved.</returns>
    public static IQueryable<TEntity> Include<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, IEnumerable<TProperty>>> navigationPropertyPath)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        return new DeferredIncludeQueryable<TEntity>(source, new IncludeOperation
        {
            Type = IncludeOperationType.Include,
            Expression = navigationPropertyPath
        });
    }

    /// <summary>
    /// Creates a ThenInclude operation for a navigation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the previously included property.</typeparam>
    /// <typeparam name="TProperty">The type of the property to include next.</typeparam>
    /// <param name="source">The source queryable that already has an include.</param>
    /// <param name="navigationPropertyPath">Expression specifying the next navigation property to include.</param>
    /// <returns>A queryable that will apply the ThenInclude when resolved.</returns>
    public static IQueryable<TEntity> ThenInclude<TEntity, TPreviousProperty, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        return new DeferredIncludeQueryable<TEntity>(source, new IncludeOperation
        {
            Type = IncludeOperationType.ThenInclude,
            Expression = navigationPropertyPath,
            PreviousPropertyType = typeof(TPreviousProperty),
            PropertyType = typeof(TProperty)
        });
    }

    /// <summary>
    /// Creates a ThenInclude operation for a collection navigation property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the previously included property.</typeparam>
    /// <typeparam name="TProperty">The type of the collection elements.</typeparam>
    /// <param name="source">The source queryable that already has an include.</param>
    /// <param name="navigationPropertyPath">Expression specifying the collection navigation property to include.</param>
    /// <returns>A queryable that will apply the ThenInclude when resolved.</returns>
    public static IQueryable<TEntity> ThenInclude<TEntity, TPreviousProperty, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationPropertyPath)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(navigationPropertyPath);

        return new DeferredIncludeQueryable<TEntity>(source, new IncludeOperation
        {
            Type = IncludeOperationType.ThenInclude,
            Expression = navigationPropertyPath,
            PreviousPropertyType = typeof(TPreviousProperty),
            PropertyType = typeof(TProperty)
        });
    }

    /// <summary>
    /// Resolves all deferred include operations and applies them using Entity Framework Core methods.
    /// This method should be called in the context where EF Core is available.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="queryable">The queryable that may contain deferred includes.</param>
    /// <returns>The queryable with all includes resolved and applied.</returns>
    internal static IQueryable<TEntity> ResolveIncludes<TEntity>(this IQueryable<TEntity> queryable)
        where TEntity : class
    {
        if (queryable is not DeferredIncludeQueryable<TEntity> deferredQueryable)
            return queryable;

        var operations = deferredQueryable.GetAllOperations();
        var baseQuery = deferredQueryable.GetBaseQueryable();

        return ApplyIncludeOperations(baseQuery, operations);
    }

    private static IQueryable<TEntity> ApplyIncludeOperations<TEntity>(
        IQueryable<TEntity> baseQuery,
        IEnumerable<IncludeOperation> operations)
        where TEntity : class
    {
        var query = baseQuery;

        foreach (var operation in operations)
        {
            query = operation.Type switch
            {
                IncludeOperationType.Include => ApplyIncludeOperation(query, operation),
                IncludeOperationType.ThenInclude => ApplyThenIncludeOperation(query, operation),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<TEntity> ApplyIncludeOperation<TEntity>(
        IQueryable<TEntity> query,
        IncludeOperation operation)
        where TEntity : class
    {
        try
        {
            // Try to find and call EF Core's Include method
            var efCoreAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Microsoft.EntityFrameworkCore");

            if (efCoreAssembly is null)
                return query;

            var extensionsType = efCoreAssembly
                .GetType("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");

            if (extensionsType is null)
                return query;

            var includeMethod = extensionsType
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "Include" &&
                                   m.GetParameters().Length == 2 &&
                                   m.GetParameters()[1].ParameterType.IsGenericType &&
                                   m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

            if (includeMethod is null)
                return query;

            var propertyType = operation.Expression.ReturnType;
            var genericIncludeMethod = includeMethod.MakeGenericMethod(typeof(TEntity), propertyType);
            var result = genericIncludeMethod.Invoke(null, [query, operation.Expression]);

            return (IQueryable<TEntity>)result!;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            Trace.WriteLine($"Failed to apply Include operation: {exception.Message}");
            // If EF Core is not available or reflection fails, return original query
            return query;
        }
    }

    private static IQueryable<TEntity> ApplyThenIncludeOperation<TEntity>(
        IQueryable<TEntity> query,
        IncludeOperation operation)
        where TEntity : class
    {
        try
        {
            // Try to find and call EF Core's ThenInclude method
            var efCoreAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Microsoft.EntityFrameworkCore");

            if (efCoreAssembly is null)
                return query;

            var extensionsType = efCoreAssembly
                .GetType("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");

            if (extensionsType is null)
                return query;

            var thenIncludeMethod = extensionsType
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "ThenInclude" &&
                                   m.GetParameters().Length == 2 &&
                                   m.GetParameters()[1].ParameterType.IsGenericType &&
                                   m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>));

            if (thenIncludeMethod is null)
                return query;

            var genericThenIncludeMethod = thenIncludeMethod.MakeGenericMethod(
                typeof(TEntity),
                operation.PreviousPropertyType!,
                operation.PropertyType!);

            var result = genericThenIncludeMethod.Invoke(null, [query, operation.Expression]);

            return (IQueryable<TEntity>)result!;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            Trace.WriteLine($"Failed to apply ThenInclude operation: {exception.Message}");
            // If EF Core is not available or reflection fails, return original query
            return query;
        }
    }
}

/// <summary>
/// Represents the type of include operation to be performed.
/// </summary>
public enum IncludeOperationType
{
    /// <summary>
    /// Includes the specified item in the collection if it is not already present.
    /// </summary>
    Include,

    /// <summary>
    /// Specifies additional related data to be included in the query results.
    /// </summary>
    /// <remarks>This method is used in conjunction with the <see cref="Include"/> method to specify
    /// additional navigation properties for eager loading. It is typically used to include multiple levels of related
    /// data in a single query.</remarks>
    ThenInclude
}

/// <summary>
/// Represents a deferred include operation that will be resolved later.
/// </summary>
internal sealed class IncludeOperation
{
    public required IncludeOperationType Type { get; init; }
    public required LambdaExpression Expression { get; init; }
    public Type? PreviousPropertyType { get; init; }
    public Type? PropertyType { get; init; }
}

/// <summary>
/// A queryable wrapper that stores deferred include operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
internal sealed class DeferredIncludeQueryable<TEntity>(
    IQueryable<TEntity> baseQueryable,
    IncludeOperation operation) : IQueryable<TEntity>
    where TEntity : class
{
    private readonly IQueryable<TEntity> _baseQueryable = baseQueryable is DeferredIncludeQueryable<TEntity> deferred
            ? deferred.GetBaseQueryable()
            : baseQueryable;
    private readonly List<IncludeOperation> _operations = baseQueryable is DeferredIncludeQueryable<TEntity> existing
            ? [.. existing._operations, operation]
            : [operation];

    public IEnumerable<IncludeOperation> GetAllOperations() => _operations;
    public IQueryable<TEntity> GetBaseQueryable() => _baseQueryable;

    public Type ElementType => _baseQueryable.ElementType;
    public Expression Expression => _baseQueryable.Expression;
    public IQueryProvider Provider => _baseQueryable.Provider;

    public IEnumerator<TEntity> GetEnumerator() => _baseQueryable.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}