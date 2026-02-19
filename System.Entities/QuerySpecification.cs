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
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace System.Entities;

/// <summary>
/// Provides factory methods for creating query specifications using a fluent builder pattern.
/// <example>
/// <code>
/// var spec = QuerySpecification
///     .For&lt;Product&gt;()
///     .Where(p =&gt; p.IsActive)
///     .OrderBy(p =&gt; p.Name)
///     .Select(p =&gt; new ProductDto(p.Id, p.Name));
///     
/// var products = await repository.FetchAsync(spec, cancellationToken);
/// </code>
/// </example>
/// </summary>
/// <remarks>
/// <para>Query specifications provide a type-safe way to define queries.</para>
/// </remarks>
public static class QuerySpecification
{
    /// <summary>
    /// Creates a new query specification builder for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to query.</typeparam>
    /// <returns>A new specification builder instance.</returns>
    public static QuerySpecificationBuilder<TEntity> For<TEntity>()
        where TEntity : class
        => new();
}

/// <summary>
/// A fluent builder for constructing query specifications.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query.</typeparam>
public readonly record struct QuerySpecificationBuilder<TEntity>
    where TEntity : class
{
    private readonly Expression<Func<TEntity, bool>>? _predicate;
    private readonly ImmutableList<IIncludeSpecification<TEntity>> _includes;
    private readonly ImmutableList<IOrderSpecification<TEntity>> _orderBy;
    private readonly int? _skip;
    private readonly int? _take;
    private readonly bool _asTracking;
    private readonly bool _isDistinct;

    internal QuerySpecificationBuilder(
        Expression<Func<TEntity, bool>>? predicate = null,
        ImmutableList<IIncludeSpecification<TEntity>>? includes = null,
        ImmutableList<IOrderSpecification<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool asTracking = false,
        bool isDistinct = false)
    {
        _predicate = predicate;
        _includes = includes ?? [];
        _orderBy = orderBy ?? [];
        _skip = skip;
        _take = take;
        _asTracking = asTracking;
        _isDistinct = isDistinct;
    }

    /// <summary>
    /// Adds a predicate to filter entities.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>A new builder with the predicate applied.</returns>
    public QuerySpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

		Expression<Func<TEntity, bool>> combined = _predicate is null
            ? predicate
            : CombinePredicates(_predicate, predicate);

        return new(combined, _includes, _orderBy, _skip, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Adds a navigation property to eagerly load.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="includeExpression">The expression selecting the navigation property.</param>
    /// <returns>A new builder with the include applied.</returns>
    public QuerySpecificationBuilder<TEntity> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(includeExpression);

        var include = new IncludeSpecification<TEntity, TProperty>(includeExpression);
        return new(_predicate, _includes.Add(include), _orderBy, _skip, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Adds a navigation property to eagerly load with nested includes.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="includeExpression">The expression selecting the navigation property.</param>
    /// <param name="thenIncludeBuilder">A function to configure nested includes.</param>
    /// <returns>A new builder with the include applied.</returns>
    public QuerySpecificationBuilder<TEntity> Include<TProperty>(
        Expression<Func<TEntity, TProperty>> includeExpression,
        Func<ThenIncludeBuilder<TEntity, TProperty>, ThenIncludeBuilder<TEntity, TProperty>> thenIncludeBuilder)
    {
        ArgumentNullException.ThrowIfNull(includeExpression);
        ArgumentNullException.ThrowIfNull(thenIncludeBuilder);

        var builder = new ThenIncludeBuilder<TEntity, TProperty>();
        builder = thenIncludeBuilder(builder);

        var include = new IncludeSpecification<TEntity, TProperty>(includeExpression, builder.Build());
        return new(_predicate, _includes.Add(include), _orderBy, _skip, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Adds ascending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public QuerySpecificationBuilder<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification<TEntity, TKey>(keySelector, Descending: false);
        return new(_predicate, _includes, _orderBy.Add(order), _skip, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Adds descending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public QuerySpecificationBuilder<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification<TEntity, TKey>(keySelector, Descending: true);
        return new(_predicate, _includes, _orderBy.Add(order), _skip, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Adds additional ascending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public QuerySpecificationBuilder<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => OrderBy(keySelector);

    /// <summary>
    /// Adds additional descending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public QuerySpecificationBuilder<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => OrderByDescending(keySelector);

    /// <summary>
    /// Skips the specified number of entities.
    /// </summary>
    /// <param name="count">The number of entities to skip.</param>
    /// <returns>A new builder with skip applied.</returns>
    public QuerySpecificationBuilder<TEntity> Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new(_predicate, _includes, _orderBy, count, _take, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Takes the specified number of entities.
    /// </summary>
    /// <param name="count">The maximum number of entities to return.</param>
    /// <returns>A new builder with take applied.</returns>
    public QuerySpecificationBuilder<TEntity> Take(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new(_predicate, _includes, _orderBy, _skip, count, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Applies paging with the specified page number and page size.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new builder with paging applied.</returns>
    public QuerySpecificationBuilder<TEntity> Page(int pageIndex, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return new(_predicate, _includes, _orderBy, pageIndex * pageSize, pageSize, _asTracking, _isDistinct);
    }

    /// <summary>
    /// Enables change tracking for the queried entities.
    /// </summary>
    /// <returns>A new builder with tracking enabled.</returns>
    public QuerySpecificationBuilder<TEntity> WithTracking()
        => new(_predicate, _includes, _orderBy, _skip, _take, asTracking: true, _isDistinct);

    /// <summary>
    /// Applies distinct to the result set.
    /// </summary>
    /// <returns>A new builder with distinct applied.</returns>
    public QuerySpecificationBuilder<TEntity> Distinct()
        => new(_predicate, _includes, _orderBy, _skip, _take, _asTracking, isDistinct: true);

    /// <summary>
    /// Projects the entities to the specified result type and builds the specification.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="selector">The projection expression.</param>
    /// <returns>A completed query specification.</returns>
    public QuerySpecification<TEntity, TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new(
            _predicate,
            selector,
            _includes,
            _orderBy,
            _skip,
            _take,
            _asTracking,
            _isDistinct);
    }

    /// <summary>
    /// Builds the specification returning the entity itself (identity projection).
    /// </summary>
    /// <returns>A completed query specification.</returns>
    public QuerySpecification<TEntity, TEntity> Build()
        => Select(e => e);

    private static Expression<Func<TEntity, bool>> CombinePredicates(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
		ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

		Expression leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
		Expression rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

		BinaryExpression combined = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
        => new ParameterReplacer(oldParam, newParam).Visit(expression);

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}

/// <summary>
/// A fluent builder for configuring nested includes (ThenInclude).
/// </summary>
/// <typeparam name="TEntity">The type of the root entity.</typeparam>
/// <typeparam name="TPreviousProperty">The type of the previous navigation property.</typeparam>
public readonly record struct ThenIncludeBuilder<TEntity, TPreviousProperty>
    where TEntity : class
{
    private readonly ImmutableList<IThenIncludeSpecification> _thenIncludes;

    internal ThenIncludeBuilder(ImmutableList<IThenIncludeSpecification>? thenIncludes = null)
    {
        _thenIncludes = thenIncludes ?? [];
    }

    /// <summary>
    /// Adds a nested navigation property to eagerly load.
    /// </summary>
    /// <typeparam name="TProperty">The type of the nested navigation property.</typeparam>
    /// <param name="thenIncludeExpression">The expression selecting the nested navigation property.</param>
    /// <returns>A new builder with the nested include applied.</returns>
    public ThenIncludeBuilder<TEntity, TProperty> ThenInclude<TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
    {
        ArgumentNullException.ThrowIfNull(thenIncludeExpression);

        var thenInclude = new ThenIncludeSpecification<TPreviousProperty, TProperty>(thenIncludeExpression);
        return new(_thenIncludes.Add(thenInclude));
    }

    internal ImmutableList<IThenIncludeSpecification> Build() => _thenIncludes;
}

/// <summary>
/// An immutable query specification that defines how to query entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query.</typeparam>
/// <typeparam name="TResult">The type of the projected result.</typeparam>
public readonly record struct QuerySpecification<TEntity, TResult> : IQuerySpecification<TEntity, TResult>
    where TEntity : class
{
    /// <inheritdoc />
    public Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <inheritdoc />
    public Expression<Func<TEntity, TResult>> Selector { get; }

    /// <inheritdoc />
    public IReadOnlyList<IIncludeSpecification<TEntity>> Includes { get; }

    /// <inheritdoc />
    public IReadOnlyList<IOrderSpecification<TEntity>> OrderBy { get; }

    /// <inheritdoc />
    public int? Skip { get; }

    /// <inheritdoc />
    public int? Take { get; }

    /// <inheritdoc />
    public bool AsTracking { get; }

    /// <inheritdoc />
    public bool IsDistinct { get; }

    internal QuerySpecification(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TResult>> selector,
        IReadOnlyList<IIncludeSpecification<TEntity>> includes,
        IReadOnlyList<IOrderSpecification<TEntity>> orderBy,
        int? skip,
        int? take,
        bool asTracking,
        bool isDistinct)
    {
        Predicate = predicate;
        Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        Includes = includes ?? [];
        OrderBy = orderBy ?? [];
        Skip = skip;
        Take = take;
        AsTracking = asTracking;
        IsDistinct = isDistinct;
    }
}

/// <summary>
/// Represents an include specification for a specific navigation property.
/// </summary>
internal sealed record IncludeSpecification<TEntity, TProperty> : IIncludeSpecification<TEntity>
    where TEntity : class
{
    public LambdaExpression IncludeExpression { get; }
    public IReadOnlyList<IThenIncludeSpecification> ThenIncludes { get; }

    internal IncludeSpecification(
        Expression<Func<TEntity, TProperty>> includeExpression,
        IReadOnlyList<IThenIncludeSpecification>? thenIncludes = null)
    {
        IncludeExpression = includeExpression;
        ThenIncludes = thenIncludes ?? [];
    }
}

/// <summary>
/// Represents a then-include specification for a nested navigation property.
/// </summary>
internal sealed record ThenIncludeSpecification<TPreviousProperty, TProperty> : IThenIncludeSpecification
{
    public LambdaExpression ThenIncludeExpression { get; }

    internal ThenIncludeSpecification(Expression<Func<TPreviousProperty, TProperty>> expression)
    {
        ThenIncludeExpression = expression;
    }
}

/// <summary>
/// Represents an ordering specification for a specific property.
/// </summary>
internal sealed record OrderSpecification<TEntity, TKey>(
    Expression<Func<TEntity, TKey>> KeySelector,
    bool Descending) : IOrderSpecification<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public IOrderedQueryable<TEntity> ApplyFirst(IQueryable<TEntity> query) =>
        Descending
            ? query.OrderByDescending(KeySelector)
            : query.OrderBy(KeySelector);

    /// <inheritdoc />
    public IOrderedQueryable<TEntity> ApplySubsequent(IOrderedQueryable<TEntity> query) =>
        Descending
            ? query.ThenByDescending(KeySelector)
            : query.ThenBy(KeySelector);
}
