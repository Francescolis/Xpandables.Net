
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
using System.Linq.Expressions;

namespace Xpandables.Net.Operations.Expressions;

/// <summary>
/// Provides with extension methods for <see cref="Expression"/>.
/// </summary>
public static class QueryExpressionFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/> with <see cref="bool"/> result that return <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TSource">The data type source.</typeparam>
    /// <returns>a new instance of <see cref="QueryExpression{TSource, TResult}"/> with boolean result.</returns>
    public static QueryExpression<TSource, bool> Create<TSource>() => new QueryExpressionBuilder<TSource, bool>(_ => true);

    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/> from the specified expression.
    /// </summary>
    /// <typeparam name="TSource">The data type source.</typeparam>
    /// <param name="expression">The expression to be wrapped.</param>
    /// <returns>a new instance of <see cref="QueryExpression{TSource, TResult}"/> with boolean result.</returns>
    public static QueryExpression<TSource, bool> Create<TSource>(Expression<Func<TSource, bool>> expression)
        => new QueryExpressionBuilder<TSource, bool>(expression);

    /// <summary>
    /// Returns the member name from the expression if found, otherwise returns null.
    /// </summary>
    /// <typeparam name="TSource">The type of the model class.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertyExpression">The expression that contains the member name.</param>
    /// <returns>A string that represents the name of the member found in the expression.</returns>
    public static string? GetMemberName<TSource, TProperty>(
        this Expression<Func<TSource, TProperty>> propertyExpression)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        if (propertyExpression.NodeType == ExpressionType.Constant)
        {
            var expression = propertyExpression as Expression;
            var constantExpression = expression as ConstantExpression;
            return constantExpression?.Value!.ToString();
        }

        if (propertyExpression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (propertyExpression.Body is UnaryExpression { Operand: MemberExpression operandExpression })
            return operandExpression.Member.Name;

        return default;
    }

    /// <summary>
    /// Returns a property or field access-or expression for the specified name that matches a property or a field in the model.
    /// </summary>
    /// <typeparam name="TSource">The type of the model class.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertyOrFieldName">The name of the property or field.</param>
    /// <returns>An expression tree.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyOrFieldName"/> is null.</exception>
    /// <exception cref="ArgumentException">No property or field named propertyOrFieldName is
    /// defined in expression.Type or its base types.</exception>
    public static Expression<Func<TSource, TProperty>> CreateAccessorFor<TSource, TProperty>(
        this string propertyOrFieldName)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(propertyOrFieldName);

        var paramExpr = Expression.Parameter(typeof(TSource));
        var bodyExpr = Expression.PropertyOrField(paramExpr, propertyOrFieldName);
        return Expression.Lambda<Func<TSource, TProperty>>(bodyExpr, paramExpr);
    }
}

/// <summary>
/// Provides the query expression factory that contains methods to create generic query expressions.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public static class QueryExpressionFactory<TResult>
{
    /// <summary>
    /// Creates a new instance of <see cref="QueryExpression{TSource, TResult}"/> with the specified expression.
    /// </summary>
    /// <typeparam name="TSource">The data type source.</typeparam>
    /// <param name="expression">The expression to be used by the instance.</param>
    /// <returns>a new instance of <see cref="QueryExpression{TSource, TResult}"/></returns>
    /// <exception cref="ArgumentNullException">The <paramref name="expression"/> is null.</exception>
    public static QueryExpression<TSource, TResult> Create<TSource>(Expression<Func<TSource, TResult>> expression)
        => new QueryExpressionBuilder<TSource, TResult>(expression);

    /// <summary>
    /// Returns the <see cref="Expression{TDelegate}"/> that represents the And form of two expressions.
    /// </summary>
    /// <typeparam name="TSource">The type of the expression parameter.</typeparam>
    /// <param name="left">The expression value  for left side.</param>
    /// <param name="right">The expression value for right side.</param>
    /// <returns><see cref="Expression{TDelegate}"/> result</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static Expression<Func<TSource, TResult>> And<TSource>(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var invokedExpr = Expression.Invoke(right, left.Parameters!);
        return Expression.Lambda<Func<TSource, TResult>>(Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
    }

    /// <summary>
    /// Returns the <see cref="Expression{TDelegate}"/> that represents the Or form of two expressions.
    /// </summary>
    /// <typeparam name="TSource">The type of the expression parameter.</typeparam>
    /// <param name="left">The expression value  for left side.</param>
    /// <param name="right">The expression value for right side.</param>
    /// <returns><see cref="Expression{TDelegate}"/> result</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static Expression<Func<TSource, TResult>> Or<TSource>(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var invokedExpr = Expression.Invoke(right, left.Parameters!);
        return Expression.Lambda<Func<TSource, TResult>>(Expression.OrElse(left.Body, invokedExpr), left.Parameters);
    }

    /// <summary>
    /// Returns the <see cref="Expression{TDelegate}"/> that represents the Not form of an expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the expression parameter.</typeparam>
    /// <param name="expression">The expression value.</param>
    /// <returns><see cref="Expression{TDelegate}"/> result</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="expression"/>is null.</exception>
    public static Expression<Func<TSource, TResult>> Not<TSource>(Expression<Func<TSource, TResult>> expression)
    {
        var left = expression ?? throw new ArgumentNullException(nameof(expression));

        return Expression.Lambda<Func<TSource, TResult>>(Expression.Not(left.Body), left.Parameters);
    }
}
