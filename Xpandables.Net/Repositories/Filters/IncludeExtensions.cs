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

using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Provides extension methods for including related entities in queries.
/// These methods create deferred include operations that are resolved when the filter is applied.
/// </summary>
public static class IncludeExtensions
{
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
    /// <returns>A wrapped queryable that will apply the ThenInclude when resolved.</returns>
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
    /// <returns>A wrapped queryable that will apply the ThenInclude when resolved.</returns>
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