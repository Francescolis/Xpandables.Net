
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

namespace Xpandables.Net.Expressions;
/// <summary>
/// Represents a factory for creating query expressions.
/// </summary>
public sealed record QueryExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="expression">The expression to be used.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static QueryExpression<TSource, TResult> Create<TSource, TResult>(
        Expression<Func<TSource, TResult>> expression) =>
        new() { Expression = expression };

    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="expression">The expression to be used.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static QueryExpression<TSource, bool> Create<TSource>(
        Expression<Func<TSource, bool>> expression) =>
        new() { Expression = expression };

    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/> with a 
    /// default expression that always returns true.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static QueryExpression<TSource, bool> Create<TSource>() =>
        new() { Expression = _ => true };
}
