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
using System.Linq.Expressions;

namespace System.Entities;

/// <summary>
/// Defines a specification for querying entities with type-safe, expression-based 
/// filtering, projection, sorting, and paging.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query.</typeparam>
/// <typeparam name="TResult">The type of the projected result.</typeparam>
/// <remarks>
/// <para>Use <see cref="QuerySpecification.For{TEntity}"/> to create specifications using a fluent builder pattern.</para>
/// </remarks>
public interface IQuerySpecification<TEntity, TResult>
    where TEntity : class
{
    /// <summary>
    /// Gets the predicate expression to filter entities.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no filtering is applied.</remarks>
    Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <summary>
    /// Gets the projection expression to transform entities into results.
    /// </summary>
    Expression<Func<TEntity, TResult>> Selector { get; }

    /// <summary>
    /// Gets the collection of navigation properties to eagerly load.
    /// </summary>
    IReadOnlyList<IIncludeSpecification<TEntity>> Includes { get; }

    /// <summary>
    /// Gets the collection of ordering specifications.
    /// </summary>
    IReadOnlyList<IOrderSpecification<TEntity>> OrderBy { get; }

    /// <summary>
    /// Gets the number of entities to skip.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no skipping is applied.</remarks>
    int? Skip { get; }

    /// <summary>
    /// Gets the maximum number of entities to return.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no limit is applied.</remarks>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether to track the queried entities.
    /// </summary>
    /// <remarks>When <see langword="false"/> (default), entities are queried with no-tracking for better performance.</remarks>
    bool AsTracking { get; }

    /// <summary>
    /// Gets a value indicating whether to apply distinct to the result.
    /// </summary>
    bool IsDistinct { get; }
}

/// <summary>
/// Represents an include specification for eager loading navigation properties.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IIncludeSpecification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets the expression representing the navigation property to include.
    /// </summary>
    LambdaExpression IncludeExpression { get; }

    /// <summary>
    /// Gets the collection of then-include specifications for nested navigation properties.
    /// </summary>
    IReadOnlyList<IThenIncludeSpecification> ThenIncludes { get; }
}

/// <summary>
/// Represents a then-include specification for nested eager loading.
/// </summary>
public interface IThenIncludeSpecification
{
    /// <summary>
    /// Gets the expression representing the nested navigation property to include.
    /// </summary>
    LambdaExpression ThenIncludeExpression { get; }
}

/// <summary>
/// Represents an ordering specification that can apply itself to a query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IOrderSpecification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets a value indicating whether to order in descending order.
    /// </summary>
    bool Descending { get; }

    /// <summary>
    /// Applies this ordering as the primary sort to an unordered query.
    /// </summary>
    /// <param name="query">The unordered query.</param>
    /// <returns>An ordered query with this ordering applied.</returns>
    IOrderedQueryable<TEntity> ApplyFirst(IQueryable<TEntity> query);

    /// <summary>
    /// Applies this ordering as a secondary sort to an already ordered query.
    /// </summary>
    /// <param name="query">The already ordered query.</param>
    /// <returns>An ordered query with this additional ordering applied.</returns>
    IOrderedQueryable<TEntity> ApplySubsequent(IOrderedQueryable<TEntity> query);
}
