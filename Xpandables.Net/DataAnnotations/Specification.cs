
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Xpandables.Net.Expressions;

namespace Xpandables.Net.DataAnnotations;
/// <summary>
/// Represents a specification that can be used to evaluate whether a given 
/// source satisfies certain criteria.
/// </summary>
/// <typeparam name="TSource">The type of the source to be evaluated.</typeparam>
public record Specification<TSource> : QueryExpression<TSource, bool>, ISpecification<TSource>
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="Specification{TSource}"/> class of 
    /// <typeparamref name="TSource"/> type.
    /// </summary>
    public Specification() { }

    /// <inheritdoc/>
    public bool IsSatisfiedBy(TSource source) =>
        Expression.Compile().Invoke(source);

    [SetsRequiredMembers]
    private Specification(
         Expression<Func<TSource, bool>> left,
         Expression<Func<TSource, bool>> right,
         ExpressionType expressionType) :
        base(left, right, expressionType)
    { }

    [SetsRequiredMembers]
    private Specification(Expression<Func<TSource, bool>> expression) : base(expression)
    { }

    /// <summary>
    /// Combines two specifications using a logical AND operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical AND of 
    /// the two specifications.</returns>
    public static Specification<TSource> operator &(Specification<TSource> left, Specification<TSource> right) =>
        new(left, right.Expression, ExpressionType.AndAlso);

    /// <summary>
    /// Combines two specifications using a logical OR operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical OR of 
    /// the two specifications.</returns>
    public static Specification<TSource> operator |(Specification<TSource> left, Specification<TSource> right) =>
        new(left, right.Expression, ExpressionType.OrElse);

    /// <summary>  
    /// Negates the given specification.  
    /// </summary>  
    /// <param name="expression">The specification to negate.</param>  
    /// <returns>A new specification that represents the negation of the given 
    /// specification.</returns> 
    public static Specification<TSource> operator !(Specification<TSource> expression) =>
        new(expression.Expression);
}