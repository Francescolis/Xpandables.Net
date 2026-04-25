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
using System.Linq.Expressions;

namespace System.Data;

/// <summary>
/// Defines a specification for querying data with SQL-specific filtering, joins, grouping, sorting, and paging.
/// </summary>
/// <typeparam name="TData">The type of data to query.</typeparam>
/// <typeparam name="TResult">The type of the projected result.</typeparam>
/// <remarks>
/// <para>Use <see cref="DataSpecification.For{TEntity}()"/> to create specifications using a fluent builder pattern.</para>
/// </remarks>
public interface IDataSpecification<TData, TResult>
	where TData : class
{
	/// <summary>
	/// Gets the predicate expression to filter data.
	/// </summary>
	/// <remarks>When <see langword="null"/>, no filtering is applied.</remarks>
	LambdaExpression? Predicate { get; }

	/// <summary>
	/// Gets the projection expression to transform data into results.
	/// </summary>
	LambdaExpression Selector { get; }

	/// <summary>
	/// Gets the join specifications to apply in the query.
	/// </summary>
	IReadOnlyList<IJoinSpecification> Joins { get; }

	/// <summary>
	/// Gets the grouping expressions to apply to the query.
	/// </summary>
	IReadOnlyList<LambdaExpression> GroupBy { get; }

	/// <summary>
	/// Gets the HAVING predicate applied after grouping.
	/// </summary>
	LambdaExpression? Having { get; }

	/// <summary>
	/// Gets the collection of ordering specifications.
	/// </summary>
	IReadOnlyList<OrderSpecification> OrderBy { get; }

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

	/// <summary>
	/// Gets the optional source table, view, or raw SQL subquery to use in the FROM clause
	/// instead of the default table name derived from <typeparamref name="TData"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When set, this value replaces the default <c>FROM [TableName]</c> clause.
	/// It can be a simple view or table name (e.g., <c>"vw_ActiveProducts"</c>),
	/// or a raw SQL subquery (e.g., <c>"SELECT * FROM Products WHERE Region = 'EU'"</c>).
	/// </para>
	/// <para>
	/// <strong>SQL Injection Warning:</strong> The source string is inserted into the generated SQL
	/// without parameterization. Callers must ensure the value is safe and does not originate from
	/// untrusted user input.
	/// </para>
	/// <para>
	/// <strong>Column Alignment:</strong> When using a raw SQL subquery, the subquery must select
	/// all columns required by the <typeparamref name="TData"/> entity type (matching property names
	/// or <see cref="System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"/> names).
	/// </para>
	/// </remarks>
	string? Source { get; }

	/// <summary>
	/// Gets a value indicating how the selector expression is evaluated.
	/// </summary>
	/// <remarks>
	/// When <see cref="SelectorEvaluation.Server"/>, the selector is translated to SQL column projections.
	/// When <see cref="SelectorEvaluation.Client"/>, all mapped columns are selected and the
	/// selector is applied in memory after reading from the database.
	/// </remarks>
	SelectorEvaluation SelectorEvaluation { get; }
}

/// <summary>
/// Specifies how a selector expression is evaluated during query execution.
/// </summary>
public enum SelectorEvaluation
{
	/// <summary>
	/// The evaluation mode is determined automatically by inspecting the expression tree.
	/// Translatable expressions are evaluated server-side; untranslatable ones client-side.
	/// </summary>
	Auto = 0,

	/// <summary>
	/// The selector is translated into SQL column projections and evaluated by the database server.
	/// </summary>
	Server = 1,

	/// <summary>
	/// All mapped columns are selected and the selector is applied client-side after data retrieval.
	/// Use this for selectors containing method calls, extension methods, or complex transformations
	/// that cannot be translated to SQL.
	/// </summary>
	Client = 2
}

/// <summary>
/// Specifies the join type to apply.
/// </summary>
public enum SqlJoinType
{
	/// <summary>
	/// Represents an inner join.
	/// </summary>
	Inner,
	/// <summary>
	/// Represents a left join.
	/// </summary>
	Left,
	/// <summary>
	/// Represents a right join.
	/// </summary>
	Right,
	/// <summary>
	/// Represents a full outer join.
	/// </summary>
	Full,
	/// <summary>
	/// Represents a cross join.
	/// </summary>
	Cross
}

/// <summary>
/// Represents a join specification for a SQL query.
/// </summary>
public interface IJoinSpecification
{
	/// <summary>
	/// Gets the left side entity type of the join.
	/// </summary>
	Type LeftType { get; }

	/// <summary>
	/// Gets the right side entity type of the join.
	/// </summary>
	Type RightType { get; }

	/// <summary>
	/// Gets the join type.
	/// </summary>
	SqlJoinType JoinType { get; }

	/// <summary>
	/// Gets the join predicate expression.
	/// </summary>
	LambdaExpression? OnExpression { get; }

	/// <summary>
	/// Gets the optional table alias.
	/// </summary>
	string? TableAlias { get; }
}

/// <summary>
/// Represents an ordering specification for SQL queries.
/// </summary>
public readonly record struct OrderSpecification(LambdaExpression KeySelector, bool Descending);
