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
using System.Reflection;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides static factory and combinator methods for creating and composing specifications based on boolean
/// expressions.
/// </summary>
/// <remarks>The Specification class enables the construction of reusable, composable business rules or predicates
/// using logical operations such as AND, OR, and NOT. Specifications can be created from expressions and combined to
/// form more complex criteria. This class is typically used as a base for building strongly-typed, testable filtering
/// logic in domain-driven design or query scenarios.</remarks>
public abstract record Specification
{
    /// <summary>
    /// Creates a specification from an expression.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A specification based on the expression.</returns>
    public static ISpecification<TSource> FromExpression<TSource>(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new Specification<TSource>(expression);
    }

    /// <summary>
    /// Converts a specification to an expression.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    /// <returns>The expression from the specification.</returns>
    public static Expression<Func<TSource, bool>> ToExpression<TSource>(Specification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.Expression;
    }

    /// <summary>
    /// Creates a specification that always returns true.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <returns>A specification that is always satisfied.</returns>
    public static ISpecification<TSource> True<TSource>()
    {
        return new Specification<TSource>(_ => true);
    }

    /// <summary>
    /// Creates a specification that always returns false.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <returns>A specification that is never satisfied.</returns>
    public static ISpecification<TSource> False<TSource>()
    {
        return new Specification<TSource>(_ => false);
    }

    /// <summary>
    /// Combines two specifications using logical AND (a &amp; b).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical AND.</returns>
    public static ISpecification<TSource> And<TSource>(ISpecification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return new Specification<TSource>(left, right, ExpressionType.And);
    }

    /// <summary>
    /// Combines two specifications using logical AND ALSO (a &amp;&amp; b).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical AND.</returns>
    public static ISpecification<TSource> AndAlso<TSource>(ISpecification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return new Specification<TSource>(left, right, ExpressionType.AndAlso);
    }

    /// <summary>
    /// Combines two specifications using logical OR (a | b).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical OR.</returns>
    public static ISpecification<TSource> Or<TSource>(ISpecification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return new Specification<TSource>(left, right, ExpressionType.Or);
    }

    /// <summary>
    /// Combines two specifications using logical OR ELSE (a || b).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical OR.</returns>
    public static ISpecification<TSource> OrElse<TSource>(ISpecification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return new Specification<TSource>(left, right, ExpressionType.OrElse);
    }
    /// <summary>
    /// Creates the logical negation of a specification (!a).
    /// </summary>
    /// <param name="specification">The specification to negate.</param>
    /// <returns>A new specification representing the logical NOT.</returns>
    public static ISpecification<TSource> Not<TSource>(ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return new Specification<TSource>(NotExpression(specification.Expression));
    }

    /// <summary>
    /// Creates a specification that combines multiple specifications using logical AND.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="specifications">The specifications to combine.</param>
    /// <returns>A new specification representing the logical AND of all specifications.</returns>
    public static ISpecification<TSource> All<TSource>(params ISpecification<TSource>[] specifications)
    {
        ArgumentNullException.ThrowIfNull(specifications);

        if (specifications.Length == 0)
		{
			return True<TSource>();
		}

		ISpecification<TSource> result = specifications[0];
        for (int i = 1; i < specifications.Length; i++)
        {
            result = result.And(specifications[i]);
        }
        return result;
    }

    /// <summary>
    /// Creates a specification that combines multiple specifications using logical OR.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="specifications">The specifications to combine.</param>
    /// <returns>A new specification representing the logical OR of all specifications.</returns>
    public static ISpecification<TSource> Any<TSource>(params ISpecification<TSource>[] specifications)
    {
        ArgumentNullException.ThrowIfNull(specifications);

        if (specifications.Length == 0)
		{
			return False<TSource>();
		}

		ISpecification<TSource> result = specifications[0];
        for (int i = 1; i < specifications.Length; i++)
        {
            result = result.Or(specifications[i]);
        }
        return result;
    }

    /// <summary>
    /// Creates a specification that checks if a value is equal to the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A new specification that checks equality.</returns>
    public static ISpecification<TSource> Equal<TSource, TValue>(Expression<Func<TSource, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression equality = Expression.Equal(selector.Body, Expression.Constant(value, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(equality, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a value is not equal to the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A new specification that checks inequality.</returns>
    public static ISpecification<TSource> NotEqual<TSource, TValue>(Expression<Func<TSource, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression inequality = Expression.NotEqual(selector.Body, Expression.Constant(value, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(inequality, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a value is null.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the value to check.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>A new specification that checks for null.</returns>
    public static ISpecification<TSource> IsNull<TSource, TValue>(Expression<Func<TSource, TValue?>> selector)
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression nullCheck = Expression.Equal(selector.Body, Expression.Constant(null, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(nullCheck, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a value is not null.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the value to check.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <returns>A new specification that checks for not null.</returns>
    public static ISpecification<TSource> IsNotNull<TSource, TValue>(Expression<Func<TSource, TValue?>> selector)
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression notNullCheck = Expression.NotEqual(selector.Body, Expression.Constant(null, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(notNullCheck, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a string contains the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="selector">The string property selector.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparisonType">The string comparison type.</param>
    /// <returns>A new specification that checks string containment.</returns>
    public static ISpecification<TSource> Contains<TSource>(
        Expression<Func<TSource, string>> selector,
        string value,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);

		ParameterExpression parameter = selector.Parameters[0];
		MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;
		MethodCallExpression containsCall = Expression.Call(selector.Body, containsMethod,
            Expression.Constant(value), Expression.Constant(comparisonType));
        var lambda = Expression.Lambda<Func<TSource, bool>>(containsCall, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a string starts with the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="selector">The string property selector.</param>
    /// <param name="value">The value to check for.</param>
    /// <param name="comparisonType">The string comparison type.</param>
    /// <returns>A new specification that checks string prefix.</returns>
    public static ISpecification<TSource> StartsWith<TSource>(
        Expression<Func<TSource, string>> selector,
        string value,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);

		ParameterExpression parameter = selector.Parameters[0];
		MethodInfo startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;
		MethodCallExpression startsWithCall = Expression.Call(selector.Body, startsWithMethod,
            Expression.Constant(value), Expression.Constant(comparisonType));
        var lambda = Expression.Lambda<Func<TSource, bool>>(startsWithCall, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a string ends with the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <param name="selector">The string property selector.</param>
    /// <param name="value">The value to check for.</param>
    /// <param name="comparisonType">The string comparison type.</param>
    /// <returns>A new specification that checks string suffix.</returns>
    public static ISpecification<TSource> EndsWith<TSource>(
        Expression<Func<TSource, string>> selector,
        string value,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);

		ParameterExpression parameter = selector.Parameters[0];
		MethodInfo endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;
		MethodCallExpression endsWithCall = Expression.Call(selector.Body, endsWithMethod,
            Expression.Constant(value), Expression.Constant(comparisonType));
        var lambda = Expression.Lambda<Func<TSource, bool>>(endsWithCall, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a comparable value is greater than the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the comparable value.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A new specification that checks greater than.</returns>
    public static ISpecification<TSource> GreaterThan<TSource, TValue>(Expression<Func<TSource, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression greaterThan = Expression.GreaterThan(selector.Body, Expression.Constant(value, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(greaterThan, parameter);
        return new Specification<TSource>(lambda);
    }

    /// <summary>
    /// Creates a specification that checks if a comparable value is less than the specified value.
    /// </summary>
    /// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
    /// <typeparam name="TValue">The type of the comparable value.</typeparam>
    /// <param name="selector">The property selector.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A new specification that checks less than.</returns>
    public static ISpecification<TSource> LessThan<TSource, TValue>(Expression<Func<TSource, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
		ParameterExpression parameter = selector.Parameters[0];
		BinaryExpression lessThan = Expression.LessThan(selector.Body, Expression.Constant(value, typeof(TValue)));
        var lambda = Expression.Lambda<Func<TSource, bool>>(lessThan, parameter);
        return new Specification<TSource>(lambda);
    }

    internal static Expression<Func<TSource, bool>> AndExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.And);

    internal static Expression<Func<TSource, bool>> AndAlsoExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.AndAlso);

    internal static Expression<Func<TSource, bool>> OrExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.Or);

    internal static Expression<Func<TSource, bool>> OrElseExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.OrElse);

    internal static Expression<Func<TSource, bool>> NotExpression<TSource>(
        Expression<Func<TSource, bool>> expression) =>
        CombineExpressions(expression, Expression.Not);

    internal static Expression<Func<TSource, bool>> CombineExpressions<TSource>(
        Expression<Func<TSource, bool>> expression,
        Func<Expression, UnaryExpression> combiner)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "param");
        Expression body = ReplaceParameter(expression.Body, expression.Parameters[0], parameter);
        return Expression.Lambda<Func<TSource, bool>>(
            combiner(body), parameter);
    }

    internal static Expression<Func<TSource, bool>> CombineExpressions<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "param");
        Expression leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        Expression rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
        return Expression.Lambda<Func<TSource, bool>>(
            combiner(leftBody, rightBody), parameter);
    }

    internal static Expression ReplaceParameter(
        Expression body,
        ParameterExpression toReplace,
        ParameterExpression replaceWith) =>
        new ParameterReplacer(toReplace, replaceWith).Visit(body);

    internal sealed class ParameterReplacer(
        ParameterExpression toReplace,
        ParameterExpression replaceWith) : ExpressionVisitor
    {
        private readonly ParameterExpression _toReplace = toReplace;
        private readonly ParameterExpression _replaceWith = replaceWith;

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _toReplace ? _replaceWith : base.VisitParameter(node);
    }
}

/// <summary>
/// Base abstract class providing common functionality for specifications.
/// </summary>
/// <typeparam name="TSource">The type of object to which the specification is applied.</typeparam>
public record Specification<TSource> : Specification, ISpecification<TSource>
{
    private readonly Lazy<Expression<Func<TSource, bool>>> _expression;

    /// <inheritdoc/>
    public Expression<Func<TSource, bool>> Expression => _expression.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TSource}"/> class
    /// with a default expression that always returns true.
    /// </summary>
    public Specification() => _expression = new Lazy<Expression<Func<TSource, bool>>>(_ => true);

    /// <summary>
    /// Initializes a new instance of the Specification class with the specified predicate expression.
    /// </summary>
    /// <param name="expression">An expression that defines the criteria used to evaluate objects of type TSource. Cannot be null.</param>
    public Specification(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        _expression = new Lazy<Expression<Func<TSource, bool>>>(expression);
    }

    /// <summary>
    /// Initializes a new instance of the Specification class by combining two specifications using the specified
    /// logical expression type.
    /// </summary>
    /// <param name="left">The left specification to be combined. Cannot be null.</param>
    /// <param name="right">The right specification to be combined. Cannot be null.</param>
    /// <param name="expressionType">The logical expression type used to combine the specifications. Typically ExpressionType.AndAlso or
    /// ExpressionType.OrElse.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
    public Specification(
         [Required] ISpecification<TSource> left,
         [Required] ISpecification<TSource> right,
         [Required] ExpressionType expressionType) : this(left.Expression, right.Expression, expressionType) { }

    /// <summary>
    /// Initializes a new instance of the Specification class that combines two predicate expressions using the
    /// specified logical operator.
    /// </summary>
    /// <param name="left">The first predicate expression to combine. Cannot be null.</param>
    /// <param name="right">The second predicate expression to combine. Cannot be null.</param>
    /// <param name="expressionType">The logical operator to use when combining the expressions. Must be one of ExpressionType.AndAlso,
    /// ExpressionType.OrElse, ExpressionType.And, or ExpressionType.Or.</param>
    /// <exception cref="InvalidOperationException">Thrown if expressionType is not a supported logical operator (AndAlso, OrElse, And, or Or).</exception>
    public Specification(
        [Required] Expression<Func<TSource, bool>> left,
        [Required] Expression<Func<TSource, bool>> right,
        [Required] ExpressionType expressionType) =>
        _expression = expressionType switch
        {
            ExpressionType.AndAlso => new Lazy<Expression<Func<TSource, bool>>>(AndAlsoExpression(left, right)),
            ExpressionType.OrElse => new Lazy<Expression<Func<TSource, bool>>>(OrElseExpression(left, right)),
            ExpressionType.And => new Lazy<Expression<Func<TSource, bool>>>(AndExpression(left, right)),
            ExpressionType.Or => new Lazy<Expression<Func<TSource, bool>>>(OrExpression(left, right)),
            _ => throw new InvalidOperationException($"Unsupported expression type : {expressionType}")
        };

    /// <summary>
    /// Initializes a new instance of the Specification class by combining an existing specification with an additional
    /// predicate using the specified logical operator.
    /// </summary>
    /// <param name="left">The existing specification to combine. Cannot be null.</param>
    /// <param name="right">An expression representing the predicate to combine with the specification. Cannot be null.</param>
    /// <param name="expressionType">The logical operator to use when combining the specification and the predicate. Must be a valid ExpressionType
    /// representing a logical operation, such as AndAlso or OrElse.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
    public Specification(
        [Required] ISpecification<TSource> left,
        [Required] Expression<Func<TSource, bool>> right,
        [Required] ExpressionType expressionType) : this(left.Expression, right, expressionType)
    { }

    /// <inheritdoc/>
    public bool IsSatisfiedBy(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Expression.Compile().Invoke(source);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Expression.GetHashCode();

    /// <summary>
    /// Combines this specification with another using logical AND.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the logical AND of both specifications.</returns>
    public ISpecification<TSource> And(ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return And(this, specification);
    }

    /// <summary>
    /// Combines the current specification with an additional predicate using a logical AND operation.
    /// </summary>
    /// <remarks>The resulting specification is satisfied only if both the current specification and the
    /// provided expression are satisfied for a given candidate.</remarks>
    /// <param name="expression">An expression that defines the additional condition to be combined with the current specification. Cannot be
    /// null.</param>
    /// <returns>A new specification that represents the logical AND of the current specification and the specified expression.</returns>
    public ISpecification<TSource> And(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new Specification<TSource>(this, expression, ExpressionType.AndAlso);
    }

    /// <summary>
    /// Combines this specification with another using logical AND ALSO.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the logical AND of both specifications.</returns>
    public ISpecification<TSource> AndAlso(ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return AndAlso(this, specification);
    }

    /// <summary>
    /// Combines this specification with an expression using logical AND ALSO.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the logical AND of both.</returns>
    public ISpecification<TSource> AndAlso(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new Specification<TSource>(this, expression, ExpressionType.AndAlso);
    }

    /// <summary> 
    /// Combines this specification with another using logical OR.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the logical OR of both specifications.</returns>
    public ISpecification<TSource> Or(ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return Or(this, specification);
    }

    /// <summary>
    /// Creates a new specification that represents the logical OR of the current specification and the provided
    /// expression.
    /// </summary>
    /// <param name="expression">An expression that defines the additional condition to combine with the current specification using a logical
    /// OR. Cannot be null.</param>
    /// <returns>A new specification that is satisfied when either the current specification or the provided expression is
    /// satisfied.</returns>
    public ISpecification<TSource> Or(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new Specification<TSource>(this, expression, ExpressionType.OrElse);
    }

    /// <summary>
    /// Combines this specification with another using logical OR with short-circuit evaluation.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification representing the logical OR with short-circuit evaluation.</returns>
    public ISpecification<TSource> OrElse(ISpecification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return OrElse(this, specification);
    }

    /// <summary>
    /// Combines this specification with another expression using logical OR with short-circuit evaluation.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the logical OR with short-circuit evaluation.</returns>
    public ISpecification<TSource> OrElse(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new Specification<TSource>(this, expression, ExpressionType.OrElse);
    }

    /// <summary>
    /// Creates the logical negation of this specification.
    /// </summary>
    /// <returns>A new specification representing the logical NOT of this specification.</returns>
    public ISpecification<TSource> Not() => Not(this);

#pragma warning disable CA2225 // Operator overloads have named alternates

    /// <summary>
    /// Explicitly converts a specification to an expression.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    public static explicit operator Expression<Func<TSource, bool>>(Specification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.Expression;
    }

    /// <summary>
    /// Explicitly converts a specification to a function.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    public static implicit operator Func<TSource, bool>(Specification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.Expression.Compile();
    }

    /// <summary>
    /// Implicitly converts an expression to a specification.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    public static implicit operator Specification<TSource>(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        return new(expression);
    }

    /// <summary>
    /// Implicitly converts a function to a specification.
    /// </summary>
    /// <param name="func">The function to convert.</param>
    public static implicit operator Specification<TSource>(Func<TSource, bool> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return new(x => func(x));
    }

    /// <summary>
    /// Combines two specifications using logical AND.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical AND.</returns>
    public static ISpecification<TSource> operator &(Specification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.And(right);
    }

    /// <summary>
    /// Combines two specifications using logical OR.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification representing the logical OR.</returns>
    public static ISpecification<TSource> operator |(Specification<TSource> left, ISpecification<TSource> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Or(right);
    }

    /// <summary>
    /// Creates the logical negation of a specification.
    /// </summary>
    /// <param name="specification">The specification to negate.</param>
    /// <returns>A new specification representing the logical NOT.</returns>
    public static ISpecification<TSource> operator !(Specification<TSource> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return specification.Not();
    }
}