
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

using Xpandables.Net.Expressions;

namespace Xpandables.Net.DataAnnotations;
/// <summary>
/// Provides extension methods for <see cref="Specification{TSource}"/>.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications with a logical AND.
    /// </summary>
    public static Specification<TSource> AndAlso<TSource>(
        this Specification<TSource> left, Specification<TSource> right) =>
        new(left, right, ExpressionType.AndAlso);

    /// <summary>
    /// Combines a specification and an expression with a logical AND.
    /// </summary>
    public static Specification<TSource> AndAlso<TSource>(
        this Specification<TSource> left, Expression<Func<TSource, bool>> right) =>
        new(left, right, ExpressionType.AndAlso);

    /// <summary>
    /// Combines two specifications with a bitwise AND.
    /// </summary>
    public static Specification<TSource> And<TSource>(
        this Specification<TSource> left, Specification<TSource> right) =>
        new(left, right, ExpressionType.And);

    /// <summary>
    /// Combines a specification and an expression with a bitwise AND.
    /// </summary>
    public static Specification<TSource> And<TSource>(
        this Specification<TSource> left, Expression<Func<TSource, bool>> right) =>
        new(left, right, ExpressionType.And);

    /// <summary>
    /// Combines two specifications with a logical OR.
    /// </summary>
    public static Specification<TSource> OrElse<TSource>(
        this Specification<TSource> left, Specification<TSource> right) =>
        new(left, right, ExpressionType.OrElse);

    /// <summary>
    /// Combines a specification and an expression with a logical OR.
    /// </summary>
    public static Specification<TSource> OrElse<TSource>(
        this Specification<TSource> left, Expression<Func<TSource, bool>> right) =>
        new(left, right, ExpressionType.OrElse);

    /// <summary>
    /// Combines two specifications with a bitwise OR.
    /// </summary>
    public static Specification<TSource> Or<TSource>(
        this Specification<TSource> left, Specification<TSource> right) =>
        new(left, right, ExpressionType.Or);

    /// <summary>
    /// Combines a specification and an expression with a bitwise OR.
    /// </summary>
    public static Specification<TSource> Or<TSource>(
        this Specification<TSource> left, Expression<Func<TSource, bool>> right) =>
        new(left, right, ExpressionType.Or);

    /// <summary>
    /// Negates the given specification.
    /// </summary>
    public static Specification<TSource> Not<TSource>(
        this Specification<TSource> specification) =>
        !specification;

    /// <summary>
    /// Negates the given expression as a specification.
    /// </summary>
    public static Specification<TSource> Not<TSource>(
        this Expression<Func<TSource, bool>> expression) =>
        new(QueryExpressionExtensions.Not(expression));
}
