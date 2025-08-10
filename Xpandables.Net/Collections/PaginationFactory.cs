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
using System.Reflection;

namespace Xpandables.Net.Collections;
/// <summary>
/// Contains information extracted from a query expression for pagination analysis.
/// </summary>
/// <param name="Skip">The number of items to skip.</param>
/// <param name="Take">The number of items to take.</param>
/// <param name="QueryWithoutPagination">The original query with Skip/Take methods removed.</param>
internal readonly record struct PaginationExtractionResult<TResult>(
    int? Skip,
    int? Take,
    IQueryable<TResult> QueryWithoutPagination);

/// <summary>
/// Analyzes LINQ expressions to extract Skip and Take operations.
/// </summary>
internal static class PaginationExpressionAnalyzer
{
    private static readonly MethodInfo SkipMethod = typeof(Queryable)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == nameof(Queryable.Skip) && m.GetParameters().Length == 2);

    private static readonly MethodInfo TakeMethod = typeof(Queryable)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == nameof(Queryable.Take) && m.GetParameters().Length == 2);

    /// <summary>
    /// Extracts Skip and Take values from a query and returns a version without pagination.
    /// </summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    /// <param name="query">The query to analyze.</param>
    /// <returns>Pagination extraction result containing Skip/Take values and unpaginated query.</returns>
    public static PaginationExtractionResult<TResult> ExtractPagination<TResult>(IQueryable<TResult> query)
    {
        var visitor = new PaginationExtractionVisitor();
        var modifiedExpression = visitor.Visit(query.Expression);

        var queryWithoutPagination = query.Provider.CreateQuery<TResult>(modifiedExpression);

        return new PaginationExtractionResult<TResult>(
            visitor.Skip,
            visitor.Take,
            queryWithoutPagination);
    }

    /// <summary>
    /// Expression visitor that extracts and removes Skip/Take method calls.
    /// </summary>
    private sealed class PaginationExtractionVisitor : ExpressionVisitor
    {
        public int? Skip { get; private set; }
        public int? Take { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod &&
                node.Method.GetGenericMethodDefinition() == SkipMethod.GetGenericMethodDefinition())
            {
                Skip = ExtractConstantValue(node.Arguments[1]);
                return Visit(node.Arguments[0]);
            }

            if (node.Method.IsGenericMethod &&
                node.Method.GetGenericMethodDefinition() == TakeMethod.GetGenericMethodDefinition())
            {
                Take = ExtractConstantValue(node.Arguments[1]);
                return Visit(node.Arguments[0]);
            }

            return base.VisitMethodCall(node);
        }

        private static int? ExtractConstantValue(Expression expression) =>
            expression switch
            {
                ConstantExpression constant => constant.Value as int?,
                UnaryExpression { NodeType: ExpressionType.Convert } unary
                    when unary.Operand is ConstantExpression constantOperand => constantOperand.Value as int?,
                MemberExpression member => ExtractMemberValue(member),
                _ => null
            };

        private static int? ExtractMemberValue(MemberExpression member)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                // Handle cases like local variables or properties
                if (member.Expression is ConstantExpression constant)
                {
                    var value = member.Member switch
                    {
                        FieldInfo field => field.GetValue(constant.Value),
                        PropertyInfo property => property.GetValue(constant.Value),
                        _ => null
                    };
                    return value as int?;
                }
            }
            catch
            {
                // If we can't extract the value, return null
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return null;
        }
    }
}
