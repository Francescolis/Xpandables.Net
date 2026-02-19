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
using System.Linq.Expressions;
using System.Reflection;

namespace System.Collections.Generic;

/// <summary>
/// Provides utility methods for analyzing and manipulating LINQ queryable sequences with respect to Skip and Take
/// (pagination) operations.
/// </summary>
/// <remarks>Use this class to extract, remove, or normalize pagination operators from LINQ queries. These methods
/// are useful for scenarios such as inspecting paging parameters, removing pagination for total count queries, or
/// reapplying different paging logic. All methods require non-null queryable arguments and do not execute the queries;
/// they operate on the expression trees.</remarks>
public static class QueryPaginationNormalizer
{
    private static readonly MethodInfo SkipMethod = ((MethodCallExpression)
        ((Expression<Func<IQueryable<int>, IQueryable<int>>>)(q => q.Skip(0))).Body)
        .Method.GetGenericMethodDefinition();

    private static readonly MethodInfo TakeMethod = ((MethodCallExpression)
        ((Expression<Func<IQueryable<int>, IQueryable<int>>>)(q => q.Take(0))).Body)
        .Method.GetGenericMethodDefinition();

    /// <summary>
    /// Normalizes the specified queryable by extracting and removing any Skip and Take operations, returning the
    /// modified queryable and the corresponding skip and take values.
    /// </summary>
    /// <remarks>This method is useful for scenarios where you need to analyze or manipulate the paging
    /// operations (Skip/Take) applied to a queryable sequence. The returned queryable will have any Skip and Take
    /// operations removed, allowing further composition or inspection. The skip and take values are extracted from the
    /// original queryable, if present.</remarks>
    /// <typeparam name="T">The type of the elements in the source queryable.</typeparam>
    /// <param name="queryable">The source queryable to normalize. Cannot be null.</param>
    /// <returns>A tuple containing the normalized queryable, the skip value if present, and the take value if present. If no
    /// Skip or Take operations are found, the corresponding values will be null.</returns>
    public static (IQueryable<T> Queryable, int? Skip, int? Take) Normalize<T>(IQueryable<T> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        var visitor = new SkipTakeVisitor();
		Expression newExpression = visitor.Visit(queryable.Expression);
		IQueryable<T> newQueryable = queryable.Provider.CreateQuery<T>(newExpression);
        return (newQueryable, visitor.Skip, visitor.Take);
    }

    /// <summary>
    /// Extracts the values of the Skip and Take operations from the specified LINQ query, if present.
    /// </summary>
    /// <remarks>This method inspects the query expression tree to determine if Skip and Take operations have
    /// been applied. It does not execute the query or modify its behavior.</remarks>
    /// <typeparam name="T">The type of the elements in the source queryable.</typeparam>
    /// <param name="queryable">The LINQ queryable to analyze for Skip and Take operations. Cannot be null.</param>
    /// <returns>A tuple containing the number of elements to skip and take, respectively, as specified in the query. If a Skip
    /// or Take operation is not present, the corresponding value will be null.</returns>
    public static (int? Skip, int? Take) ExtractSkipTake<T>(IQueryable<T> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        var visitor = new SkipTakeVisitor();
        visitor.Visit(queryable.Expression);
        return (visitor.Skip, visitor.Take);
    }

    /// <summary>
    /// Returns a queryable sequence with any Skip or Take operations removed from the query expression.
    /// </summary>
    /// <remarks>Use this method to obtain the underlying query without pagination operators, which may be
    /// useful for scenarios such as counting total items or reapplying different paging logic.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="queryable">The source queryable sequence from which Skip and Take operations will be removed. Cannot be null.</param>
    /// <returns>An <see cref="IQueryable{T}"/> representing the original sequence without Skip or Take operations applied.</returns>
    public static IQueryable<T> RemoveSkipTake<T>(IQueryable<T> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        var remover = new SkipTakeVisitor();
		Expression newExpression = remover.Visit(queryable.Expression);
        return queryable.Provider.CreateQuery<T>(newExpression);
    }

    private sealed class SkipTakeVisitor : ExpressionVisitor
    {
        public int? Skip { get; private set; }
        public int? Take { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod)
            {
				MethodInfo genericDef = node.Method.GetGenericMethodDefinition();

                if (genericDef == SkipMethod)
                {
                    Skip = ExtractConstantInt(node.Arguments[1]) ?? Skip;
                    return Visit(node.Arguments[0]);
                }

                if (genericDef == TakeMethod)
                {
                    Take = ExtractConstantInt(node.Arguments[1]) ?? Take;
                    return Visit(node.Arguments[0]);
                }
            }

            return base.VisitMethodCall(node);
        }

        private static int? ExtractConstantInt(Expression expression) =>
            expression switch
            {
                ConstantExpression { Value: int value } => value,
                UnaryExpression
                {
                    NodeType: ExpressionType.Convert,
                    Operand: ConstantExpression { Value: int convertValue }
                } => convertValue,
                _ => null
            };

    }
}

