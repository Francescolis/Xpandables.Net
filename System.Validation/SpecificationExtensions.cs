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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Extension methods for working with specifications.
/// </summary>
public static class SpecificationExtensions
{
	/// <summary>
	/// Combines this specification with another using logical AND.
	/// </summary>
	/// <param name="specification">The first specification.</param>
	/// <param name="other">The specification to combine with.</param>
	/// <returns>A new specification representing the logical AND of both specifications.</returns>
	public static ISpecification<TSource> And<TSource>(this ISpecification<TSource> specification, ISpecification<TSource> other)
	{
		ArgumentNullException.ThrowIfNull(specification);
		ArgumentNullException.ThrowIfNull(other);
		return Specification.And(specification, other);
	}

	/// <summary>
	/// Combines this specification with another using logical OR.
	/// </summary>
	/// <param name="specification">The first specification.</param>
	/// <param name="other">The specification to combine with.</param>
	/// <returns>A new specification representing the logical OR of both specifications.</returns>
	public static ISpecification<TSource> Or<TSource>(this ISpecification<TSource> specification, ISpecification<TSource> other)
	{
		ArgumentNullException.ThrowIfNull(specification);
		ArgumentNullException.ThrowIfNull(other);
		return Specification.Or(specification, other);
	}

	/// <summary>
	/// Combines this specification with another using logical OR with short-circuit evaluation.
	/// </summary>
	/// <param name="specification">The first specification.</param>
	/// <param name="other">The specification to combine with.</param>
	/// <returns>A new specification representing the logical OR with short-circuit evaluation.</returns>
	public static ISpecification<TSource> OrElse<TSource>(this ISpecification<TSource> specification, ISpecification<TSource> other)
	{
		ArgumentNullException.ThrowIfNull(specification);
		ArgumentNullException.ThrowIfNull(other);
		return Specification.OrElse(specification, other);
	}

	/// <summary>
	/// Creates the logical negation of this specification.
	/// </summary>
	/// <returns>A new specification representing the logical NOT of the specification.</returns>
	public static ISpecification<TSource> Not<TSource>(this ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(specification);
		return Specification.Not(specification);
	}

	/// <summary>
	/// Converts an expression to a specification.
	/// </summary>
	/// <returns>A specification based on the expression.</returns>
	public static ISpecification<TSource> ToSpecification<TSource>(this Expression<Func<TSource, bool>> expression)
	{
		ArgumentNullException.ThrowIfNull(expression);
		return Specification.FromExpression(expression);
	}

	/// <summary>
	/// Applies the specification to filter a queryable collection.
	/// </summary>
	/// <param name="queryable">The queryable collection to filter.</param>
	/// <param name="specification">The specification to apply.</param>
	/// <returns>A filtered queryable collection.</returns>
	public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> queryable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(queryable);
		ArgumentNullException.ThrowIfNull(specification);
		return queryable.Where(specification.Expression);
	}

	/// <summary>
	/// Applies the specification to filter an enumerable collection.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to filter.</param>
	/// <param name="specification">The specification to apply.</param>
	/// <returns>A filtered enumerable collection.</returns>
	public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.Where(specification.Expression.Compile());
	}

	/// <summary>
	/// Determines whether any element in the collection satisfies the specification.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns><see langword="true"/> if any element satisfies the specification; otherwise, <see langword="false"/>.</returns>
	public static bool Any<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.Any(specification.Expression.Compile());
	}

	/// <summary>
	/// Determines whether all elements in the collection satisfy the specification.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns><see langword="true"/> if all elements satisfy the specification; otherwise, <see langword="false"/>.</returns>
	public static bool All<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.All(specification.Expression.Compile());
	}

	/// <summary>
	/// Returns the first element that satisfies the specification.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns>The first element that satisfies the specification.</returns>
	/// <exception cref="InvalidOperationException">No element satisfies the specification.</exception>
	public static TSource First<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.First(specification.Expression.Compile());
	}

	/// <summary>
	/// Returns the first element that satisfies the specification, or a default value if no such element is found.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns>The first element that satisfies the specification, or a default value.</returns>
	public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.FirstOrDefault(specification.Expression.Compile());
	}

	/// <summary>
	/// Returns the single element that satisfies the specification.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns>The single element that satisfies the specification.</returns>
	/// <exception cref="InvalidOperationException">No element or more than one element satisfies the specification.</exception>
	[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
	public static TSource Single<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.Single(specification.Expression.Compile());
	}

	/// <summary>
	/// Returns the single element that satisfies the specification, or a default value if no such element is found.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns>The single element that satisfies the specification, or a default value.</returns>
	/// <exception cref="InvalidOperationException">More than one element satisfies the specification.</exception>
	public static TSource? SingleOrDefault<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.SingleOrDefault(specification.Expression.Compile());
	}

	/// <summary>
	/// Returns the count of elements that satisfy the specification.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to test.</param>
	/// <param name="specification">The specification to test.</param>
	/// <returns>The count of elements that satisfy the specification.</returns>
	public static int Count<TSource>(this IEnumerable<TSource> enumerable, ISpecification<TSource> specification)
	{
		ArgumentNullException.ThrowIfNull(enumerable);
		ArgumentNullException.ThrowIfNull(specification);
		return enumerable.Count(specification.Expression.Compile());
	}

	/// <summary>
	/// Combines multiple specifications using logical AND.
	/// </summary>
	/// <returns>A new specification representing the logical AND of all specifications.</returns>
	public static ISpecification<TSource> AllOf<TSource>(this IEnumerable<ISpecification<TSource>> specifications)
	{
		ArgumentNullException.ThrowIfNull(specifications);
		ISpecification<TSource>[] specArray = specifications.ToArray();
		return Specification.All(specArray);
	}

	/// <summary>
	/// Combines multiple specifications using logical OR.
	/// </summary>
	/// <returns>A new specification representing the logical OR of all specifications.</returns>
	public static ISpecification<TSource> AnyOf<TSource>(this IEnumerable<ISpecification<TSource>> specifications)
	{
		ArgumentNullException.ThrowIfNull(specifications);
		ISpecification<TSource>[] specArray = specifications.ToArray();
		return Specification.Any(specArray);
	}
}
