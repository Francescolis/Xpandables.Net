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

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Extension methods for working with specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// <see cref="ISpecification{TSource}"/> extension methods.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="specification">The current specification.</param>
    extension<TSource>(ISpecification<TSource> specification)
    {
        /// <summary>
        /// Combines this specification with another using logical AND.
        /// </summary>
        /// <param name="other">The specification to combine with.</param>
        /// <returns>A new specification representing the logical AND of both specifications.</returns>
        public ISpecification<TSource> And(ISpecification<TSource> other)
        {
            ArgumentNullException.ThrowIfNull(specification);
            ArgumentNullException.ThrowIfNull(other);
            return Specification.And(specification, other);
        }

        /// <summary>
        /// Combines this specification with another using logical OR.
        /// </summary>
        /// <param name="other">The specification to combine with.</param>
        /// <returns>A new specification representing the logical OR of both specifications.</returns>
        public ISpecification<TSource> Or(ISpecification<TSource> other)
        {
            ArgumentNullException.ThrowIfNull(specification);
            ArgumentNullException.ThrowIfNull(other);
            return Specification.Or(specification, other);
        }

        /// <summary>
        /// Combines this specification with another using logical OR with short-circuit evaluation.
        /// </summary>
        /// <param name="other">The specification to combine with.</param>
        /// <returns>A new specification representing the logical OR with short-circuit evaluation.</returns>
        public ISpecification<TSource> OrElse(ISpecification<TSource> other)
        {
            ArgumentNullException.ThrowIfNull(specification);
            ArgumentNullException.ThrowIfNull(other);
            return Specification.OrElse(specification, other);
        }

        /// <summary>
        /// Creates the logical negation of this specification.
        /// </summary>
        /// <returns>A new specification representing the logical NOT of the specification.</returns>
        public ISpecification<TSource> Not()
        {
            ArgumentNullException.ThrowIfNull(specification);
            return Specification.Not(specification);
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="expression">The expression to convert to a specification.</param>
    extension<TSource>(Expression<Func<TSource, bool>> expression)
    {
        /// <summary>
        /// Converts an expression to a specification.
        /// </summary>
        /// <returns>A specification based on the expression.</returns>
        public ISpecification<TSource> ToSpecification()
        {
            ArgumentNullException.ThrowIfNull(expression);
            return Specification.FromExpression(expression);
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <param name="queryable">The queryable collection.</param>
    extension<TSource>(IQueryable<TSource> queryable)
    {
        /// <summary>
        /// Applies the specification to filter a queryable collection.
        /// </summary>
        /// <param name="specification">The specification to apply.</param>
        /// <returns>A filtered queryable collection.</returns>
        public IQueryable<TSource> Where(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(queryable);
            ArgumentNullException.ThrowIfNull(specification);
            return queryable.Where(specification.Expression);
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
    /// <param name="enumerable">The enumerable collection.</param>
    extension<TSource>(IEnumerable<TSource> enumerable)
    {
        /// <summary>
        /// Applies the specification to filter an enumerable collection.
        /// </summary>
        /// <param name="specification">The specification to apply.</param>
        /// <returns>A filtered enumerable collection.</returns>
        public IEnumerable<TSource> Where(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.Where(specification.Expression.Compile());
        }

        /// <summary>
        /// Determines whether any element in the collection satisfies the specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns><see langword="true"/> if any element satisfies the specification; otherwise, <see langword="false"/>.</returns>
        public bool Any(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.Any(specification.Expression.Compile());
        }

        /// <summary>
        /// Determines whether all elements in the collection satisfy the specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns><see langword="true"/> if all elements satisfy the specification; otherwise, <see langword="false"/>.</returns>
        public bool All(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.All(specification.Expression.Compile());
        }

        /// <summary>
        /// Returns the first element that satisfies the specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>The first element that satisfies the specification.</returns>
        /// <exception cref="InvalidOperationException">No element satisfies the specification.</exception>
        public TSource First(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.First(specification.Expression.Compile());
        }

        /// <summary>
        /// Returns the first element that satisfies the specification, or a default value if no such element is found.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>The first element that satisfies the specification, or a default value.</returns>
        public TSource? FirstOrDefault(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.FirstOrDefault(specification.Expression.Compile());
        }

        /// <summary>
        /// Returns the single element that satisfies the specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>The single element that satisfies the specification.</returns>
        /// <exception cref="InvalidOperationException">No element or more than one element satisfies the specification.</exception>
        [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
        public TSource Single(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.Single(specification.Expression.Compile());
        }

        /// <summary>
        /// Returns the single element that satisfies the specification, or a default value if no such element is found.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>The single element that satisfies the specification, or a default value.</returns>
        /// <exception cref="InvalidOperationException">More than one element satisfies the specification.</exception>
        public TSource? SingleOrDefault(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.SingleOrDefault(specification.Expression.Compile());
        }

        /// <summary>
        /// Returns the count of elements that satisfy the specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>The count of elements that satisfy the specification.</returns>
        public int Count(ISpecification<TSource> specification)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            ArgumentNullException.ThrowIfNull(specification);
            return enumerable.Count(specification.Expression.Compile());
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specifications are applied.</typeparam>
    /// <param name="specifications">The specifications to combine.</param>
    extension<TSource>(IEnumerable<ISpecification<TSource>> specifications)
    {
        /// <summary>
        /// Combines multiple specifications using logical AND.
        /// </summary>
        /// <returns>A new specification representing the logical AND of all specifications.</returns>
        public ISpecification<TSource> AllOf()
        {
            ArgumentNullException.ThrowIfNull(specifications);
            var specArray = specifications.ToArray();
            return Specification.All(specArray);
        }

        /// <summary>
        /// Combines multiple specifications using logical OR.
        /// </summary>
        /// <returns>A new specification representing the logical OR of all specifications.</returns>
        public ISpecification<TSource> AnyOf()
        {
            ArgumentNullException.ThrowIfNull(specifications);
            var specArray = specifications.ToArray();
            return Specification.Any(specArray);
        }
    }
}