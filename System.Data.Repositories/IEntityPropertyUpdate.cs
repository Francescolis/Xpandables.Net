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

namespace System.Data.Repositories;

/// <summary>
/// Represents a property update expression for entity update operations.
/// </summary>
/// <typeparam name="TSource">The type of the entity being updated.</typeparam>
/// <remarks>This interface defines the contract for property update expressions that can be
/// applied to update operations. Implementations should encapsulate the logic for
/// applying specific property updates.</remarks>
public interface IEntityPropertyUpdate<TSource>
    where TSource : class
{
    /// <summary>
    /// Gets the expression that selects the property to update.
    /// </summary>
    LambdaExpression PropertyExpression { get; }

    /// <summary>
    /// Gets the expression or value that provides the new value for the property.
    /// </summary>
    Expression ValueExpression { get; }

    /// <summary>
    /// Gets the type of the property being updated.
    /// </summary>
    Type PropertyType { get; }

    /// <summary>
    /// Gets a value indicating whether the update uses a constant value.
    /// </summary>
    public bool IsConstant => ValueExpression is ConstantExpression;
}


/// <summary>
/// Represents a property update with a computed value expression.
/// </summary>
/// <typeparam name="TSource">The type of the entity being updated.</typeparam>
/// <typeparam name="TProperty">The type of the property being updated.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="PropertyUpdate{TSource, TProperty}"/> class.
/// </remarks>
/// <param name="propertyExpression">The expression that selects the property to update.</param>
/// <param name="valueExpression">The expression that computes the new value for the property.</param>
internal sealed class PropertyUpdate<TSource, TProperty>(
    Expression<Func<TSource, TProperty>> propertyExpression,
    Expression valueExpression) : IEntityPropertyUpdate<TSource>
    where TSource : class
{
    /// <inheritdoc />
    public LambdaExpression PropertyExpression => propertyExpression;

    /// <inheritdoc />
    public Expression ValueExpression => valueExpression;

    /// <inheritdoc />
    public Type PropertyType => typeof(TProperty);
}