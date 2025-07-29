
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
using System.Linq.Expressions;


namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides extension methods for working with specifications and collections.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Converts an expression to a specification.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A new specification based on the expression.</returns>
    public static Specification<TSource> AsSpecification<TSource>(
        this Expression<Func<TSource, bool>> expression) =>
        new(expression);

    /// <summary>
    /// Negates the given expression as a specification.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="expression">The expression to negate.</param>
    /// <returns>A new specification representing the negation of the expression.</returns>
    public static Specification<TSource> Not<TSource>(
        this Expression<Func<TSource, bool>> expression) =>
        Specification.Not(new Specification<TSource>(expression));
}