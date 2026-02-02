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

namespace System.Data;

/// <summary>
/// Defines a specification for querying data with type-safe, expression-based 
/// filtering, projection, sorting, and paging.
/// </summary>
/// <typeparam name="TData">The type of data to query.</typeparam>
/// <typeparam name="TResult">The type of the projected result.</typeparam>
/// <remarks>
/// <para>Use <see cref="DataSpecification.For{TEntity}"/> to create specifications using a fluent builder pattern.</para>
/// </remarks>
public interface IDataSpecification<TData, TResult>
    where TData : class
{
    /// <summary>
    /// Gets the predicate expression to filter data.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no filtering is applied.</remarks>
    Expression<Func<TData, bool>>? Predicate { get; }

    /// <summary>
    /// Gets the projection expression to transform data into results.
    /// </summary>
    Expression<Func<TData, TResult>> Selector { get; }

    /// <summary>
    /// Gets the collection of navigation properties to eagerly load.
    /// </summary>
    IReadOnlyList<IIncludeSpecification<TData>> Includes { get; }

    /// <summary>
    /// Gets the collection of ordering specifications.
    /// </summary>
    IReadOnlyList<IOrderSpecification<TData>> OrderBy { get; }

    /// <summary>
    /// Gets the number of data to skip.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no skipping is applied.</remarks>
    int? Skip { get; }

    /// <summary>
    /// Gets the maximum number of data to return.
    /// </summary>
    /// <remarks>When <see langword="null"/>, no limit is applied.</remarks>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether to apply distinct to the result.
    /// </summary>
    bool IsDistinct { get; }
}

/// <summary>
/// Represents an include specification for eager loading navigation properties.
/// </summary>
/// <typeparam name="TData">The type of the data.</typeparam>
public interface IIncludeSpecification<TData>
    where TData : class
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
/// <typeparam name="TData">The type of the data.</typeparam>
public interface IOrderSpecification<TData>
    where TData : class
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
    IOrderedQueryable<TData> ApplyFirst(IQueryable<TData> query);

    /// <summary>
    /// Applies this ordering as a secondary sort to an already ordered query.
    /// </summary>
    /// <param name="query">The already ordered query.</param>
    /// <returns>An ordered query with this additional ordering applied.</returns>
    IOrderedQueryable<TData> ApplySubsequent(IOrderedQueryable<TData> query);
}
