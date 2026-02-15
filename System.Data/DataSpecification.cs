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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace System.Data;

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
public static class DataSpecification
{
    /// <summary>
    /// Creates a new query specification builder for the specified entity type.
    /// </summary>
    /// <typeparam name="TData">The type of data to query.</typeparam>
    /// <returns>A new specification builder instance.</returns>
    public static DataSpecificationBuilder<TData> For<TData>()
        where TData : class
        => new(
            predicate: null,
            joins: [],
            groupBy: [],
            having: null,
            orderBy: [],
            skip: null,
            take: null,
            isDistinct: false);
}

/// <summary>
/// A fluent builder for constructing query specifications.
/// </summary>
/// <typeparam name="TData">The type of entity to query.</typeparam>
public readonly record struct DataSpecificationBuilder<TData>
    where TData : class
{
    private readonly LambdaExpression? _predicate;
    private readonly ImmutableArray<IJoinSpecification> _joins;
    private readonly ImmutableArray<LambdaExpression> _groupBy;
    private readonly LambdaExpression? _having;
    private readonly ImmutableArray<OrderSpecification> _orderBy;
    private readonly int? _skip;
    private readonly int? _take;
    private readonly bool _isDistinct;

    internal DataSpecificationBuilder(
        LambdaExpression? predicate,
        ImmutableArray<IJoinSpecification> joins,
        ImmutableArray<LambdaExpression> groupBy,
        LambdaExpression? having,
        ImmutableArray<OrderSpecification> orderBy,
        int? skip,
        int? take,
        bool isDistinct)
    {
        _predicate = predicate;
        _joins = joins;
        _groupBy = groupBy;
        _having = having;
        _orderBy = orderBy;
        _skip = skip;
        _take = take;
        _isDistinct = isDistinct;
    }

    /// <summary>
    /// Adds a predicate to filter data.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>A new builder with the predicate applied.</returns>
    public DataSpecificationBuilder<TData> Where(Expression<Func<TData, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var combined = _predicate is null
            ? predicate
            : CombinePredicates((Expression<Func<TData, bool>>)_predicate, predicate);

        return new(combined, _joins, _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds an inner join to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> InnerJoin<TJoin>(
        Expression<Func<TData, TJoin, bool>> onExpression,
        string? tableAlias = null)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(onExpression);
        var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Inner, onExpression, tableAlias);
        return new(_predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a left join to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> LeftJoin<TJoin>(
        Expression<Func<TData, TJoin, bool>> onExpression,
        string? tableAlias = null)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(onExpression);
        var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Left, onExpression, tableAlias);
        return new(_predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a right join to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> RightJoin<TJoin>(
        Expression<Func<TData, TJoin, bool>> onExpression,
        string? tableAlias = null)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(onExpression);
        var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Right, onExpression, tableAlias);
        return new(_predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a full outer join to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> FullJoin<TJoin>(
        Expression<Func<TData, TJoin, bool>> onExpression,
        string? tableAlias = null)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(onExpression);
        var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Full, onExpression, tableAlias);
        return new(_predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a cross join to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> CrossJoin<TJoin>(string? tableAlias = null)
        where TJoin : class
    {
        var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Cross, null, tableAlias);
        return new(_predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds ascending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public DataSpecificationBuilder<TData> OrderBy<TKey>(Expression<Func<TData, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification(keySelector, Descending: false);
        return new(_predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds ascending ordering by the specified property from a joined entity.
    /// </summary>
    public DataSpecificationBuilder<TData> OrderBy<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification(keySelector, Descending: false);
        return new(_predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds descending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public DataSpecificationBuilder<TData> OrderByDescending<TKey>(Expression<Func<TData, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification(keySelector, Descending: true);
        return new(_predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds descending ordering by the specified property from a joined entity.
    /// </summary>
    public DataSpecificationBuilder<TData> OrderByDescending<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        var order = new OrderSpecification(keySelector, Descending: true);
        return new(_predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds additional ascending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public DataSpecificationBuilder<TData> ThenBy<TKey>(Expression<Func<TData, TKey>> keySelector)
        => OrderBy(keySelector);

    /// <summary>
    /// Adds additional descending ordering by the specified property.
    /// </summary>
    /// <typeparam name="TKey">The type of the ordering key.</typeparam>
    /// <param name="keySelector">The expression selecting the property to order by.</param>
    /// <returns>A new builder with the ordering applied.</returns>
    public DataSpecificationBuilder<TData> ThenByDescending<TKey>(Expression<Func<TData, TKey>> keySelector)
        => OrderByDescending(keySelector);

    /// <summary>
    /// Skips the specified number of data.
    /// </summary>
    /// <param name="count">The number of entities to skip.</param>
    /// <returns>A new builder with skip applied.</returns>
    public DataSpecificationBuilder<TData> Skip(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new(_predicate, _joins, _groupBy, _having, _orderBy, count, _take, _isDistinct);
    }

    /// <summary>
    /// Takes the specified number of data.
    /// </summary>
    /// <param name="count">The maximum number of entities to return.</param>
    /// <returns>A new builder with take applied.</returns>
    public DataSpecificationBuilder<TData> Take(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return new(_predicate, _joins, _groupBy, _having, _orderBy, _skip, count, _isDistinct);
    }

    /// <summary>
    /// Applies paging with the specified page number and page size.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A new builder with paging applied.</returns>
    public DataSpecificationBuilder<TData> Page(int pageIndex, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return new(_predicate, _joins, _groupBy, _having, _orderBy, pageIndex * pageSize, pageSize, _isDistinct);
    }

    /// <summary>
    /// Applies distinct to the result set.
    /// </summary>
    /// <returns>A new builder with distinct applied.</returns>
    public DataSpecificationBuilder<TData> Distinct()
        => new(_predicate, _joins, _groupBy, _having, _orderBy, _skip, _take, isDistinct: true);

    /// <summary>
    /// Adds a grouping expression to the query.
    /// </summary>
    public DataSpecificationBuilder<TData> GroupBy<TKey>(Expression<Func<TData, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(_predicate, _joins, _groupBy.Add(keySelector), _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a grouping expression to the query with a joined entity.
    /// </summary>
    public DataSpecificationBuilder<TData> GroupBy<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(_predicate, _joins, _groupBy.Add(keySelector), _having, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a HAVING predicate applied after grouping.
    /// </summary>
    public DataSpecificationBuilder<TData> Having(Expression<Func<TData, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(_predicate, _joins, _groupBy, predicate, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Adds a HAVING predicate applied after grouping with a joined entity.
    /// </summary>
    public DataSpecificationBuilder<TData> Having<TJoin>(Expression<Func<TData, TJoin, bool>> predicate)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(_predicate, _joins, _groupBy, predicate, _orderBy, _skip, _take, _isDistinct);
    }

    /// <summary>
    /// Projects the entities to the specified result type and builds the specification.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected result.</typeparam>
    /// <param name="selector">The projection expression.</param>
    /// <returns>A completed query specification.</returns>
    public DataSpecification<TData, TResult> Select<TResult>(Expression<Func<TData, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new(
            _predicate,
            selector,
            _joins,
            _groupBy,
            _having,
            _orderBy,
            _skip,
            _take,
            _isDistinct);
    }

    /// <summary>
    /// Projects the entities and a joined entity to the specified result type.
    /// </summary>
    public DataSpecification<TData, TResult> Select<TJoin, TResult>(Expression<Func<TData, TJoin, TResult>> selector)
        where TJoin : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new(
            _predicate,
            selector,
            _joins,
            _groupBy,
            _having,
            _orderBy,
            _skip,
            _take,
            _isDistinct);
    }

    /// <summary>
    /// Projects the entities and joined entities to the specified result type.
    /// </summary>
    public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TResult>(
        Expression<Func<TData, TJoin1, TJoin2, TResult>> selector)
        where TJoin1 : class
        where TJoin2 : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new(
            _predicate,
            selector,
            _joins,
            _groupBy,
            _having,
            _orderBy,
            _skip,
            _take,
            _isDistinct);
    }

    /// <summary>
    /// Projects the entities and joined entities to the specified result type.
    /// </summary>
    public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TResult>(
        Expression<Func<TData, TJoin1, TJoin2, TJoin3, TResult>> selector)
        where TJoin1 : class
        where TJoin2 : class
        where TJoin3 : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        return new(
            _predicate,
            selector,
            _joins,
            _groupBy,
            _having,
            _orderBy,
            _skip,
            _take,
            _isDistinct);
    }

    // Cached per closed generic type — ensures ReferenceEqualityComparer hits in DataSqlMapper._compiledSelectors
    private static readonly Expression<Func<TData, TData>> _identitySelector = static e => e;

    /// <summary>
    /// Builds the specification returning the entity itself (identity projection).
    /// </summary>
    /// <returns>A completed query specification.</returns>
    public DataSpecification<TData, TData> Build()
        => Select(_identitySelector);

    private static Expression<Func<TData, bool>> CombinePredicates(
        Expression<Func<TData, bool>> left,
        Expression<Func<TData, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(TData), "x");

        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

        var combined = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<TData, bool>>(combined, parameter);
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
/// An immutable query specification that defines how to query entities.
/// </summary>
/// <typeparam name="TData">The type of data to query.</typeparam>
/// <typeparam name="TResult">The type of the projected result.</typeparam>
public readonly record struct DataSpecification<TData, TResult> : IDataSpecification<TData, TResult>
    where TData : class
{
    /// <inheritdoc />
    public LambdaExpression? Predicate { get; }

    /// <inheritdoc />
    public LambdaExpression Selector { get; }

    /// <inheritdoc />
    public IReadOnlyList<IJoinSpecification> Joins { get; }

    /// <inheritdoc />
    public IReadOnlyList<LambdaExpression> GroupBy { get; }

    /// <inheritdoc />
    public LambdaExpression? Having { get; }

    /// <inheritdoc />
    public IReadOnlyList<OrderSpecification> OrderBy { get; }

    /// <inheritdoc />
    public int? Skip { get; }

    /// <inheritdoc />
    public int? Take { get; }

    /// <inheritdoc />
    public bool IsDistinct { get; }

    internal DataSpecification(
        LambdaExpression? predicate,
        LambdaExpression selector,
        ImmutableArray<IJoinSpecification> joins,
        ImmutableArray<LambdaExpression> groupBy,
        LambdaExpression? having,
        ImmutableArray<OrderSpecification> orderBy,
        int? skip,
        int? take,
        bool isDistinct)
    {
        Predicate = predicate;
        Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        Joins = joins;
        GroupBy = groupBy;
        Having = having;
        OrderBy = orderBy;
        Skip = skip;
        Take = take;
        IsDistinct = isDistinct;
    }
}

/// <summary>
/// Represents a join specification for a SQL query.
/// </summary>
internal sealed record JoinSpecification<TLeft, TRight>(
    SqlJoinType JoinType,
    LambdaExpression? OnExpression,
    string? TableAlias) : IJoinSpecification
    where TLeft : class
    where TRight : class
{
    public Type LeftType => typeof(TLeft);
    public Type RightType => typeof(TRight);
}
