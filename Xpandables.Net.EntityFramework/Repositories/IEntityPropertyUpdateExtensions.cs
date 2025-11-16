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
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides extension methods for converting collections of entity property update expressions to the format required
/// by Entity Framework Core's ExecuteUpdate and related APIs.
/// </summary>
/// <remarks>Updated for EF Core 10 : uses <see cref="UpdateSettersBuilder{TSource}"/> and returns
/// <see cref="Action{T}"/> delegates compatible with ExecuteUpdate/ExecuteUpdateAsync.</remarks>
public static class IEntityPropertyUpdateExtensions
{
    private static class MethodCache<TSource>
        where TSource : class
    {
        // Capture the two UpdateSettersBuilder<T>.SetProperty overloads via expressions
        // to obtain generic method definitions without name-based reflection.

        // s => s.SetProperty((Expression<Func<TSrc, TProp>>)null!, (Expression<Func<TSrc, TProp>>)null!)
        private static Expression<Func<UpdateSettersBuilder<TSrc>, UpdateSettersBuilder<TSrc>>> CaptureComputed<TSrc, TProp>()
            where TSrc : class
            => s => s.SetProperty((Expression<Func<TSrc, TProp>>)null!, (Expression<Func<TSrc, TProp>>)null!);

        // s => s.SetProperty((Expression<Func<TSrc, TProp>>)null!, default(TProp)!)
        private static Expression<Func<UpdateSettersBuilder<TSrc>, UpdateSettersBuilder<TSrc>>> CaptureConstant<TSrc, TProp>()
            where TSrc : class
            => s => s.SetProperty((Expression<Func<TSrc, TProp>>)null!, default(TProp)!);

        public static readonly MethodInfo SetPropertyComputedOpenGeneric =
            ((MethodCallExpression)CaptureComputed<TSource, int>().Body).Method.GetGenericMethodDefinition();

        public static readonly MethodInfo SetPropertyConstantOpenGeneric =
            ((MethodCallExpression)CaptureConstant<TSource, int>().Body).Method.GetGenericMethodDefinition();
    }

    /// <summary>
    /// <see cref="IEntityPropertyUpdate{TSource}"/> extensions.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity being updated.</typeparam>
    extension<TSource>(IEnumerable<IEntityPropertyUpdate<TSource>> updates)
        where TSource : class
    {
        /// <summary>
        /// Builds an <see cref="Action{T}"/> that applies all configured property updates to a given
        /// <see cref="UpdateSettersBuilder{TSource}"/> instance.
        /// </summary>
        /// <remarks>This method generates and compiles a delegate compatible with EF Core 10 RC1
        /// ExecuteUpdate/ExecuteUpdateAsync.</remarks>
        /// <returns>An <see cref="Action{T}"/> taking <see cref="UpdateSettersBuilder{TSource}"/> that applies all updates.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a property update contains a <c>PropertyExpression</c> that is not a <see
        /// cref="LambdaExpression"/> or has an unexpected parameter signature.</exception>
        [RequiresDynamicCode("Dynamic code generation is required for this method.")]
        [RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
        public Action<UpdateSettersBuilder<TSource>> ToSetPropertyCalls()
        {
            ArgumentNullException.ThrowIfNull(updates);

            var list = updates as IReadOnlyList<IEntityPropertyUpdate<TSource>> ?? [.. updates];
            if (list.Count == 0)
            {
                return static _ => { };
            }

            var builderParam = Expression.Parameter(typeof(UpdateSettersBuilder<TSource>), "s");
            var calls = new List<Expression>(capacity: list.Count + 1);

            foreach (var update in list)
            {
                ArgumentNullException.ThrowIfNull(update);

                if (update.PropertyExpression is not LambdaExpression propertyLambda)
                    throw new InvalidOperationException("PropertyExpression must be a LambdaExpression.");

                var propertyType = update.PropertyType;
                var expectedPropLambdaType = typeof(Func<,>).MakeGenericType(typeof(TSource), propertyType);
                var propParam = EnsureSingleParameter(propertyLambda, typeof(TSource));

                var rebuiltPropLambda = Expression.Lambda(
                    expectedPropLambdaType,
                    Expression.Convert(propertyLambda.Body, propertyType),
                    propParam);

                if (update.ValueExpression is LambdaExpression valueAsLambda)
                {
                    // Computed value: SetProperty(Expression<Func<TSource, TProp>>, Expression<Func<TSource, TProp>>)
                    var valueParam = EnsureSingleParameter(valueAsLambda, typeof(TSource));
                    var retargetedBody = new ParameterReplaceVisitor(valueParam, propParam).Visit(valueAsLambda.Body)!;
                    var valueLambda = Expression.Lambda(
                        expectedPropLambdaType,
                        Expression.Convert(retargetedBody, propertyType),
                        propParam);

                    var computedMethod = MethodCache<TSource>.SetPropertyComputedOpenGeneric.MakeGenericMethod(propertyType);
                    calls.Add(
                        Expression.Call(
                            instance: builderParam,
                            method: computedMethod,
                            arguments:
                            [
                                Expression.Quote(rebuiltPropLambda),
                                Expression.Quote(valueLambda)
                            ]));
                }
                else
                {
                    // Constant value: SetProperty(Expression<Func<TSource, TProp>>, TProp)
                    var constantBody = Expression.Convert(update.ValueExpression, propertyType);
                    var constantMethod = MethodCache<TSource>.SetPropertyConstantOpenGeneric.MakeGenericMethod(propertyType);

                    calls.Add(
                        Expression.Call(
                            instance: builderParam,
                            method: constantMethod,
                            arguments:
                            [
                                Expression.Quote(rebuiltPropLambda),
                                constantBody
                            ]));
                }
            }

            // Ensure void-return block
            calls.Add(Expression.Empty());
            var block = Expression.Block(calls);

            var lambda = Expression.Lambda<Action<UpdateSettersBuilder<TSource>>>(block, builderParam);
            return lambda.Compile();
        }
    }

    private static ParameterExpression EnsureSingleParameter(LambdaExpression lambda, Type expectedParamType)
    {
        if (lambda.Parameters.Count != 1)
            throw new InvalidOperationException("Lambda must have exactly one parameter.");

        var p = lambda.Parameters[0];
        if (p.Type != expectedParamType)
            throw new InvalidOperationException("Lambda parameter type mismatch.");

        return p;
    }

    private sealed class ParameterReplaceVisitor(Expression oldParam, Expression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => ReferenceEquals(node, oldParam) ? newParam : base.VisitParameter(node);
    }
}
