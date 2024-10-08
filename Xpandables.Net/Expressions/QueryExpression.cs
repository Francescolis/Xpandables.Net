﻿
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
/// The base class to define a class expression.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
/// <typeparam name="TResult">The type of the result of expression.</typeparam>
public abstract record class QueryExpression<TSource, TResult>
    : IQueryExpression<TSource, TResult>
{
    /// <summary>
    /// Gets the expression tree for the underlying instance.
    /// </summary>
    public abstract Expression<Func<TSource, TResult>> GetExpression();

    /// <summary>
    /// Returns the unique hash code for the current instance.
    /// </summary>
    /// <returns><see cref="int"/> value.</returns>
    public override int GetHashCode() => GetExpression().GetHashCode();

    ///<inheritdoc/>
    public static implicit operator Expression<Func<TSource, TResult>>(
         QueryExpression<TSource, TResult> queryExpression)
    {
        ArgumentNullException.ThrowIfNull(queryExpression);
        return queryExpression.GetExpression();
    }

    ///<inheritdoc/>
    public static implicit operator Func<TSource, TResult>(
         QueryExpression<TSource, TResult> queryExpression)
    {
        ArgumentNullException.ThrowIfNull(queryExpression);
        return queryExpression.GetExpression().Compile();
    }

    ///<inheritdoc/>
    public static implicit operator QueryExpression<TSource, TResult>(
         Expression<Func<TSource, TResult>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return QueryExpressionFactory.Create(expression);
    }

    ///<inheritdoc/>
    public static QueryExpression<TSource, TResult> operator &(
         QueryExpression<TSource, TResult> left,
         QueryExpression<TSource, TResult> right)
      => new QueryExpressionAnd<TSource, TResult>(left, right: right);

    ///<inheritdoc/>
    public static QueryExpression<TSource, TResult> operator |(
         QueryExpression<TSource, TResult> left,
         QueryExpression<TSource, TResult> right)
        => new QueryExpressionOr<TSource, TResult>(left, right: right);

    ///<inheritdoc/>
    public static QueryExpression<TSource, TResult> operator !(
         QueryExpression<TSource, TResult> left)
        => new QueryExpressionNot<TSource, TResult>(expression: left);

    ///<inheritdoc/>
    public Expression<Func<TSource, TResult>> ToExpression() => GetExpression();

    ///<inheritdoc/>
    public Func<TSource, TResult> ToFunc() => GetExpression().Compile();
}

/// <summary>
/// This class is a helper that provides a default implementation 
/// for <see cref="IQueryExpression{TSource}"/> with <see cref="bool"/> as result.
/// </summary>
/// <typeparam name="TSource">The data source type.</typeparam>
public abstract record class QueryExpression<TSource>
    : QueryExpression<TSource, bool>, IQueryExpression<TSource>
{
    ///<inheritdoc/>
    public static implicit operator Expression<Func<TSource, bool>>(
          QueryExpression<TSource> queryExpression)
    {
        ArgumentNullException.ThrowIfNull(queryExpression);
        return queryExpression.GetExpression();
    }

    ///<inheritdoc/>
    public static implicit operator Func<TSource, bool>(
         QueryExpression<TSource> queryExpression)
    {
        ArgumentNullException.ThrowIfNull(queryExpression);
        return queryExpression.GetExpression().Compile();
    }

    ///<inheritdoc/>
    public static implicit operator QueryExpression<TSource>(
         Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return QueryExpressionFactory.Create(expression);
    }

    ///<inheritdoc/>
    public static QueryExpression<TSource> operator &(
         QueryExpression<TSource> left,
         QueryExpression<TSource> right)
      => new QueryExpressionAnd<TSource>(left, right: right);

    ///<inheritdoc/>
    public static QueryExpression<TSource> operator |(
         QueryExpression<TSource> left,
         QueryExpression<TSource> right)
        => new QueryExpressionOr<TSource>(left, right: right);

    ///<inheritdoc/>
    public static QueryExpression<TSource> operator !(
         QueryExpression<TSource> left)
        => new QueryExpressionNot<TSource>(expression: left);
}
