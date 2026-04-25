/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
			source: null,
			predicate: null,
			joins: [],
			groupBy: [],
			having: null,
			orderBy: [],
			skip: null,
			take: null,
			isDistinct: false);

	/// <summary>
	/// Creates a new query specification builder for the specified entity type,
	/// using the given source as the FROM clause instead of the default table name.
	/// </summary>
	/// <typeparam name="TData">The type of data to query.</typeparam>
	/// <param name="source">
	/// The table name, view name, or raw SQL subquery to use in the FROM clause.
	/// <para>
	/// <strong>SQL Injection Warning:</strong> This value is inserted into the generated SQL
	/// without parameterization. Callers must ensure the value is safe and does not originate from
	/// untrusted user input.
	/// </para>
	/// <para>
	/// <strong>Column Alignment:</strong> When using a raw SQL subquery, the subquery must select
	/// all columns required by the <typeparamref name="TData"/> entity type.
	/// </para>
	/// </param>
	/// <returns>A new specification builder instance.</returns>
	public static DataSpecificationBuilder<TData> For<TData>(string source)
		where TData : class
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(source);
		return new(
			source: source,
			predicate: null,
			joins: [],
			groupBy: [],
			having: null,
			orderBy: [],
			skip: null,
			take: null,
			isDistinct: false);
	}
}

/// <summary>
/// A fluent builder for constructing query specifications.
/// </summary>
/// <typeparam name="TData">The type of entity to query.</typeparam>
public readonly record struct DataSpecificationBuilder<TData>
	where TData : class
{
	private readonly string? _source;
	private readonly LambdaExpression? _predicate;
	private readonly ImmutableArray<IJoinSpecification> _joins;
	private readonly ImmutableArray<LambdaExpression> _groupBy;
	private readonly LambdaExpression? _having;
	private readonly ImmutableArray<OrderSpecification> _orderBy;
	private readonly int? _skip;
	private readonly int? _take;
	private readonly bool _isDistinct;

	internal DataSpecificationBuilder(
		string? source,
		LambdaExpression? predicate,
		ImmutableArray<IJoinSpecification> joins,
		ImmutableArray<LambdaExpression> groupBy,
		LambdaExpression? having,
		ImmutableArray<OrderSpecification> orderBy,
		int? skip,
		int? take,
		bool isDistinct)
	{
		_source = source;
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

		Expression<Func<TData, bool>> combined = _predicate is null
			? predicate
			: CombinePredicates((Expression<Func<TData, bool>>)_predicate, predicate);

		return new(_source, combined, _joins, _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds a cross join to the query.
	/// </summary>
	public DataSpecificationBuilder<TData> CrossJoin<TJoin>(string? tableAlias = null)
		where TJoin : class
	{
		var join = new JoinSpecification<TData, TJoin>(SqlJoinType.Cross, null, tableAlias);
		return new(_source, _predicate, _joins.Add(join), _groupBy, _having, _orderBy, _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds ascending ordering by the specified property from a joined entity.
	/// </summary>
	public DataSpecificationBuilder<TData> OrderBy<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
		where TJoin : class
	{
		ArgumentNullException.ThrowIfNull(keySelector);

		var order = new OrderSpecification(keySelector, Descending: false);
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds descending ordering by the specified property from a joined entity.
	/// </summary>
	public DataSpecificationBuilder<TData> OrderByDescending<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
		where TJoin : class
	{
		ArgumentNullException.ThrowIfNull(keySelector);

		var order = new OrderSpecification(keySelector, Descending: true);
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy.Add(order), _skip, _take, _isDistinct);
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
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy, count, _take, _isDistinct);
	}

	/// <summary>
	/// Takes the specified number of data.
	/// </summary>
	/// <param name="count">The maximum number of entities to return.</param>
	/// <returns>A new builder with take applied.</returns>
	public DataSpecificationBuilder<TData> Take(int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy, _skip, count, _isDistinct);
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

		return new(_source, _predicate, _joins, _groupBy, _having, _orderBy, pageIndex * pageSize, pageSize, _isDistinct);
	}

	/// <summary>
	/// Applies distinct to the result set.
	/// </summary>
	/// <returns>A new builder with distinct applied.</returns>
	public DataSpecificationBuilder<TData> Distinct()
		=> new(_source, _predicate, _joins, _groupBy, _having, _orderBy, _skip, _take, isDistinct: true);

	/// <summary>
	/// Adds a grouping expression to the query.
	/// </summary>
	/// <remarks>You may use the <see cref="SqlFunctions"/> class to apply aggregate functions in the grouping.</remarks>
	public DataSpecificationBuilder<TData> GroupBy<TKey>(Expression<Func<TData, TKey>> keySelector)
	{
		ArgumentNullException.ThrowIfNull(keySelector);
		return new(_source, _predicate, _joins, _groupBy.Add(keySelector), _having, _orderBy, _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds a grouping expression to the query with a joined entity.
	/// </summary>
	/// <remarks>You may use the <see cref="SqlFunctions"/> class to apply aggregate functions in the grouping.</remarks>
	public DataSpecificationBuilder<TData> GroupBy<TJoin, TKey>(Expression<Func<TData, TJoin, TKey>> keySelector)
		where TJoin : class
	{
		ArgumentNullException.ThrowIfNull(keySelector);
		return new(_source, _predicate, _joins, _groupBy.Add(keySelector), _having, _orderBy, _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds a HAVING predicate applied after grouping.
	/// </summary>
	/// <remarks>You may use the <see cref="SqlFunctions"/> class to apply aggregate functions in the HAVING clause.</remarks>
	public DataSpecificationBuilder<TData> Having(Expression<Func<TData, bool>> predicate)
	{
		ArgumentNullException.ThrowIfNull(predicate);
		return new(_source, _predicate, _joins, _groupBy, predicate, _orderBy, _skip, _take, _isDistinct);
	}

	/// <summary>
	/// Adds a HAVING predicate applied after grouping with a joined entity.
	/// </summary>
	/// <remarks>You may use the <see cref="SqlFunctions"/> class to apply aggregate functions in the HAVING clause.</remarks>
	public DataSpecificationBuilder<TData> Having<TJoin>(Expression<Func<TData, TJoin, bool>> predicate)
		where TJoin : class
	{
		ArgumentNullException.ThrowIfNull(predicate);
		return new(_source, _predicate, _joins, _groupBy, predicate, _orderBy, _skip, _take, _isDistinct);
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
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and a joined entity to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin, TResult>(Expression<Func<TData, TJoin, TResult>> selector)
		where TJoin : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
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
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
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
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
		where TJoin6 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
		where TJoin6 : class
		where TJoin7 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
		where TJoin6 : class
		where TJoin7 : class
		where TJoin8 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TJoin9, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TJoin9, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
		where TJoin6 : class
		where TJoin7 : class
		where TJoin8 : class
		where TJoin9 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	/// <summary>
	/// Projects the entities and joined entities to the specified result type.
	/// </summary>
	public DataSpecification<TData, TResult> Select<TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TJoin9, TJoin10, TResult>(
		Expression<Func<TData, TJoin1, TJoin2, TJoin3, TJoin4, TJoin5, TJoin6, TJoin7, TJoin8, TJoin9, TJoin10, TResult>> selector)
		where TJoin1 : class
		where TJoin2 : class
		where TJoin3 : class
		where TJoin4 : class
		where TJoin5 : class
		where TJoin6 : class
		where TJoin7 : class
		where TJoin8 : class
		where TJoin9 : class
		where TJoin10 : class
	{
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			_source,
			_predicate,
			selector,
			_joins,
			_groupBy,
			_having,
			_orderBy,
			_skip,
			_take,
			_isDistinct,
			SelectorTranslatabilityVisitor.Classify(selector));
	}

	// Cached per closed generic type
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
		ParameterExpression parameter = Expression.Parameter(typeof(TData), "x");

		Expression leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
		Expression rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

		BinaryExpression combined = Expression.AndAlso(leftBody, rightBody);
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

	/// <inheritdoc />
	public string? Source { get; }

	/// <inheritdoc />
	public SelectorEvaluation SelectorEvaluation { get; }

	internal DataSpecification(
		string? source,
		LambdaExpression? predicate,
		LambdaExpression selector,
		ImmutableArray<IJoinSpecification> joins,
		ImmutableArray<LambdaExpression> groupBy,
		LambdaExpression? having,
		ImmutableArray<OrderSpecification> orderBy,
		int? skip,
		int? take,
		bool isDistinct,
		SelectorEvaluation selectorEvaluation)
	{
		Source = source;
		Predicate = predicate;
		Selector = selector ?? throw new ArgumentNullException(nameof(selector));
		Joins = joins;
		GroupBy = groupBy;
		Having = having;
		OrderBy = orderBy;
		Skip = skip;
		Take = take;
		IsDistinct = isDistinct;
		SelectorEvaluation = selectorEvaluation;
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

/// <summary>
/// Walks a selector expression tree to determine whether it can be fully
/// translated to SQL column projections (Server) or must be applied
/// client-side after data retrieval (Client).
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="NewExpression"/> or <see cref="MemberInitExpression"/> at the top level
/// of the selector body is translatable (it becomes the SELECT column list).
/// However, the same expression types nested inside another expression are NOT
/// translatable because SQL cannot construct objects within a SELECT column.
/// </para>
/// <para>
/// To distinguish these cases, the <see cref="Classify"/> method visits only the
/// <em>children</em> of a top-level construction. Any <c>NewExpression</c> or
/// <c>MemberInitExpression</c> encountered during child traversal triggers
/// <see cref="SelectorEvaluation.Client"/> classification.
/// </para>
/// </remarks>
file sealed class SelectorTranslatabilityVisitor : ExpressionVisitor
{
	private bool _isTranslatable = true;

	public static SelectorEvaluation Classify(LambdaExpression selector)
	{
		var visitor = new SelectorTranslatabilityVisitor();
		visitor.VisitSelectorBody(selector.Body);
		return visitor._isTranslatable ? SelectorEvaluation.Server : SelectorEvaluation.Client;
	}

	/// <summary>
	/// Visits the selector body, treating top-level <see cref="NewExpression"/> and
	/// <see cref="MemberInitExpression"/> as translatable SELECT scaffolding while
	/// checking their children for untranslatable nested constructions.
	/// </summary>
	private void VisitSelectorBody(Expression body)
	{
		// Top-level NewExpression (e.g., new Dto(p.Id, p.Name) or new { p.Id }):
		// The construction itself maps to the SELECT column list.
		// Visit only the constructor arguments for untranslatable children.
		if (body is NewExpression topNew)
		{
			foreach (Expression arg in topNew.Arguments)
			{
				Visit(arg);
			}
			return;
		}

		// Top-level MemberInitExpression (e.g., new Dto { Id = p.Id, Nested = new X { } }):
		// Visit the constructor arguments and each binding value expression.
		if (body is MemberInitExpression topInit)
		{
			foreach (Expression arg in topInit.NewExpression.Arguments)
			{
				if (IsNonScalarParameter(arg))
				{
					_isTranslatable = false;
					return;
				}
				Visit(arg);
			}
			foreach (MemberBinding binding in topInit.Bindings)
			{
				if (binding is MemberAssignment assignment)
				{
					if (IsNonScalarParameter(assignment.Expression))
					{
						_isTranslatable = false;
						return;
					}
					Visit(assignment.Expression);
				}
			}
			return;
		}

		// All other expression types: visit the full tree.
		Visit(body);
	}

	public override Expression? Visit(Expression? node)
	{
		if (!_isTranslatable)
			return node;

		return base.Visit(node);
	}

	protected override Expression VisitNew(NewExpression node)
	{
		// Nested object construction cannot be translated to SQL.
		_isTranslatable = false;
		return node;
	}

	protected override Expression VisitMemberInit(MemberInitExpression node)
	{
		// Nested object initialization cannot be translated to SQL.
		_isTranslatable = false;
		return node;
	}

	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		if (!IsKnownSqlMethod(node))
		{
			_isTranslatable = false;
			return node;
		}

		return base.VisitMethodCall(node);
	}

	private static bool IsNonScalarParameter(Expression expression)
	{
		if (expression is not ParameterExpression param)
		{
			return false;
		}

		Type type = Nullable.GetUnderlyingType(param.Type) ?? param.Type;
		return !type.IsPrimitive
			   && !type.IsEnum
			   && type != typeof(string)
			   && type != typeof(Guid)
			   && type != typeof(DateTime)
			   && type != typeof(DateTimeOffset)
			   && type != typeof(DateOnly)
			   && type != typeof(TimeOnly)
			   && type != typeof(decimal);
	}

	protected override Expression VisitInvocation(InvocationExpression node)
	{
		_isTranslatable = false;
		return node;
	}

	private static bool IsKnownSqlMethod(MethodCallExpression mc)
	{
		// Sql aggregate marker methods (Count, Sum, Avg, Min, Max, CountDistinct)
		if (mc.Method.DeclaringType == typeof(SqlFunctions))
		{
			return true;
		}

		if (mc.Method.DeclaringType == typeof(string))
		{
			return mc.Method.Name is "Contains" or "StartsWith" or "EndsWith"
				or "Equals" or "ToLower" or "ToLowerInvariant" or "ToUpper" or "ToUpperInvariant"
				or "Trim" or "TrimStart" or "TrimEnd" or "Substring" or "Replace" or "IndexOf"
				or "Concat" or "IsNullOrEmpty" or "IsNullOrWhiteSpace";
		}

		if (mc.Method.Name == "Equals" && mc.Object is not null && mc.Arguments.Count == 1)
		{
			return true;
		}

		if (mc.Method.Name == "GetValueOrDefault"
			&& mc.Object is not null
			&& Nullable.GetUnderlyingType(mc.Object.Type) is not null)
		{
			return true;
		}

		if (mc.Method.Name == "Contains" && mc.Method.DeclaringType != typeof(string))
		{
			return true;
		}

		return false;
	}
}
