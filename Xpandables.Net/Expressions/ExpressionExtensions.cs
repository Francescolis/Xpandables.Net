
/************************************************************************************************************
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
************************************************************************************************************/
using System.Linq.Expressions;

namespace Xpandables.Net.Expressions;

/// <summary>
/// Provides with <see cref="Expression{TDelegate}"/> extensions.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Gets a set of Expressions representing the parameters which will be passed to the constructor.
    /// </summary>
    /// <param name="parameterTypes">A collection of type to be used to build parameter expressions</param>
    /// <exception cref="ArgumentNullException">The <paramref name="parameterTypes"/> is null.</exception>
    public static ParameterExpression[] GetParameterExpression(params Type[] parameterTypes)
    {
        _ = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));

        return parameterTypes
            .Select((type, index) => Expression.Parameter(type, $"param{index + 1}"))
            .ToArray();
    }

    /// <summary>
    /// Returns the member name from the expression.
    /// The expression delegate is <see langword="nameof"/>, otherwise the result is null.
    /// </summary>
    /// <typeparam name="T">The type of the target class.</typeparam>
    /// <param name="nameOfExpression">The expression delegate for the property : <see langword="nameof"/>
    /// with delegate expected.</param>
    /// <returns>A string that represents the name of the member found in the expression.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="nameOfExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="nameOfExpression"/> is
    /// not a <see cref="ConstantExpression"/>.</exception>
    public static string GetMemberNameFromExpression<T>(this Expression<Func<T, string>> nameOfExpression)
        where T : class
    {
        _ = nameOfExpression ?? throw new ArgumentNullException(nameof(nameOfExpression));

        return nameOfExpression.Body is ConstantExpression constantExpression
            ? constantExpression.Value?.ToString() ?? throw new ArgumentException("The member expression is null.")
            : throw new ArgumentException("A member expression is expected.");
    }

    /// <summary>
    /// Returns the member name from the expression if found, otherwise returns null.
    /// </summary>
    /// <typeparam name="T">The type of the target class.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertyExpression">The expression that contains the member name.</param>
    /// <returns>A string that represents the name of the member found in the expression.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="propertyExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="propertyExpression"/> is not a member expression."</exception>
    public static string GetMemberNameFromExpression<T, TProperty>(this Expression<Func<T, TProperty>> propertyExpression)
        where T : class
    {
        _ = propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression));

        return (propertyExpression.Body as MemberExpression
            ?? ((UnaryExpression)propertyExpression.Body).Operand as MemberExpression)
            ?.Member.Name ??
            throw new ArgumentException("A member expression is expected.");
    }

}
