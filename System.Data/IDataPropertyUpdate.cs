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

namespace System.Data;

/// <summary>
/// Represents a property update expression for data update operations.
/// </summary>
/// <typeparam name="TData">The type of the data being updated.</typeparam>
public interface IDataPropertyUpdate<TData>
    where TData : class
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
    bool IsConstant { get; }

    /// <summary>
    /// Applies this update using the provided applicator.
    /// This enables AOT-compliant polymorphic dispatch without reflection.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder (e.g., UpdateSettersBuilder).</typeparam>
    /// <param name="builder">The builder instance to apply the update to.</param>
    /// <param name="applicator">The applicator that knows how to apply updates to the builder.</param>
    void Apply<TBuilder>(TBuilder builder, IPropertyUpdateApplicator<TData, TBuilder> applicator);
}

/// <summary>
/// Defines how to apply property updates to a specific builder type.
/// Implement this interface in the data layer for specific ORMs.
/// </summary>
/// <typeparam name="TData">The type of the data being updated.</typeparam>
/// <typeparam name="TBuilder">The type of the builder (e.g., UpdateSettersBuilder).</typeparam>
public interface IPropertyUpdateApplicator<TData, TBuilder>
    where TData : class
{
    /// <summary>
    /// Applies a computed property update (property selector + value expression).
    /// </summary>
    void ApplyComputed<TProperty>(
        TBuilder builder,
        Expression<Func<TData, TProperty>> propertyExpression,
        Expression<Func<TData, TProperty>> valueExpression);

    /// <summary>
    /// Applies a constant property update (property selector + constant value).
    /// </summary>
    void ApplyConstant<TProperty>(
        TBuilder builder,
        Expression<Func<TData, TProperty>> propertyExpression,
        TProperty value);
}

/// <summary>
/// Represents a property update with a computed value expression.
/// </summary>
internal sealed class ComputedPropertyUpdate<TData, TProperty>(
    Expression<Func<TData, TProperty>> propertyExpression,
    Expression<Func<TData, TProperty>> valueExpression) : IDataPropertyUpdate<TData>
    where TData : class
{
    public LambdaExpression PropertyExpression => propertyExpression;
    public Expression ValueExpression => valueExpression;
    public Type PropertyType => typeof(TProperty);
    public bool IsConstant => false;

    public void Apply<TBuilder>(TBuilder builder, IPropertyUpdateApplicator<TData, TBuilder> applicator)
        => applicator.ApplyComputed(builder, propertyExpression, valueExpression);
}

/// <summary>
/// Represents a property update with a constant value.
/// </summary>
internal sealed class ConstantPropertyUpdate<TData, TProperty>(
    Expression<Func<TData, TProperty>> propertyExpression,
    TProperty value) : IDataPropertyUpdate<TData>
    where TData : class
{
    private readonly ConstantExpression _valueExpression = Expression.Constant(value, typeof(TProperty));

    public LambdaExpression PropertyExpression => propertyExpression;
    public Expression ValueExpression => _valueExpression;
    public Type PropertyType => typeof(TProperty);
    public bool IsConstant => true;

    public void Apply<TBuilder>(TBuilder builder, IPropertyUpdateApplicator<TData, TBuilder> applicator)
        => applicator.ApplyConstant(builder, propertyExpression, value);
}