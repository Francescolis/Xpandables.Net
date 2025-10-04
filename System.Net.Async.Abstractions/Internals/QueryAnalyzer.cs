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
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Net.Async;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xpandables.Net.Async.Internals;

/// <summary>
/// Provides optimized query analysis utilities for extracting pagination information.
/// </summary>
public static class QueryAnalyzer
{
    private static readonly ConcurrentDictionary<Expression, (int? Skip, int? Take)> s_skipTakeCache = new();
    private static readonly MethodInfo s_skipMethod;
    private static readonly MethodInfo s_takeMethod;

#pragma warning disable CA1810 // Initialize reference type static fields inline
    static QueryAnalyzer()
#pragma warning restore CA1810 // Initialize reference type static fields inline
    {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        MethodInfo[] queryableMethods = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        s_skipMethod = queryableMethods.First(m => m.Name == nameof(Queryable.Skip) && m.GetParameters().Length == 2);
        s_takeMethod = queryableMethods.First(m => m.Name == nameof(Queryable.Take) && m.GetParameters().Length == 2);
    }

    /// <summary>
    /// Extracts Skip and Take values from a query expression with caching.
    /// </summary>
    /// <param name="expression">The query expression.</param>
    /// <returns>Skip and Take values, or null if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int? Skip, int? Take) ExtractSkipTake(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        // Try cache first for hot paths
        if (s_skipTakeCache.TryGetValue(expression, out (int? Skip, int? Take) cached))
        {
            return cached;
        }

        // Simple recursive extraction for most cases
        (int? Skip, int? Take) result = ExtractSkipTakeSimple(expression);

        // Cache the result if the expression is cacheable
        if (IsCacheable(expression))
        {
            _ = s_skipTakeCache.TryAdd(expression, result);
        }

        return result;
    }

    /// <summary>
    /// Removes Skip and Take operations from a query to get the base query for counting.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="query">Original query.</param>
    /// <returns>Query without Skip/Take operations.</returns>
    public static IQueryable<T> RemoveSkipTake<T>(IQueryable<T> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        Expression newExpression = RemoveSkipTakeFromExpression(query.Expression);
        return newExpression == query.Expression ? query : query.Provider.CreateQuery<T>(newExpression);
    }

    /// <summary>
    /// Simple recursive extraction for linear expression chains.
    /// </summary>
    private static (int? Skip, int? Take) ExtractSkipTakeSimple(Expression expression)
    {
        int? skip = null;
        int? take = null;

        Expression current = expression;
        while (current is MethodCallExpression methodCall)
        {
            if (IsSkipMethod(methodCall.Method))
            {
                skip ??= ExtractConstantValue(methodCall.Arguments[1]);
            }
            else if (IsTakeMethod(methodCall.Method))
            {
                take ??= ExtractConstantValue(methodCall.Arguments[1]);
            }

            current = methodCall.Arguments[0];
        }

        return (skip, take);
    }

    private static Expression RemoveSkipTakeFromExpression(Expression expression)
    {
        if (expression is not MethodCallExpression methodCall)
        {
            return expression;
        }

        if (IsSkipMethod(methodCall.Method) || IsTakeMethod(methodCall.Method))
        {
            return RemoveSkipTakeFromExpression(methodCall.Arguments[0]);
        }

        Expression[] newArguments = [.. methodCall.Arguments];
        newArguments[0] = RemoveSkipTakeFromExpression(methodCall.Arguments[0]);

        return Expression.Call(methodCall.Object, methodCall.Method, newArguments);
    }

    private static bool IsCacheable(Expression expression) =>
        // Only cache relatively simple expressions to avoid memory bloat
        GetComplexity(expression) <= 10;

    private static int GetComplexity(Expression expression)
    {
        return expression switch
        {
            MethodCallExpression method => 1 + (method.Arguments.Count > 0 ? GetComplexity(method.Arguments[0]) : 0),
            _ => 1
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSkipMethod(MethodInfo method) =>
        method.IsGenericMethod && method.GetGenericMethodDefinition() == s_skipMethod;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTakeMethod(MethodInfo method) =>
        method.IsGenericMethod && method.GetGenericMethodDefinition() == s_takeMethod;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int? ExtractConstantValue(Expression expression)
    {
        return expression switch
        {
            ConstantExpression { Value: int value } => value,
            UnaryExpression { NodeType: ExpressionType.Convert, Operand: ConstantExpression { Value: int convertValue } } => convertValue,
            _ => null
        };
    }
}