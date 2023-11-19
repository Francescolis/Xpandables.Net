
/************************************************************************************************************
 * Copyright (C) 2022 Francis-Black EWANE
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
************************************************************************************************************/
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Xpandables.Net.Operations.Specifications;

/// <summary>
/// Provides the specification extensions that contains methods to map generic specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Applies the AND operator to both specifications and returns a new one.
    /// </summary>
    /// <param name="left">The specification left side.</param>
    /// <param name="right">The specification right side.</param>
    /// <returns>A new <see cref="Specification{TSource}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    [return: NotNull]
    public static Specification<TSource> And<TSource>(this ISpecification<TSource> left, ISpecification<TSource> right)
        => new SpecificationAnd<TSource>(left, right);

    /// <summary>
    /// Applies the AND operator to specification and expression, and returns a new specification.
    /// </summary>
    /// <param name="left">The specification left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns>A new <see cref="Specification{TSource}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    [return: NotNull]
    public static Specification<TSource> And<TSource>(this ISpecification<TSource> left, Expression<Func<TSource, bool>> right)
        => new SpecificationAnd<TSource>(left, new SpecificationExpression<TSource>(right));

    /// <summary>
    /// Applies the OR operator to both specifications and returns a new one.
    /// </summary>
    /// <param name="left">The specification left side.</param>
    /// <param name="right">The specification right side.</param>
    /// <returns>A new <see cref="Specification{TSource}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    [return: NotNull]
    public static Specification<TSource> Or<TSource>(this ISpecification<TSource> left, ISpecification<TSource> right)
        => new SpecificationOr<TSource>(left, right);

    /// <summary>
    /// Applies the OR operator to specification and expression, and returns a new one.
    /// </summary>
    /// <param name="left">The specification left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns>A new <see cref="Specification{TSource}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    [return: NotNull]
    public static Specification<TSource> Or<TSource>(this ISpecification<TSource> left, Expression<Func<TSource, bool>> right)
        => new SpecificationOr<TSource>(left, new SpecificationExpression<TSource>(right));

    /// <summary>
    /// Applies the NOT operator to the specification and returns a new one.
    /// </summary>
    /// <param name="specification">The specification to act on.</param>
    /// <returns>A new <see cref="Specification{TSource}"/> object</returns>
    /// <exception cref="ArgumentNullException"></exception>
    [return: NotNull]
    public static Specification<TSource> Not<TSource>(this ISpecification<TSource> specification)
        => new SpecificationNot<TSource>(specification);

    /// <summary>
    /// Returns a boolean value that determines whether or not the source satisfied to the specified specification.
    /// If so, returns <see langword="true"/>, otherwise <see langword="false"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to check for.</typeparam>
    /// <param name="source">The target object to check for.</param>
    /// <param name="specification">The specification to be applied.</param>
    /// <returns><see langword="true"/> if specification is satisfied, otherwise <see langword="false"/>.</returns>
    public static bool SatisfiesTo<TSource>(this TSource source, ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.IsSatisfiedBy(source);
    }

    /// <summary>
    /// Returns a boolean value that determines whether or not the collection of items satisfy to the.
    /// If so, returns <see langword="true"/>, otherwise <see langword="false"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to check for.</typeparam>
    /// <param name="sources">The collection of items to check for.</param>
    /// <param name="specification">The specification to be applied.</param>
    /// <returns><see langword="true"/> if specification is satisfied by all items, otherwise <see langword="false"/>.</returns>
    public static bool SatisfyTo<TSource>(this IEnumerable<TSource> sources, ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return sources.All(specification.IsSatisfiedBy);
    }
}
